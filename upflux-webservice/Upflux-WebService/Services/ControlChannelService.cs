using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Services.Interfaces;
using UpFlux_WebService.Protos;
using Upflux_WebService.Repository;
using static Upflux_WebService.Services.EntityQueryService;
using System.Text.Json;
using System.Diagnostics.Metrics;
using static Google.Protobuf.Reflection.FieldOptions.Types;
using System.ComponentModel.Design;
using Upflux_WebService.Services.Enums;


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
                case ControlMessage.PayloadOneofCase.AiRecommendations:
                    await HandleAiRecommendations(gatewayId, msg.AiRecommendations);
                    break;
                case ControlMessage.PayloadOneofCase.DeviceStatus:
                    await HandleDeviceStatus(gatewayId, msg.DeviceStatus);
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

            using var scope = _serviceScopeFactory.CreateScope();
            var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
            var generatedMachineIdRepository =
                scope.ServiceProvider.GetRequiredService<IGeneratedMachineIdRepository>();

            try
            {
                var generatedId = await generatedMachineIdRepository.GetByMachineId(req.DeviceUuid);
                if (generatedId is null)
                {
                    _logger.LogWarning(
                        $"An unrecognized device is trying to communicate using the machine id: {req.DeviceUuid}");
                    return;
                }

                var machine = await machineRepository.GetByIdAsync(req.DeviceUuid);

                if (machine is null)
                    await AddUnregisteredDevice(gatewayId, req.DeviceUuid);
                else if (machine is not null)
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
            var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();

            foreach (var aggregatedData in mon.AggregatedData)
            {
                _logger.LogInformation("Monitoring from dev={0} (gw={1}): CPU={2}%, MEM={3}%",
                    aggregatedData.Uuid, gatewayId, aggregatedData.Metrics.CpuUsage,
                    aggregatedData.Metrics.MemoryUsage);

                try
                {
                    // Check if the device exists in the database
                    var existingMachine = await machineRepository.GetByIdAsync(aggregatedData.Uuid);

                    if (existingMachine == null)
                    {
                        _logger.LogInformation("Device with UUID {Uuid} not found. Adding to database.",
                            aggregatedData.Uuid);
                        await machineRepository.AddAsync(new Machine
                        {
                            machineName = aggregatedData.Uuid,
                            dateAddedOn = DateTime.UtcNow,
                            ipAddress = "NA",
                            MachineId = aggregatedData.Uuid
                        });
                        _logger.LogInformation("Device with UUID {Uuid} added successfully.", aggregatedData.Uuid);

                        await machineRepository.SaveChangesAsync();
                    }

                    // Send notification
                    await notificationService.SendMessageToUriAsync(aggregatedData.Uuid, aggregatedData.ToString());
                    _logger.LogInformation("Successfully sent data for MachineId {Uuid}.", aggregatedData.Uuid);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process data for MachineId {Uuid}.", aggregatedData.Uuid);
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

            // This handles alerts that affects device packages
            if ((alert.Message.StartsWith("Update to version ") && alert.Message.Contains(" installed successfully")) || (alert.Message.StartsWith("Rollback to version ") &&
                    alert.Message.Contains(" completed successfully")))
            {
                await SendVersionDataRequestAsync("gateway-patrick-1234");
                await notificationService.SendMessageToUriAsync($"Alert/Update", alert.ToString());
            }

            await notificationService.SendMessageToUriAsync($"Alert", alert.ToString());

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
                    if (metadata.CommandType == ControlChannelCommandType.Rollback)
                        await ProcessMachineRollbackResponse(machineId, metadata, req);
                    else if (metadata.CommandType == ControlChannelCommandType.ScheduledUpdate)
                        await ProcessScheduledUpdateResponse(machineId, metadata, req);

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

        ///
        private async Task ProcessScheduledUpdateResponse(
            string machineId,
            CommandMetadata metadata,
            CommandResponse req)
        {
            var alert = new AlertMessage();
            using var scope = _serviceScopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();

            try
            {
                if (req.Success)
                {
                    var machine = await machineRepository.GetByIdAsync(machineId);
                    machine.lastUpdatedBy = metadata.UserId;

                    machineRepository.Update(machine);
                    await machineRepository.SaveChangesAsync();

                    var successMessage =
                        $"Scheduled update request for MachineId: {machineId}, to version: {metadata} successfully sent.";
                    alert.Message = successMessage;
                    await notificationService.SendMessageToUriAsync("Alert/ScheduledUpdate", alert.ToString());

                    _logger.LogInformation(
                        "Successfully sent rollback request for MachineId: {0}, CommandId: {1}. Updated to version: {2}",
                        machineId, req.CommandId, metadata);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending scheduled update for MachineId: {0}, CommandId: {1}.",
                    machineId,
                    req.CommandId);
                alert.Message =
                    $"An error occurred while processing scheduled update for MachineId: {machineId}. CommandId: {req.CommandId}. Error: {ex.Message}";
                await notificationService.SendMessageToUriAsync("Alert/ScheduledUpdate", alert.ToString());
            }
        }

        /// <summary>
        /// handles case where rollback request is successfully sent to a device (important: sent but not finished)
        /// </summary>
        private async Task ProcessMachineRollbackResponse(
            string machineId,
            CommandMetadata metadata,
            CommandResponse req)
        {
            var alert = new AlertMessage();
            using var scope = _serviceScopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();

            try
            {
                // in case of partial success
                var (succeededDevices, failedDevices) = string.IsNullOrWhiteSpace(req.Details)
                    ? (new List<string>(), new List<string>())
                    : ParseDeviceDetails(req.Details);

                var isSuccess = req.Success || succeededDevices.Contains(machineId);

                if (isSuccess)
                {
                    var machine = await machineRepository.GetByIdAsync(machineId);
                    machine.lastUpdatedBy = metadata.UserId;
                    machineRepository.Update(machine);
                    await machineRepository.SaveChangesAsync();

                    var successMessage =
                        $"Rollback request for MachineId: {machineId}, to version: {metadata} successfully sent.";
                    alert.Message = successMessage;
                    await notificationService.SendMessageToUriAsync("Alert/Rollback", alert.ToString());

                    _logger.LogInformation(
                        "Successfully sent rollback request for MachineId: {0}, CommandId: {1}. Updated to version: {2}",
                        machineId, req.CommandId, metadata);
                }
                else
                {
                    await HandleFailedRollback(machineId, req, notificationService, alert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing rollback for MachineId: {0}, CommandId: {1}.", machineId,
                    req.CommandId);
                alert.Message =
                    $"An error occurred while processing rollback for MachineId: {machineId}. CommandId: {req.CommandId}. Error: {ex.Message}";
                await notificationService.SendMessageToUriAsync("Alert/Rollback", alert.ToString());
            }
        }

        private async Task HandleFailedRollback(string machineId, CommandResponse req,
            INotificationService notificationService, AlertMessage alert)
        {
            _logger.LogWarning("Rollback command failed for MachineId: {0}, CommandId: {1}.", machineId, req.CommandId);

            var failureMessage = $"Rollback failed for MachineId: {machineId}. CommandId: {req.CommandId}.";
            alert.Message = failureMessage;
            await notificationService.SendMessageToUriAsync("Alert/Rollback", alert.ToString());
        }

        private (List<string> succeeded, List<string> failed) ParseDeviceDetails(string details)
        {
            var succeeded = new List<string>();
            var failed = new List<string>();

            if (string.IsNullOrWhiteSpace(details)) return (succeeded, failed);

            var successMatch =
                Regex.Match(details, @"succeeded on\s*(.*?)(?:,\s*failed on|$)", RegexOptions.IgnoreCase);
            if (successMatch.Success && !string.IsNullOrWhiteSpace(successMatch.Groups[1].Value))
                succeeded = successMatch.Groups[1].Value
                    .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .ToList();

            var failedMatch = Regex.Match(details, @"failed on\s*(.*)", RegexOptions.IgnoreCase);
            if (failedMatch.Success && !string.IsNullOrWhiteSpace(failedMatch.Groups[1].Value))
                failed = failedMatch.Groups[1].Value
                    .TrimEnd('.')
                    .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .ToList();

            return (succeeded, failed);
        }

        private async Task HandleUpdateAcknowledged(string gatewayId, UpdateAck req)
        {
            _logger.LogInformation("Gateway [{0}] acknowledged update: {1}, success={2}",
                gatewayId, req.FileName, req.Success);

            using var scope = _serviceScopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();

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
                    if (succeededDevices.Contains(deviceUuid))
                    {
                        var machine = await machineRepository.GetByIdAsync(deviceUuid);
                        machine.lastUpdatedBy = metadata.UserId;

                        machineRepository.Update(machine);
                        await machineRepository.SaveChangesAsync();

                        _logger.LogInformation(
                            $"The Package: {metadata.AppName}, Version: {metadata.Version}, has been sent to the device: {deviceUuid}");

                        await notificationService.SendMessageToUriAsync("Alert/Update",
                            $"The Package: {metadata.AppName}, Version: {metadata.Version}, has been sent to the device: {deviceUuid}");
                    }
                    else
                    {
                        await LogAndNotifyFailure(notificationService, deviceUuid, req.FileName, metadata.AppName);
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

        //private async Task UpdateCurrentVersionAndNotify(IMachineRepository repository,
        //	INotificationService notificationService, UpdateMetadata metadata,
        //	string deviceUuid)
        //{
        //	var machine = await repository.GetByIdAsync(deviceUuid);

        //	machine.currentVersion = metadata.Version;
        //	machine.appName = "Montoring Service";
        //	machine.lastUpdatedBy = metadata.UserId;

        //	repository.Update(machine);
        //	await repository.SaveChangesAsync();

        //	//await SendVersionDataRequestAsync("gateway-patrick-1234");

        //	_logger.LogInformation(
        //		"Successfully updated application for DeviceUuid: {0} to version: {1}, AppName: {2}",
        //		deviceUuid, metadata.Version, metadata.AppName);

        //	await notificationService.SendMessageToUriAsync("Alert/Update",
        //		$"DeviceUuid: {deviceUuid} successfully updated to version: {metadata.Version}, AppName: {metadata.AppName}.");
        //}

        private async Task LogAndNotifyFailure(INotificationService notificationService, string deviceUuid,
            string fileName, string appName)
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
                var start = detailMsg.IndexOf("Succeeded on:") + "Succeeded on:".Length;
                var end = detailMsg.Contains("Failed on:") ? detailMsg.IndexOf("Failed on:") : detailMsg.Length;
                succeededPart = detailMsg.Substring(start, end - start).Trim().TrimEnd(';');
            }

            if (detailMsg.Contains("Failed on:"))
            {
                var start = detailMsg.IndexOf("Failed on:") + "Failed on:".Length;
                failedPart = detailMsg.Substring(start).Trim().TrimEnd('.');
            }

            if (!string.IsNullOrWhiteSpace(succeededPart))
                succeededDevices = succeededPart.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (!string.IsNullOrWhiteSpace(failedPart))
                failedDevices = failedPart.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return (succeededDevices, failedDevices);
        }

        private async Task HandleDeviceStatus(string gatewayId, DeviceStatus status)
        {
            _logger.LogInformation(
                "DeviceStatus from Gateway [{0}]: device={1}, isOnline={2}, changedAt={3}",
                gatewayId, status.DeviceUuid, status.IsOnline, status.LastSeen);

            var jsonStatus = JsonSerializer.Serialize(status);

            using var scope = _serviceScopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var machineStatusRepository = scope.ServiceProvider.GetRequiredService<IMachineStatusRepository>();
            var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();

            var machine = await machineRepository.GetByIdAsync(status.DeviceUuid);
            var existingStatus = await machineStatusRepository.GetByIdAsync(status.DeviceUuid);
            if (machine is not null && existingStatus is null)
            {
                await notificationService.SendMessageToUriAsync($"Status/{status.DeviceUuid}", jsonStatus);

                MachineStatus machineStatus = new()
                {
                    MachineId = status.DeviceUuid,
                    IsOnline = status.IsOnline,
                    LastSeen = status.LastSeen.ToDateTime()
                };

                await machineStatusRepository.AddAsync(machineStatus);
                await machineStatusRepository.SaveChangesAsync();

                _logger.LogInformation($"Device Status Updated for {status.DeviceUuid}");
            }
            else if (machine is not null && existingStatus is not null)
            {
                await notificationService.SendMessageToUriAsync($"Status/{status.DeviceUuid}", jsonStatus);

                existingStatus.IsOnline = status.IsOnline;
                existingStatus.LastSeen = status.LastSeen.ToDateTime();

                machineStatusRepository.Update(existingStatus);
                await machineStatusRepository.SaveChangesAsync();

                _logger.LogInformation($"Device Status Updated for {status.DeviceUuid}");
            }
            else
            {
                _logger.LogInformation(
                    $"Invalid Device Status Detected: {status}");
            }
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
                var applicationVersionRepository =
                    scope.ServiceProvider.GetRequiredService<IApplicationVersionRepository>();
                var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
                var machineStoredVersions =
                    scope.ServiceProvider.GetRequiredService<IMachineStoredVersionsRepository>();

                foreach (var dv in resp.DeviceVersionsList)
                {
                    var machineExists = await machineRepository.GetByIdAsync(dv.DeviceUuid);
                    if (machineExists is null)
                    {
                        _logger.LogError(
                            "Foreign Key Violation: Machine UUID [{0}] does not exist in Machines table. Skipping insert.",
                            dv.DeviceUuid);
                        continue;
                    }

                    _logger.LogInformation(" Device={0}", dv.DeviceUuid);

                    // if there are no Current Version for a machine (current running service) in the database, add the "current" of the gRPC version data info (if it is not null)
                    // if theres is an Application for the UUID, update it if it is a different version
                    var machine = await machineRepository.GetByIdAsync(dv.DeviceUuid);
                    if (machine != null)
                    {
                        // this situation where the application table does not have an existing application should not happen
                        // this means that the application is not uploaded through the front end
                        if (dv.Current is not null)
                        {
                            var applicationVerion = await applicationVersionRepository.GetByIdAsync(dv.Current.Version);
                            if (applicationVerion != null)
                            {
                                machine.currentVersion = applicationVerion.VersionName;
                            }
                            else
                            {
                                await applicationVersionRepository.AddAsync(new ApplicationVersion()
                                {
                                    VersionName = dv.Current.Version,
                                    Date = DateTime.UtcNow,
                                    UploadedBy = "E120023"
                                });
                                await applicationVersionRepository.SaveChangesAsync();
                                machine.currentVersion = dv.Current.Version;
                            }

                            machineRepository.Update(machine);
                            await machineRepository.SaveChangesAsync();
                        }

                        _logger.LogInformation(
                            $"machine: [{dv.DeviceUuid}] have an application running that is not in the cloud database");
                    }

                    // if there are available versions in the message find in database a version with the same machineid and version name combination
                    // get all versions in database with machineid. cross check with version data response
                    // if the versions in the database contains a version that is not in the machine delete from database and add from the version data response
                    // TLDR: this make sure database is exactly the same as the version data received
                    if (dv.Available.Count > 0)
                    {
                        _logger.LogInformation("  AVAILABLE:");

                        foreach (var av in dv.Available)
                            _logger.LogInformation($"Version: {av.Version}, Release Date: {av.InstalledAt}");

                        // Get all versions from the database for this machine and the one from incoming data
                        var availableVersions =
                            machineStoredVersions.GetMachineStoredVersions(dv.DeviceUuid);
                        var incomingVersionNames = dv.Available
                            .Select(av => av.Version)
                            .ToHashSet();

                        // Find versions in the database that are not in the incoming data (to delete)
                        var versionsToDelete = availableVersions
                            .Where(v => !incomingVersionNames.Contains(v.VersionName))
                            .ToList();

                        // Delete versions that are no longer on the machine
                        if (versionsToDelete.Any())
                            foreach (var version in versionsToDelete)
                            {
                                _logger.LogInformation("Deleting outdated version {0} from database for Machine [{1}]",
                                    version.VersionName, dv.DeviceUuid);

                                machineStoredVersions.Remove(version);
                            }
                        else
                            _logger.LogInformation(
                                $"No database deletes required for application version for device: {dv.DeviceUuid}");

                        // Find versions from incoming data that are not in the database (to add)
                        var existingVersionNames = availableVersions
                            .Select(v => v.VersionName).ToHashSet();

                        var versionsToAdd = dv.Available
                            .Where(av => !existingVersionNames.Contains(av.Version))
                            .ToHashSet();

                        // Add new versions that are missing in the database
                        if (versionsToAdd.Any())
                            foreach (var version in versionsToAdd)
                            {
                                var applicationVerion =
                                    await applicationVersionRepository.GetByIdAsync(version.Version);

                                if (applicationVerion == null)
                                {
                                    await applicationVersionRepository.AddAsync(new ApplicationVersion()
                                    {
                                        VersionName = version.Version,
                                        Date = DateTime.UtcNow,
                                        UploadedBy = "E120023"
                                    });
                                    await applicationVersionRepository.SaveChangesAsync();
                                }

                                var newVersion = new MachineStoredVersion()
                                {
                                    MachineId = dv.DeviceUuid,
                                    VersionName = version.Version,
                                    InstalledAt = version.InstalledAt.ToDateTime()
                                };

                                _logger.LogInformation($"Adding new version {version.Version} to database");
                                await machineStoredVersions.AddAsync(newVersion);
                            }
                        else
                            _logger.LogInformation(
                                $"No database ammends required for application version for device {dv.DeviceUuid}");

                        await applicationVersionRepository.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogInformation("  AVAILABLE => (none)");
                    }
                }
            }
        }

        /// <summary>
        /// Receives AI Recommendations and sent it through singnalR to the web application
        /// </summary>
        /// <param name="gatewayId"></param>
        /// <param name="aiRec"></param>
        private async Task HandleAiRecommendations(string gatewayId, AIRecommendations aiRec)
        {
            _logger.LogInformation("AI Recommendations from [{0}]:", gatewayId);

            using var scope = _serviceScopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            foreach (var cluster in aiRec.Clusters)
            {
                _logger.LogInformation(" Cluster={0}, updated={1}", cluster.ClusterId, cluster.UpdateTime.ToDateTime());
                _logger.LogInformation("  Devices: {0}", string.Join(", ", cluster.DeviceUuids));

                await notificationService.SendMessageToUriAsync("Recommendations/Cluster",
                    JsonSerializer.Serialize(cluster));
            }

            foreach (var plot in aiRec.PlotData)
            {
                _logger.LogInformation(" Plot: dev={0}, x={1}, y={2}, cluster={3}",
                    plot.DeviceUuid, plot.X, plot.Y, plot.ClusterId);

                await notificationService.SendMessageToUriAsync($"Recommendations/Plot/{plot.DeviceUuid}",
                    JsonSerializer.Serialize(plot));
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
            using var scope = _serviceScopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();

            var user = await userRepository.GetUserByEmail(userEmail);
            if (user is null)
            {
                _logger.LogWarning("Update package upload failed. Invalid user email");
                return;
            }

            foreach (var device in targetDevices)
            {
                var machine = machineRepository.GetByIdAsync(device);
                if (machine is null) return;
            }

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

                // MetadataMap Helps with asynchronous handling, it does not send anything to the gateway
                _commandIdToMetadataMap[commandId] = new CommandMetadata
                {
                    MachineIds = targetDevices.ToList(),
                    Parameters = parameters,
                    CommandType =
                        ControlChannelCommandType
                            .Rollback, //current only rollback is available (visit in the future when there are multiple command types)
                    UserId = user.UserId
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
        public async Task SendUpdatePackageAsync(
            string gatewayId,
            string fileName,
            byte[] packageData,
            byte[] signatureData,
            string[] targetDevices,
            string appName,
            string version,
            string userEmail)
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
                PackageData = Google.Protobuf.ByteString.CopyFrom(packageData),
                SignatureData = Google.Protobuf.ByteString.CopyFrom(signatureData)
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

        /// <summary>
        /// Sends a ScheduledUpdate to the gateway, which should install the package on the specified devices.
        /// </summary>
        /// <param name="gatewayId">The gateway to send the update to</param>
        /// <param name="scheduleId">The unique ID for this scheduled update</param>
        /// <param name="deviceUuids">The devices to target</param>
        /// <param name="fileName">The name of the update package</param>
        /// <param name="packageData">The binary data of the update package</param>
        /// <param name="startTimeUtc">The start time for the update</param>
        /// <returns>Returns the task for the async operation</returns>
        public async Task SendScheduledUpdateAsync(
            string gatewayId,
            string scheduleId,
            string[] deviceUuids,
            string fileName,
            byte[] packageData,
            byte[] signatureData,
            DateTime startTimeUtc,
            string userEmail
        )
        {
            if (!_connectedGateways.TryGetValue(gatewayId, out var writer))
            {
                _logger.LogWarning("Gateway [{0}] is not connected.", gatewayId);
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var user = await userRepository.GetUserByEmail(userEmail);
            if (user is null)
            {
                _logger.LogWarning("Update package upload failed. Invalid user email");
                return;
            }

            // build ScheduledUpdate
            ScheduledUpdate su = new()
            {
                ScheduleId = scheduleId,
                FileName = fileName,
                PackageData = Google.Protobuf.ByteString.CopyFrom(packageData),
                SignatureData = Google.Protobuf.ByteString.CopyFrom(signatureData),
                StartTime = Timestamp.FromDateTime(startTimeUtc.ToUniversalTime())
            };
            su.DeviceUuids.AddRange(deviceUuids);

            ControlMessage msg = new()
            {
                SenderId = "Cloud",
                ScheduledUpdate = su
            };

            // metadata mapping helps with asynchronous handling
            _commandIdToMetadataMap[scheduleId] = new CommandMetadata
            {
                MachineIds = deviceUuids.ToList(),
                CommandType = ControlChannelCommandType.ScheduledUpdate,
                UserId = user.UserId
            };

            await writer.WriteAsync(msg);
            _logger.LogInformation("ScheduledUpdate {0} sent to gateway [{1}], devices={2}, start={3}",
                scheduleId, gatewayId, string.Join(",", deviceUuids), startTimeUtc.ToString("o"));
        }
    }

    #endregion
}

#region supporting classes

public class CommandMetadata
{
    public List<string> MachineIds { get; set; } = new();

    public string Parameters { get; set; } = string.Empty;

    public ControlChannelCommandType CommandType { get; set; }

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