using Grpc.Core;
using System.Collections.Concurrent;
using UpFlux_WebService.Protos;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Core.Models;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Upflux_WebService.Services.Interfaces;
using static Upflux_WebService.Services.EntityQueryService;
using System.Text.RegularExpressions;
using Upflux_WebService.Repository;


namespace UpFlux_WebService
{
	/// <summary>
	/// Unified gRPC service that handles all operations (License, Commands, Logs, Monitoring, Alerts)
	/// via a single persistent streaming method (OpenControlChannel).
	/// </summary>
	public class ControlChannelService : ControlChannel.ControlChannelBase, IControlChannelService
	{
		private readonly ILogger<ControlChannelService> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly string _logDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");

		private readonly ConcurrentDictionary<string, IServerStreamWriter<ControlMessage>> _connectedGateways = new();
		private readonly ConcurrentDictionary<string, CommandMetadata> _commandIdToMetadataMap = new();
		private readonly ConcurrentDictionary<string, UpdateMetadata> _updateMetadataMap = new();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="serviceScopeFactory"></param>
		/// <param name="configuration"></param>
		public ControlChannelService(ILogger<ControlChannelService> logger, IServiceScopeFactory serviceScopeFactory,
			IConfiguration configuration)
		{
			_logger = logger;
			_serviceScopeFactory = serviceScopeFactory;
			_logDirectoryPath = configuration["Logging:MachineLogsDirectory"]!;
		}

		public override async Task OpenControlChannel(
			IAsyncStreamReader<ControlMessage> requestStream,
			IServerStreamWriter<ControlMessage> responseStream,
			ServerCallContext context)
		{
			var gatewayId = "UNKNOWN";
			try
			{
				// Expect the first message to identify the gateway
				if (!await requestStream.MoveNext())
				{
					_logger.LogWarning("No initial message from gateway; closing channel.");
					return;
				}

				var firstMsg = requestStream.Current;
				gatewayId = firstMsg.SenderId ?? "UNKNOWN";
				_connectedGateways[gatewayId] = responseStream;

				_logger.LogInformation("Gateway [{0}] connected to ControlChannel.", gatewayId);

				// Optionally handle the first message if it has a payload
				await HandleIncomingMessage(gatewayId, firstMsg);

				// Continue reading messages until the gateway disconnects
				while (await requestStream.MoveNext()) await HandleIncomingMessage(gatewayId, requestStream.Current);

				_logger.LogInformation("Gateway [{0}] disconnected.", gatewayId);
			}
			finally
			{
				_connectedGateways.TryRemove(gatewayId, out _);
			}
		}

		#region private methods

		private async Task HandleIncomingMessage(string gatewayId, ControlMessage msg)
		{
			switch (msg.PayloadCase)
			{
				case ControlMessage.PayloadOneofCase.LicenseRequest:
					await HandleLicenseRequest(gatewayId, msg.LicenseRequest);
					break;
				case ControlMessage.PayloadOneofCase.LogUpload:
					await HandleLogUploadAsync(gatewayId, msg.LogUpload);
					break;
				case ControlMessage.PayloadOneofCase.MonitoringData:
					await HandleMonitoringDataAsync(gatewayId, msg.MonitoringData);
					break;
				case ControlMessage.PayloadOneofCase.AlertMessage:
					await HandleAlertMessageAsync(gatewayId, msg.AlertMessage);
					break;
				case ControlMessage.PayloadOneofCase.CommandResponse:
					await HandleCommandResponse(gatewayId, msg.CommandResponse);
					break;
				case ControlMessage.PayloadOneofCase.UpdateAck:
					await HandleUpdateAcknowledged(gatewayId, msg.UpdateAck);
					break;
				case ControlMessage.PayloadOneofCase.LogResponse:
					_logger.LogInformation("Gateway [{0}] responded to log request => success={1}, msg={2}",
						gatewayId, msg.LogResponse.Success, msg.LogResponse.Message);
					break;
				case ControlMessage.PayloadOneofCase.VersionDataResponse:
					await HandleVersionDataResponse(gatewayId, msg.VersionDataResponse);
					break;
				default:
					_logger.LogWarning("Received unknown message from [{0}] => {1}", gatewayId, msg.PayloadCase);
					break;
			}
		}

