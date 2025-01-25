using Grpc.Core;
using System.Collections.Concurrent;
using UpFlux_WebService.Protos;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Core.Models;
using Google.Protobuf.WellKnownTypes;
using Upflux_WebService.Services.Interfaces;

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

		// A dictionary of "GatewayID" => the active IServerStreamWriter
		private readonly ConcurrentDictionary<string, IServerStreamWriter<ControlMessage>> _connectedGateways
			= new ConcurrentDictionary<string, IServerStreamWriter<ControlMessage>>();

		public ControlChannelService(ILogger<ControlChannelService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
		{
			_logger = logger;
			_serviceScopeFactory = serviceScopeFactory;
			_logDirectoryPath = configuration["Logging:MachineLogsDirectory"];
		}

		public override async Task OpenControlChannel(
			IAsyncStreamReader<ControlMessage> requestStream,
			IServerStreamWriter<ControlMessage> responseStream,
			ServerCallContext context)
		{
			string gatewayId = "UNKNOWN";
			try
			{
				// Expect the first message to identify the gateway
				if (!await requestStream.MoveNext())
				{
					_logger.LogWarning("No initial message from gateway; closing channel.");
					return;
				}

				ControlMessage firstMsg = requestStream.Current;
				gatewayId = firstMsg.SenderId ?? "UNKNOWN";
				_connectedGateways[gatewayId] = responseStream;

				_logger.LogInformation("Gateway [{0}] connected to ControlChannel.", gatewayId);

				// Optionally handle the first message if it has a payload
				await HandleIncomingMessage(gatewayId, firstMsg);

				// Continue reading messages until the gateway disconnects
				while (await requestStream.MoveNext())
				{
					await HandleIncomingMessage(gatewayId, requestStream.Current);
				}

				_logger.LogInformation("Gateway [{0}] disconnected.", gatewayId);
			}
			finally
			{
				_connectedGateways.TryRemove(gatewayId, out _);
			}
		}

		private async Task HandleIncomingMessage(string gatewayId, ControlMessage msg)
		{
			switch (msg.PayloadCase)
			{
				case ControlMessage.PayloadOneofCase.LicenseRequest:
					await HandleLicenseRequest(gatewayId, msg.LicenseRequest);
					break;
				case ControlMessage.PayloadOneofCase.LogUpload:
					await HandleLogUpload(gatewayId, msg.LogUpload);
					break;
				case ControlMessage.PayloadOneofCase.MonitoringData:
					HandleMonitoringData(gatewayId, msg.MonitoringData);
					break;
				case ControlMessage.PayloadOneofCase.AlertMessage:
					await HandleAlertMessage(gatewayId, msg.AlertMessage);
					break;
				case ControlMessage.PayloadOneofCase.CommandResponse:
					_logger.LogInformation("Gateway [{0}] responded to command: {1}", gatewayId, msg.CommandResponse.CommandId);
					break;
				case ControlMessage.PayloadOneofCase.UpdateAck:
					_logger.LogInformation("Gateway [{0}] acknowledged update: {1}, success={2}",
						gatewayId, msg.UpdateAck.FileName, msg.UpdateAck.Success);
					break;
				case ControlMessage.PayloadOneofCase.LogResponse:
					_logger.LogInformation("Gateway [{0}] responded to log request => success={1}, msg={2}",
						gatewayId, msg.LogResponse.Success, msg.LogResponse.Message);
					break;
				case ControlMessage.PayloadOneofCase.VersionDataResponse:
					HandleVersionDataResponse(gatewayId, msg.VersionDataResponse);
					break;
				default:
					_logger.LogWarning("Received unknown message from [{0}] => {1}", gatewayId, msg.PayloadCase);
					break;
			}
		}

		// ---------- EXACT license logic (with console prompt) ----------
		private async Task HandleLicenseRequest(string gatewayId, LicenseRequest req)
		{
			_logger.LogInformation("Handling license request for Gateway ID: {GatewayId}, IsRenewal: {IsRenewal}, Device UUID: {DeviceUuid}",
				gatewayId, req.IsRenewal, req.DeviceUuid);

			try
			{
				if (!req.IsRenewal)
				{
					await AddUnregisteredDevice(gatewayId, req.DeviceUuid);
				}
				else
				{
					await ProcessRenewalRequest(gatewayId, req.DeviceUuid);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while handling license request for Device UUID: {DeviceUuid}", req.DeviceUuid);
				await SendNotificationMessageAsync(gatewayId, "An error occurred while processing the license request.");
			}
		}

		private async Task AddUnregisteredDevice(string gatewayId, string deviceUuid)
		{
			_logger.LogInformation("Processing new license request for Device UUID: {DeviceUuid}", deviceUuid);

			using var scope = _serviceScopeFactory.CreateScope();
			var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
			var generatedMachineIdRepository = scope.ServiceProvider.GetRequiredService<IGeneratedMachineIdRepository>();

			var generatedId = await generatedMachineIdRepository.GetByMachineId(deviceUuid);
			if (generatedId is null)
			{
				_logger.LogWarning("Received communication attempt from unknown Machine ID: {DeviceUuid}", deviceUuid);
				await SendNotificationMessageAsync(gatewayId, $"Unknown Machine ID: {deviceUuid}. Request denied.");
				return;
			}

			_logger.LogInformation("Validated Machine ID: {DeviceUuid} with Generated Machine ID Repository", deviceUuid);

			if (await machineRepository.GetByIdAsync(deviceUuid) != null)
			{
				_logger.LogInformation("Machine ID: {DeviceUuid} already exists in the repository. No action required.", deviceUuid);
				await SendNotificationMessageAsync(gatewayId, $"Machine ID: {deviceUuid} already exists in the repository.");
				return;
			}

			Machine newMachine = new()
			{
				MachineId = deviceUuid,
				dateAddedOn = DateTime.UtcNow,
				ipAddress = "NA"
			};

			_logger.LogInformation("Adding new Machine record for Device UUID: {DeviceUuid}", deviceUuid);

			try
			{
				await machineRepository.AddAsync(newMachine);
				await machineRepository.SaveChangesAsync();

				await SendNotificationMessageAsync(gatewayId, $"Successfully added new Machine record for Device UUID: {deviceUuid}.");
				_logger.LogInformation("Successfully added new Machine record for Device UUID: {DeviceUuid}", deviceUuid);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to add or save new Machine record for Device UUID: {DeviceUuid}", deviceUuid);
				throw;
			}
		}

		private async Task ProcessRenewalRequest(string gatewayId, string deviceUuid)
		{
			_logger.LogInformation("Processing license renewal request for Device UUID: {DeviceUuid}", deviceUuid);

			// send signalR notification for and expired licence 

			await SendNotificationMessageAsync(gatewayId, $"License renewal request received for Device UUID: {deviceUuid}. Further action required.");
			_logger.LogInformation("Notification sent for license renewal request for Device UUID: {DeviceUuid}", deviceUuid);
		}

		private async Task SendNotificationMessageAsync(string gatewayId, string description)
		{
			if (_connectedGateways.TryGetValue(gatewayId, out IServerStreamWriter<ControlMessage>? writer))
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

		// ---------- EXACT log saving logic ----------
		private async Task HandleLogUpload(string gatewayId, LogUpload upload)
		{
			try
			{
				_logger.LogInformation("Received LogUpload from device={0} at gateway=[{1}], file={2}, size={3} bytes",
					upload.DeviceUuid, gatewayId, upload.FileName, upload.Data.Length);

				// Delete and recreate the directory to reset it
				if (Directory.Exists(_logDirectoryPath))
				{
					Directory.Delete(_logDirectoryPath, true); // true ensures recursive deletion
					_logger.LogInformation("Deleted existing log directory: {0}", _logDirectoryPath);
				}

				Directory.CreateDirectory(_logDirectoryPath);
				_logger.LogInformation("Recreated log directory: {0}", _logDirectoryPath);

				// Save the log to the specified directory
				string filePath = Path.Combine(_logDirectoryPath, upload.FileName);
				await File.WriteAllBytesAsync(filePath, upload.Data.ToByteArray());

				_logger.LogInformation("Log saved to: {0}", filePath);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error handling log upload for device={0} at gateway=[{1}]", upload.DeviceUuid, gatewayId);
				throw;
			}
		}

		// ---------- EXACT monitoring logic ----------
		private async Task HandleMonitoringData(string gatewayId, MonitoringDataMessage mon)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

			foreach (AggregatedData? aggregatedData in mon.AggregatedData)
			{
				_logger.LogInformation("Monitoring from dev={0} (gw={1}): CPU={2}%, MEM={3}%",
					aggregatedData.Uuid, gatewayId, aggregatedData.Metrics.CpuUsage, aggregatedData.Metrics.MemoryUsage);

				var dataToSend = new
				{
					Uuid = aggregatedData.Uuid,
					Timestamp = aggregatedData.Timestamp.ToDateTime(),
					Metrics = new
					{
						CpuUsage = aggregatedData.Metrics.CpuUsage,
						MemoryUsage = aggregatedData.Metrics.MemoryUsage,
						DiskUsage = aggregatedData.Metrics.DiskUsage,
						NetworkUsage = new
						{
							BytesSent = aggregatedData.Metrics.NetworkUsage.BytesSent,
							BytesReceived = aggregatedData.Metrics.NetworkUsage.BytesReceived
						},
						CpuTemperature = aggregatedData.Metrics.CpuTemperature,
						SystemUptime = aggregatedData.Metrics.SystemUptime
					},
					SensorData = new
					{
						RedValue = aggregatedData.SensorData.RedValue,
						GreenValue = aggregatedData.SensorData.GreenValue,
						BlueValue = aggregatedData.SensorData.BlueValue
					}
				};

				try
				{
					await notificationService.SendMessageToUriAsync(aggregatedData.Uuid, mon.AggregatedData.ToString());
					_logger.LogInformation("Successfully sent data for MachineId {Uuid}.", aggregatedData.Uuid);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to send data for MachineId {Uuid}.", aggregatedData.Uuid);
				}
			}
		}

		// ---------- EXACT alert logic ----------
		private async Task HandleAlertMessage(string gatewayId, AlertMessage alert)
		{
			_logger.LogInformation("ALERT from gw={0}, dev={1}, level={2}, msg={3}",
				gatewayId, alert.Source, alert.Level, alert.Message);

			// send signalR notification here
			using var scope = _serviceScopeFactory.CreateScope();
			var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
			await notificationService.SendMessageToUriAsync("Alert", alert.ToString());

			// Send an alertResponse back
			if (_connectedGateways.TryGetValue(gatewayId, out IServerStreamWriter<ControlMessage>? writer))
			{
				ControlMessage responseMsg = new ControlMessage
				{
					SenderId = "Cloud",
					Description= "Alert received by the cloud.",
					AlertResponse = new AlertResponseMessage
					{
						Success = true,
						Message = "Cloud: alert received"
					}
				};
				await writer.WriteAsync(responseMsg);
			}
		}

		// ---------- EXACT version data logic ----------
		private void HandleVersionDataResponse(string gatewayId, VersionDataResponse resp)
		{
			if (!resp.Success)
			{
				_logger.LogWarning("Gateway [{0}] reported version data request failed: {1}", gatewayId, resp.Message);
			}
			else
			{
				_logger.LogInformation("VersionDataResponse from [{0}]: {1}", gatewayId, resp.Message);
				foreach (DeviceVersions? dv in resp.DeviceVersionsList)
				{
					_logger.LogInformation(" Device={0}", dv.DeviceUuid);
					if (dv.Current != null)
					{
						DateTime installed = dv.Current.InstalledAt.ToDateTime();
						_logger.LogInformation("  CURRENT => Version={0}, InstalledAt={1}", dv.Current.Version, installed);
					}
					else
					{
						_logger.LogInformation("  CURRENT => (none)");
					}

					if (dv.Available.Count > 0)
					{
						_logger.LogInformation("  AVAILABLE:");
						foreach (Protos.VersionInfo? av in dv.Available)
						{
							_logger.LogInformation("    - Version={0}, InstalledAt={1}",
								av.Version, av.InstalledAt.ToDateTime());
						}
					}
					else
					{
						_logger.LogInformation("  AVAILABLE => (none)");
					}
				}
			}
		}

		// ---------- PUBLIC METHODS to push messages from the console menu ----------

		/// <summary>
		/// Sends a LicenceResponse to connected gateways.
		/// </summary>
		public async Task SendLicenceResponseAsync(
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
				SenderId = "Web Service",
				LicenseResponse = licenseResponse
			};

			/// send to every gateway for now
			foreach (var kvp in _connectedGateways)
			{
				var gatewayId = kvp.Key;
				var writer = kvp.Value;

				try
				{
					await writer.WriteAsync(responseMessage);
					_logger.LogInformation("LicenceResponse sent to Gateway [{0}] for DeviceUuid={1}, Approved={2}",
						gatewayId, deviceUuid, approved);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to send LicenceResponse to Gateway [{0}] for DeviceUuid={1}", gatewayId, deviceUuid);
				}
			}
		}

		/// <summary>
		/// Sends a command request (e.g. ROLLBACK) to a connected gateway.
		/// </summary>
		public async Task SendCommandToGatewayAsync(string gatewayId,
													string commandId,
													CommandType cmdType,
													string parameters,
													params string[] targetDevices)
		{
			if (!_connectedGateways.TryGetValue(gatewayId, out IServerStreamWriter<ControlMessage>? writer))
			{
				_logger.LogWarning("Gateway [{0}] is not connected.", gatewayId);
				return;
			}

			CommandRequest cmdReq = new CommandRequest
			{
				CommandId = commandId,
				CommandType = cmdType,
				Parameters = parameters
			};
			cmdReq.TargetDevices.AddRange(targetDevices);

			ControlMessage msg = new ControlMessage
			{
				SenderId = "CloudSim",
				CommandRequest = cmdReq
			};
			await writer.WriteAsync(msg);

			_logger.LogInformation("CommandRequest sent to [{0}]: id={1}, type={2}, deviceCount={3}",
				gatewayId, commandId, cmdType, targetDevices.Length);
		}

		/// <summary>
		/// Requests logs from specified devices on the connected gateway.
		/// </summary>
		public async Task SendLogRequestAsync(string gatewayId, string[] deviceUuids)
		{
			if (!_connectedGateways.TryGetValue(gatewayId, out IServerStreamWriter<ControlMessage>? writer))
			{
				_logger.LogWarning("Gateway [{0}] is not connected.", gatewayId);
				return;
			}

			LogRequestMessage req = new LogRequestMessage();
			req.DeviceUuids.AddRange(deviceUuids);

			ControlMessage msg = new ControlMessage
			{
				SenderId = "CloudSim",
				LogRequest = req
			};
			await writer.WriteAsync(msg);

			_logger.LogInformation("LogRequest sent to [{0}] for {1} device(s).", gatewayId, deviceUuids.Length);
		}

		/// <summary>
		/// Sends an update package to the gateway, which should forward/install it on the specified devices.
		/// </summary>
		public async Task SendUpdatePackageAsync(string gatewayId, string fileName, byte[] packageData, string[] targetDevices)
		{
			// send to every gateway fro now
			if (!_connectedGateways.TryGetValue(gatewayId, out IServerStreamWriter<ControlMessage>? writer))
			{
				_logger.LogWarning("Gateway [{0}] is not connected.", gatewayId);
				return;
			}

			UpdatePackage update = new UpdatePackage
			{
				FileName = fileName,
				PackageData = Google.Protobuf.ByteString.CopyFrom(packageData)
			};
			update.TargetDevices.AddRange(targetDevices);

			ControlMessage msg = new ControlMessage
			{
				SenderId = "CloudSim",
				UpdatePackage = update
			};
			await writer.WriteAsync(msg);

			_logger.LogInformation("UpdatePackage [{0}] of size {1} bytes sent to [{2}], for {3} devices.",
				fileName, packageData.Length, gatewayId, targetDevices.Length);
		}

		/// <summary>
		/// Sends a request for version data; the gateway should respond with a VersionDataResponse.
		/// </summary>
		public async Task SendVersionDataRequestAsync(string gatewayId)
		{
			if (!_connectedGateways.TryGetValue(gatewayId, out IServerStreamWriter<ControlMessage>? writer))
			{
				_logger.LogWarning("Gateway [{0}] is not connected.", gatewayId);
				return;
			}

			ControlMessage msg = new ControlMessage
			{
				SenderId = "CloudSim",
				VersionDataRequest = new VersionDataRequest()
			};
			await writer.WriteAsync(msg);

			_logger.LogInformation("VersionDataRequest sent to gateway [{0}].", gatewayId);
		}

	}
}