		private async Task HandleLicenseRequest(string gatewayId, LicenseRequest req)
		{
			_logger.LogInformation(
				"Handling license request for Gateway ID: {GatewayId}, IsRenewal: {IsRenewal}, Device UUID: {DeviceUuid}",
				gatewayId, req.IsRenewal, req.DeviceUuid);

			try
			{
				if (!req.IsRenewal)
					await AddUnregisteredDevice(gatewayId, req.DeviceUuid);
				else
					await ProcessRenewalRequest(gatewayId, req.DeviceUuid);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while handling license request for Device UUID: {DeviceUuid}",
					req.DeviceUuid);
				await SendControlMessageAsync(gatewayId, "An error occurred while processing the license request.");
			}
		}

		private async Task AddUnregisteredDevice(string gatewayId, string deviceUuid)
		{
			_logger.LogInformation("Processing new license request for Device UUID: {DeviceUuid}", deviceUuid);

			using var scope = _serviceScopeFactory.CreateScope();
			var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
			var generatedMachineIdRepository =
				scope.ServiceProvider.GetRequiredService<IGeneratedMachineIdRepository>();
			var entityQueryService = scope.ServiceProvider.GetRequiredService<IEntityQueryService>();

			var generatedId = await generatedMachineIdRepository.GetByMachineId(deviceUuid);
			if (generatedId is null)
			{
				_logger.LogWarning("Received communication attempt from unknown Machine ID: {DeviceUuid}", deviceUuid);
				await SendControlMessageAsync(gatewayId, $"Unknown Machine ID: {deviceUuid}. Request denied.");
				return;
			}

			_logger.LogInformation("Validated Machine ID: {DeviceUuid} with Generated Machine ID Repository",
				deviceUuid);

			if (await machineRepository.GetByIdAsync(deviceUuid) != null)
			{
				_logger.LogInformation("Machine ID: {DeviceUuid} already exists in the repository. No action required.",
					deviceUuid);
				await SendControlMessageAsync(gatewayId, $"Machine ID: {deviceUuid} already exists in the repository.");
				return;
			}

			Machine newMachine = new()
			{
				MachineId = deviceUuid,
				dateAddedOn = DateTime.UtcNow,
				machineName = entityQueryService.GenerateUserId(DbGenerateId.MACHINE),
				ipAddress = "NA"
			};

			_logger.LogInformation("Adding new Machine record for Device UUID: {DeviceUuid}", deviceUuid);

			try
			{
				await machineRepository.AddAsync(newMachine);
				await machineRepository.SaveChangesAsync();

				await SendControlMessageAsync(gatewayId,
					$"Successfully added new Machine record for Device UUID: {deviceUuid}.");
				_logger.LogInformation("Successfully added new Machine record for Device UUID: {DeviceUuid}",
					deviceUuid);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to add or save new Machine record for Device UUID: {DeviceUuid}",
					deviceUuid);
				throw;
			}
		}

		private async Task ProcessRenewalRequest(string gatewayId, string deviceUuid)
		{
			_logger.LogInformation("Processing license renewal request for Device UUID: {DeviceUuid}", deviceUuid);
			using var scope = _serviceScopeFactory.CreateScope();
			var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

			// send signalR notification for and expired licence 
			await notificationService.SendMessageToUriAsync("Alert/Licence",
				$" Device :{deviceUuid} has an expired licence and is requesting renewal");

			await SendControlMessageAsync(gatewayId,
				$"License renewal request received for Device UUID: {deviceUuid}. Further action required.");
			_logger.LogInformation("Notification sent for license renewal request for Device UUID: {DeviceUuid}",
				deviceUuid);
		}

		private async Task SendControlMessageAsync(string gatewayId, string description)
		{
			if (_connectedGateways.TryGetValue(gatewayId, out var writer))
			{
				var outMsg = new ControlMessage
				{
					SenderId = "Cloud",
					Description = description
				};

				await writer.WriteAsync(outMsg);
				_logger.LogInformation("Notification sent to Gateway [{0}]: {1}", gatewayId, description);
			}
			else
			{
				_logger.LogWarning("Unable to send notification. Gateway [{0}] is not connected.", gatewayId);
			}
		}

		private async Task HandleLogUploadAsync(string gatewayId, LogUpload upload)
		{
			try
			{
				_logger.LogInformation("Received LogUpload from device={0} at gateway=[{1}], file={2}, size={3} bytes",
					upload.DeviceUuid, gatewayId, upload.FileName, upload.Data.Length);

				Directory.CreateDirectory(_logDirectoryPath);
				var filePath = Path.Combine(_logDirectoryPath, upload.FileName);
				await File.WriteAllBytesAsync(filePath, upload.Data.ToByteArray());

				_logger.LogInformation("Log saved to: {0}", filePath);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error handling log upload for device={0} at gateway=[{1}]", upload.DeviceUuid,
					gatewayId);
				throw;
			}
		}

		private async Task HandleMonitoringDataAsync(string gatewayId, MonitoringDataMessage mon)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

			foreach (var aggregatedData in mon.AggregatedData)
			{
				_logger.LogInformation("Monitoring from dev={0} (gw={1}): CPU={2}%, MEM={3}%",
					aggregatedData.Uuid, gatewayId, aggregatedData.Metrics.CpuUsage,
					aggregatedData.Metrics.MemoryUsage);

				try
				{
					await notificationService.SendMessageToUriAsync(aggregatedData.Uuid, aggregatedData.ToString());
					_logger.LogInformation("Successfully sent data for MachineId {Uuid}.", aggregatedData.Uuid);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to send data for MachineId {Uuid}.", aggregatedData.Uuid);
				}
			}
		}

		private async Task HandleAlertMessageAsync(string gatewayId, AlertMessage alert)
		{
			_logger.LogInformation("ALERT from gw={0}, dev={1}, level={2}, msg={3}",
				gatewayId, alert.Source, alert.Level, alert.Message);

			// notification
			using var scope = _serviceScopeFactory.CreateScope();
			var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
			await notificationService.SendMessageToUriAsync($"alert", alert.ToString());

			// Send an alertResponse back
			if (_connectedGateways.TryGetValue(gatewayId, out var writer))
			{
				var responseMsg = new ControlMessage
				{
					SenderId = "Cloud",
					Description = "Alert received by the cloud.",
					AlertResponse = new AlertResponseMessage
					{
						Success = true,
						Message = "Cloud: alert received"
					}
				};
				await writer.WriteAsync(responseMsg);
			}
		}

		private async Task HandleCommandResponse(string gatewayId, CommandResponse req)
		{
			_logger.LogInformation("Gateway [{0}] responded to command: {1}", gatewayId, req.CommandId);

			if (_commandIdToMetadataMap.TryGetValue(req.CommandId, out var metadata))
			{
				_logger.LogInformation("Processing CommandResponse for CommandId: {0}, Parameters: {1}", req.CommandId,
					metadata.Parameters);

				foreach (var machineId in metadata.MachineIds)
					if (metadata.CommandType == CommandType.Rollback)
						await ProcessMachineRollbackResponse(machineId, metadata.Parameters, req);

				_commandIdToMetadataMap.TryRemove(req.CommandId, out _);

				_logger.LogInformation("Successfully handled and removed CommandId: {0} from metadata map.",
					req.CommandId);
			}
			else
			{
				_logger.LogWarning("No metadata found for CommandId: {0}. Unable to process CommandResponse.",
					req.CommandId);
			}
		}

		// TODO: use getVersionn Data to update database instead of doing it manually. do it in forntend maybe?
		/// <summary>
		/// update application database if rollback is succesful
		/// </summary>
		private async Task ProcessMachineRollbackResponse(string machineId, string parameters, CommandResponse req)
		{
			// TODO: need to keep track the user who initiated rollback (since it affects application table) - not done
			// might want to add engineer email parameter at SendUpdatePackage (used by controller), update interface, and update controller

			var alert = new AlertMessage();
			using var scope = _serviceScopeFactory.CreateScope();
			var applicationRepository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
			var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

			try
			{
				//var application = await applicationRepository.GetByMachineId(machineId);
				//if (application == null)
				//{
				//	alert.Message = $"Failed to process rollback for MachineId: {machineId}. No application found.";
				//	_logger.LogWarning("No application found for MachineId: {0}. Skipping processing.", machineId);
				//	await notificationService.SendMessageToUriAsync("alert", alert.ToString());
				//	return;
				//}

				// in case of partial success
				var (succeededDevices, failedDevices) = string.IsNullOrWhiteSpace(req.Details)
						? (new List<string>(), new List<string>())
						: ParseDeviceDetails(req.Details);

				bool isSuccess = req.Success || succeededDevices.Contains(machineId);

				if (isSuccess)
				{

					var application = await applicationRepository.GetByMachineId(machineId);
					if (application is null)
					{
						// add
					}

					await HandleSuccessfulRollback(machineId, parameters, req, application, applicationRepository, notificationService, alert);
				}
				else
				{
					await HandleFailedRollback(machineId, req, notificationService, alert);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while processing rollback for MachineId: {0}, CommandId: {1}.", machineId, req.CommandId);
				alert.Message = $"An error occurred while processing rollback for MachineId: {machineId}. CommandId: {req.CommandId}. Error: {ex.Message}";
				await notificationService.SendMessageToUriAsync("alert", alert.ToString());
			}
		}

		private async Task HandleSuccessfulRollback(string machineId, string parameters, CommandResponse req, Application application, IApplicationRepository applicationRepository, INotificationService notificationService, AlertMessage alert)
		{
			_logger.LogInformation("Processing successful CommandResponse for MachineId: {0}, CommandId: {1}, Parameters: {2}", machineId, req.CommandId, parameters);

			application.CurrentVersion = parameters;
			applicationRepository.Update(application);
			await applicationRepository.SaveChangesAsync();

			// await SendVersionDataRequestAsync("gateway-patrick-1234");

			var successMessage = $"MachineId: {machineId} successfully rolled back to version: {parameters}.";
			alert.Message = successMessage;
			await notificationService.SendMessageToUriAsync("alert", alert.ToString());

			_logger.LogInformation("Successfully processed rollback for MachineId: {0}, CommandId: {1}. Updated to version: {2}", machineId, req.CommandId, parameters);
		}

		private async Task HandleFailedRollback(string machineId, CommandResponse req, INotificationService notificationService, AlertMessage alert)
		{
			_logger.LogWarning("Rollback command failed for MachineId: {0}, CommandId: {1}.", machineId, req.CommandId);

			var failureMessage = $"Rollback failed for MachineId: {machineId}. CommandId: {req.CommandId}.";
			alert.Message = failureMessage;
			await notificationService.SendMessageToUriAsync("alert", alert.ToString());
		}

		private (List<string> succeeded, List<string> failed) ParseDeviceDetails(string details)
		{
			var succeeded = new List<string>();
			var failed = new List<string>();

			if (string.IsNullOrWhiteSpace(details))
			{
				return (succeeded, failed); 
			}

			var successMatch = Regex.Match(details, @"succeeded on\s*(.*?)(?:,\s*failed on|$)", RegexOptions.IgnoreCase);
			if (successMatch.Success && !string.IsNullOrWhiteSpace(successMatch.Groups[1].Value))
			{
				succeeded = successMatch.Groups[1].Value
					.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
					.Select(d => d.Trim())
					.Where(d => !string.IsNullOrWhiteSpace(d))
					.ToList();
			}

			var failedMatch = Regex.Match(details, @"failed on\s*(.*)", RegexOptions.IgnoreCase);
			if (failedMatch.Success && !string.IsNullOrWhiteSpace(failedMatch.Groups[1].Value))
			{
				failed = failedMatch.Groups[1].Value
					.TrimEnd('.')
					.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
					.Select(d => d.Trim())
					.Where(d => !string.IsNullOrWhiteSpace(d))
					.ToList();
			}

			return (succeeded, failed);
		}

		private async Task HandleUpdateAcknowledged(string gatewayId, UpdateAck req)
		{
			_logger.LogInformation("Gateway [{0}] acknowledged update: {1}, success={2}",
				gatewayId, req.FileName, req.Success);

			using var scope = _serviceScopeFactory.CreateScope();
			var applicationRepository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
			var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

			if (!_updateMetadataMap.TryGetValue(req.FileName, out var metadata))
			{
				_logger.LogWarning("No metadata found for FileName: {0}. Unable to process UpdateAck.", req.FileName);
				return;
			}

			try
			{
				_logger.LogInformation("Processing UpdateAck for GatewayId: {0}, FileName: {1}, Success: {2}",
					gatewayId, req.FileName, req.Success);

				var (succeededDevices, failedDevices) = req.Success
					? (metadata.TargetDevices.ToList(), new List<string>())
					: ExtractUpdatedDevices(req.Details);


				foreach (var deviceUuid in metadata.TargetDevices)
				{
					if (succeededDevices.Contains(deviceUuid))
					{
						var application = await applicationRepository.GetByMachineId(deviceUuid);
						if (application == null)
						{
							//await LogAndNotifyFailure(notificationService, deviceUuid, req.FileName, metadata.AppName);
							var newApp = new Application
							{
								MachineId = deviceUuid,
								AppName = metadata.AppName,
								AddedBy = metadata.UserId,
								CurrentVersion = metadata.Version,
								UpdatedAt = DateTime.UtcNow
							};

							await applicationRepository.AddAsync(newApp);
							await applicationRepository.SaveChangesAsync();

							continue;
						}

						await UpdateApplicationAndNotify(applicationRepository, notificationService, application, metadata, deviceUuid);
					}
					else
					{
						await LogAndNotifyFailure(notificationService, deviceUuid, req.FileName, metadata.AppName);
					}
				}

				_updateMetadataMap.TryRemove(req.FileName, out _);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while processing UpdateAck for GatewayId: {0}, FileName: {1}.",
					gatewayId, req.FileName);

				await notificationService.SendMessageToUriAsync("Alert/Update",
					$"An error occurred while processing update for GatewayId: {gatewayId}, FileName: {req.FileName}. Error: {ex.Message}");
			}
		}

		// TODO: test with real gateway calling version data info after updating could be a better way than updating manually
		// or maybe call getversion info after every rollback/update
		private async Task UpdateApplicationAndNotify(IApplicationRepository repository, INotificationService notificationService, Application application, UpdateMetadata metadata, string deviceUuid)
		{
			application.CurrentVersion = metadata.Version;
			application.AppName = metadata.AppName;
			application.AddedBy = metadata.UserId;

			repository.Update(application);
			await repository.SaveChangesAsync();

			//await SendVersionDataRequestAsync("gateway-patrick-1234");

			_logger.LogInformation(
				"Successfully updated application for DeviceUuid: {0} to version: {1}, AppName: {2}",
				deviceUuid, metadata.Version, metadata.AppName);

			await notificationService.SendMessageToUriAsync("Alert/Update",
				$"DeviceUuid: {deviceUuid} successfully updated to version: {metadata.Version}, AppName: {metadata.AppName}.");
		}


		private async Task LogAndNotifyFailure(INotificationService notificationService, string deviceUuid, string fileName, string appName)
		{
			_logger.LogWarning("Update failed for DeviceUuid: {0}. FileName: {1}", deviceUuid, fileName);

			await notificationService.SendMessageToUriAsync("Alert/Update",
				$"Update failed for DeviceUuid: {deviceUuid}. FileName: {fileName}, AppName: {appName}.");
		}

		private (List<string> succeeded, List<string> failed) ExtractUpdatedDevices(string detailMsg)
		{
			var succeededDevices = new List<string>();
			var failedDevices = new List<string>();

			string succeededPart = string.Empty, failedPart = string.Empty;

			if (detailMsg.Contains("Succeeded on:"))
			{
				int start = detailMsg.IndexOf("Succeeded on:") + "Succeeded on:".Length;
				int end = detailMsg.Contains("Failed on:") ? detailMsg.IndexOf("Failed on:") : detailMsg.Length;
				succeededPart = detailMsg.Substring(start, end - start).Trim().TrimEnd(';');
			}

			if (detailMsg.Contains("Failed on:"))
			{
				int start = detailMsg.IndexOf("Failed on:") + "Failed on:".Length;
				failedPart = detailMsg.Substring(start).Trim().TrimEnd('.');
			}

			if (!string.IsNullOrWhiteSpace(succeededPart))
				succeededDevices = succeededPart.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

			if (!string.IsNullOrWhiteSpace(failedPart))
				failedDevices = failedPart.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

			return (succeededDevices, failedDevices);
		}

		/// <summary>
		/// receive current running version and available version
		/// </summary>
		private async Task HandleVersionDataResponse(string gatewayId, VersionDataResponse resp)
		{
			// how to handle diffrent package version in cloud database and gateway version?
			// should this update database? (if gateway is the trusted version) or should cloud send what is supposed to be the correct version (if cloud is the trusted source)
			// currently updating database (trusting gateway)

			//same case with available versions (just logging it currently)
			if (!resp.Success)
			{
				_logger.LogWarning("Gateway [{0}] reported version data request failed: {1}", gatewayId, resp.Message);
			}
			else
			{
				if (resp.DeviceVersionsList == null || !resp.DeviceVersionsList.Any())
				{
					_logger.LogWarning("Gateway [{0}] reported no devices in VersionDataResponse.", gatewayId);
					return;
				}

				_logger.LogInformation("VersionDataResponse from [{0}]: {1}", gatewayId, resp.Message);

				using var scope = _serviceScopeFactory.CreateScope();
				var applicationRepository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
				var applicationVersionRepository = scope.ServiceProvider.GetRequiredService<IApplicationVersionRepository>();
				var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
				var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

				foreach (var dv in resp.DeviceVersionsList)
				{
					var machineExists = await machineRepository.GetByIdAsync(dv.DeviceUuid);
					if (machineExists is null)
					{
						_logger.LogError("Foreign Key Violation: Machine UUID [{0}] does not exist in Machines table. Skipping insert.",
							dv.DeviceUuid);
						continue; 
					}

					_logger.LogInformation(" Device={0}", dv.DeviceUuid);

					// if there are no Application (current running service) in the database add the "current" of the gRPC version data info (if it is not null)
					// if theres is an Application for the UUID update it if it is a different version
					var application = await applicationRepository.GetByMachineId(dv.DeviceUuid);
					if (application is null)
					{
						if (dv.Current is not null)
						{
							var newApp = new Application
							{
								MachineId = dv.DeviceUuid,
								AppName = "Monitoring Service",
								AddedBy = "Found on Machine", // because if this happens that mean the package didnt come from the front end where the user will be detected
								CurrentVersion = dv.Current.Version,
								UpdatedAt = DateTime.UtcNow
							};

							await applicationRepository.AddAsync(newApp);
							await applicationRepository.SaveChangesAsync();
						}

						_logger.LogInformation(
							$"machine: [{dv.DeviceUuid}] have an application running that is not in the cloud database");
					}
					else
					{
						// if current is not null and different than the one in Application table; update the table
						if (dv.Current != null)
						{
							if (application.CurrentVersion != dv.Current.Version)
							{
								application.CurrentVersion = dv.Current.Version;
								application.UpdatedAt = dv.Current.InstalledAt.ToDateTime();

								applicationRepository.Update(application);
								await applicationRepository.SaveChangesAsync();
							}
							else {
								_logger.LogInformation($"No Database update required for Current Application Running in {dv.DeviceUuid}");
							}

							_logger.LogInformation("  CURRENT => Version={0}, InstalledAt={1}", dv.Current.Version,
								dv.Current.InstalledAt.ToDateTime());
						}
						else
						{
							_logger.LogInformation("  CURRENT => (none)");
						}
					}

					// if there are available versions in the message find in database a version with the same machineid and version name combination
					// get all versions in database with machineid. cross check with version data response
					// if the versions in the database contains a version that is not in the machine delete from database and add from the version data response
					if (dv.Available.Count > 0)
					{
						_logger.LogInformation("  AVAILABLE:");

						foreach (var av in dv.Available)
						{
							_logger.LogInformation($"Version: {av.Version}, Release Date: {av.InstalledAt}");
						}

						// Get all versions from the database for this machine and the one from incoming data
						List<ApplicationVersion> availableVersions = await applicationVersionRepository.GetVersionsByMachineId(dv.DeviceUuid);
						var incomingVersionNames = dv.Available
							.Select(av => av.Version)
							.ToHashSet();

						// Find versions in the database that are not in the incoming data (to delete)
						var versionsToDelete = availableVersions
							.Where(v => !incomingVersionNames.Contains(v.VersionName))
							.ToList();

						// Delete versions that are no longer on the machine
						if (versionsToDelete.Any())
						{
							foreach (var version in versionsToDelete)
							{
								_logger.LogInformation("Deleting outdated version {0} from database for Machine [{1}]",
									version.VersionName, dv.DeviceUuid);

								applicationVersionRepository.Remove(version);
							}
						}
						else { 
							_logger.LogInformation($"No database deletes required for application version for device: {dv.DeviceUuid}");
						}

						// Find versions from incoming data that are not in the database (to add)
						var existingVersionNames = availableVersions.Select(v => v.VersionName).ToHashSet();
						var versionsToAdd = dv.Available
							.Where(av => !existingVersionNames.Contains(av.Version))
							.ToHashSet();

						// Add new versions that are missing in the database
						if (versionsToAdd.Any())
						{
							foreach (var version in versionsToAdd)
							{
								var newVersion = new ApplicationVersion
								{
									MachineId = dv.DeviceUuid,
									VersionName = version.Version,
									Date = version.InstalledAt.ToDateTime()
								};

								_logger.LogInformation($"Adding new version {version.Version} to database");
								await applicationVersionRepository.AddAsync(newVersion);
							}
						}
						else
						{
							_logger.LogInformation($"No database ammends required for application version for device {dv.DeviceUuid}");
						}

						await applicationVersionRepository.SaveChangesAsync();
					}
					else
					{
						_logger.LogInformation("  AVAILABLE => (none)");
					}
				}
			}
		}

		#endregion

		#region PUBLIC METHODS

		/// <summary>
		/// Sends a LicenceResponse to connected gateways.
		/// </summary>
		public async Task SendLicenceResponseAsync(
			string gatewayId,
			string deviceUuid,
			bool approved,
			string licenceContent,
			DateTime expirationDate)
		{
			var licenseResponse = new LicenseResponse
			{
				DeviceUuid = deviceUuid,
				Approved = approved,
				License = approved ? licenceContent : string.Empty,
				ExpirationDate = Timestamp.FromDateTime(expirationDate.ToUniversalTime())
			};

			var responseMessage = new ControlMessage
			{
				SenderId = "Cloud",
				LicenseResponse = licenseResponse
			};

			if (_connectedGateways.TryGetValue(gatewayId, out var writer))
				try
				{
					await writer.WriteAsync(responseMessage);
					_logger.LogInformation("LicenceResponse sent to Gateway [{0}] for DeviceUuid={1}, Approved={2}",
						gatewayId, deviceUuid, approved);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to send LicenceResponse to Gateway [{0}] for DeviceUuid={1}",
						gatewayId, deviceUuid);
				}
			else
				_logger.LogWarning("Gateway [{0}] is not connected. LicenceResponse not sent.", gatewayId);
		}

		/// <summary>
		/// Sends a command request (e.g. ROLLBACK) to a connected gateway.
		/// </summary>
		public async Task SendCommandToGatewayAsync(
			string gatewayId,
			string commandId,
			CommandType cmdType,
			string parameters,
			string userEmail,
			params string[] targetDevices
			)
		{
			if (!_connectedGateways.TryGetValue(gatewayId, out var writer))
			{
				_logger.LogWarning("Gateway [{0}] is not connected.", gatewayId);
				return;
			}

			var cmdReq = new CommandRequest
			{
				CommandId = commandId,
				CommandType = cmdType,
				Parameters = parameters
			};
			cmdReq.TargetDevices.AddRange(targetDevices);

			var msg = new ControlMessage
			{
				SenderId = "Cloud",
				CommandRequest = cmdReq
			};

			try
			{
				await writer.WriteAsync(msg);

				_commandIdToMetadataMap[commandId] = new CommandMetadata
				{
					MachineIds = targetDevices.ToList(),
					Parameters = parameters,
					CommandType = cmdType
				};

				_logger.LogInformation("CommandRequest sent to [{0}]: id={1}, type={2}, deviceCount={3}",
					gatewayId, commandId, cmdType, targetDevices.Length);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send CommandRequest to [{0}]: id={1}, type={2}", gatewayId, commandId,
					cmdType);
			}
		}

		/// <summary>
		/// Requests logs from specified devices on the connected gateway.
		/// </summary>
		public async Task SendLogRequestAsync(string gatewayId, string[] deviceUuids)
		{
			if (!_connectedGateways.TryGetValue(gatewayId, out var writer))
			{
				_logger.LogWarning("Gateway [{0}] is not connected.", gatewayId);
				return;
			}

			LogRequestMessage req = new();
			req.DeviceUuids.AddRange(deviceUuids);

			ControlMessage msg = new()
			{
				SenderId = "Cloud",
				LogRequest = req
			};
			await writer.WriteAsync(msg);

			_logger.LogInformation("LogRequest sent to [{0}] for {1} device(s).", gatewayId, deviceUuids.Length);
		}

		/// <summary>
		/// Sends an update package to the gateway, which should forward/install it on the specified devices.
		/// </summary>
		public async Task SendUpdatePackageAsync(string gatewayId, string fileName, byte[] packageData,
			string[] targetDevices, string appName, string version, string userEmail)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

			var user = await userRepository.GetUserByEmail(userEmail);
			if (user is null)
			{
				_logger.LogWarning("Update package upload failed. Invalid user email");
				return;
			}

			if (!_connectedGateways.TryGetValue(gatewayId, out var writer))
			{
				_logger.LogWarning("Gateway [{0}] is not connected.", gatewayId);
				return;
			}

			var update = new UpdatePackage
			{
				FileName = fileName,
				PackageData = Google.Protobuf.ByteString.CopyFrom(packageData)
			};
			update.TargetDevices.AddRange(targetDevices);

			var msg = new ControlMessage
			{
				SenderId = "Cloud",
				UpdatePackage = update
			};

			_updateMetadataMap[fileName] = new UpdateMetadata
			{
				TargetDevices = targetDevices.ToList(),
				AppName = appName,
				Version = version,
				FileName = fileName,
				UserId = user.UserId
			};

			try
			{
				await writer.WriteAsync(msg);

				_logger.LogInformation("UpdatePackage [{0}] of size {1} bytes sent to [{2}], for {3} devices.",
					fileName, packageData.Length, gatewayId, targetDevices.Length);
			}
			catch (Exception ex)
			{
				_updateMetadataMap.TryRemove(fileName, out _);
				_logger.LogError(ex, "Failed to send UpdatePackage [{0}] to Gateway [{1}].", fileName, gatewayId);
			}
		}

		/// <summary>
		/// Sends a request for version data; the gateway should respond with a VersionDataResponse.
		/// </summary>
		public async Task SendVersionDataRequestAsync(string gatewayId)
		{
			if (!_connectedGateways.TryGetValue(gatewayId, out var writer))
			{
				_logger.LogWarning("Gateway [{0}] is not connected.", gatewayId);
				return;
			}

			var msg = new ControlMessage
			{
				SenderId = "Cloud",
				VersionDataRequest = new VersionDataRequest()
			};
			await writer.WriteAsync(msg);

			_logger.LogInformation("VersionDataRequest sent to gateway [{0}].", gatewayId);
		}
	}

	#endregion
}

#region supporting classes

public class CommandMetadata
{
	public List<string> MachineIds { get; set; } = new();

	public string Parameters { get; set; } = string.Empty;

	public CommandType CommandType { get; set; }

	public string UserId { get; set; } = string.Empty;
}

public class UpdateMetadata
{
	public List<string> TargetDevices { get; set; } = new();

	public string AppName { get; set; } = string.Empty;

	public string Version { get; set; } = string.Empty;

	public string FileName { get; set; } = string.Empty;

	public string UserId { get; set; } = string.Empty;
}

#endregion