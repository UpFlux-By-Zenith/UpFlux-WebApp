using Grpc.Core;
using System.Collections.Concurrent;
using UpFlux_WebService.Protos;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Core.Models;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Upflux_WebService.Services.Interfaces;
using static Upflux_WebService.Services.EntityQueryService;


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
                        await ProcessMachineRollback(machineId, metadata.Parameters, req);

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

        /// <summary>
        /// update application database if rollback is succesful
        /// </summary>
        private async Task ProcessMachineRollback(string machineId, string parameters, CommandResponse req)
        {
            // need to keep track the user who initiated rollback (since it affects application table) - not done
            // might want to add engineer email parameter at SendUpdatePackage (used by controller), update interface, and update controller

            var alert = new AlertMessage();
            using var scope = _serviceScopeFactory.CreateScope();
            var applicationRepository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                var application = await applicationRepository.GetByMachineId(machineId);
                if (application == null)
                {
                    alert.Message = $"Failed to process rollback for MachineId: {machineId}. No application found.";
                    _logger.LogWarning("No application found for MachineId: {0}. Skipping processing.", machineId);
                    await notificationService.SendMessageToUriAsync("alert", alert.ToString());


                    return;
                }

                if (req.Success)
                {
                    _logger.LogInformation(
                        "Processing successful CommandResponse for MachineId: {0}, CommandId: {1}, Parameters: {2}",
                        machineId, req.CommandId, parameters);

                    // update
                    application.CurrentVersion = parameters;

                    applicationRepository.Update(application);
                    await applicationRepository.SaveChangesAsync();

                    // send succes notification
                    var successMessage = $"MachineId: {machineId} successfully rolled back to version: {parameters}.";
                    alert.Message = successMessage;
                    await notificationService.SendMessageToUriAsync("alert", alert.ToString());

                    _logger.LogInformation(
                        "Successfully processed rollback for MachineId: {0}, CommandId: {1}. Updated to version: {2}",
                        machineId, req.CommandId, parameters);
                }
                else
                {
                    // failure
                    _logger.LogWarning("Rollback command failed for MachineId: {0}, CommandId: {1}.", machineId,
                        req.CommandId);
                    var failureMessage = $"Rollback failed for MachineId: {machineId}. CommandId: {req.CommandId}.";
                    alert.Message = failureMessage;
                    await notificationService.SendMessageToUriAsync("alert", alert.ToString());
                }
            }
            catch (Exception ex)
            {
                // notifcation for error?
                _logger.LogError(ex, "Error while processing rollback for MachineId: {0}, CommandId: {1}.", machineId,
                    req.CommandId);
                alert.Message =
                    $"An error occurred while processing rollback for MachineId: {machineId}. CommandId: {req.CommandId}. Error: {ex.Message}";
                await notificationService.SendMessageToUriAsync("alert", alert.ToString());
            }
        }

        /// <summary>
        /// update application database if update is succesful
        /// </summary>
        private async Task HandleUpdateAcknowledged(string gatewayId, UpdateAck req)
        {
            // need to keep track of the id of user who initiated the update (since it affects application table) - not done
            // might want to add engineer email parameter at SendUpdatePackage (used by controller), update interface, and update controller

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

                foreach (var deviceUuid in metadata.TargetDevices)
                {
                    var application = await applicationRepository.GetByMachineId(deviceUuid);

                    if (application == null)
                    {
                        _logger.LogWarning("No application found for DeviceUuid: {0}. Skipping update.", deviceUuid);

                        await notificationService.SendMessageToUriAsync("Alert/Update",
                            $"Update failed for DeviceUuid: {deviceUuid}. Application not found.");
                        continue;
                    }

                    if (req.Success)
                    {
                        // update
                        application.CurrentVersion = metadata.Version;
                        application.AppName = metadata.AppName;

                        applicationRepository.Update(application);
                        await applicationRepository.SaveChangesAsync();

                        _logger.LogInformation(
                            "Successfully updated application for DeviceUuid: {0} to version: {1}, AppName: {2}",
                            deviceUuid, metadata.Version, metadata.AppName);

                        // notification
                        await notificationService.SendMessageToUriAsync("Alert/Update",
                            $"DeviceUuid: {deviceUuid} successfully updated to version: {metadata.Version}, AppName: {metadata.AppName}.");
                    }
                    else
                    {
                        _logger.LogWarning("Update failed for DeviceUuid: {0}. FileName: {1}", deviceUuid,
                            req.FileName);

                        //notification
                        await notificationService.SendMessageToUriAsync("Alert/Update",
                            $"Update failed for DeviceUuid: {deviceUuid}. FileName: {req.FileName}, AppName: {metadata.AppName}.");
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
                _logger.LogInformation("VersionDataResponse from [{0}]: {1}", gatewayId, resp.Message);

                using var scope = _serviceScopeFactory.CreateScope();
                var applicationRepository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                foreach (var dv in resp.DeviceVersionsList)
                {
                    _logger.LogInformation(" Device={0}", dv.DeviceUuid);

                    var application = await applicationRepository.GetByMachineId(dv.DeviceUuid);
                    if (application is null)
                    {
                        // or add it to application database?
                        _logger.LogInformation(
                            $"machine: [{dv.DeviceUuid}] have an applicatiuon running that is not in the database");
                        continue;
                    }

                    if (dv.Current != null)
                    {
                        if (application.CurrentVersion != dv.Current.Version)
                        {
                            application.CurrentVersion = dv.Current.Version;
                            applicationRepository.Update(application);
                            await applicationRepository.SaveChangesAsync();
                        }

                        var installed = dv.Current.InstalledAt.ToDateTime();
                        _logger.LogInformation("  CURRENT => Version={0}, InstalledAt={1}", dv.Current.Version,
                            installed);
                    }
                    else
                    {
                        _logger.LogInformation("  CURRENT => (none)");
                    }

                    if (dv.Available.Count > 0)
                    {
                        List<string> versions = new();
                        _logger.LogInformation("  AVAILABLE:");
                        foreach (var av in dv.Available) versions.Add(av.Version);
                        // var newVersion = new ApplicationVersion
                        // {
                        //     AppId = application.AppId,
                        //     VersionName = av.Version,
                        //     UpdatedBy = "E11111", // Adjust this based on actual updater
                        //     Date = av.InstalledAt.ToDateTime(),
                        //     DeviceUuid = dv.DeviceUuid
                        // };
                        //
                        // await notificationService.SendMessageToUriAsync("versions",
                        //     JsonConvert.SerializeObject(newVersion));
                        // var existingVersion = application.Versions.FirstOrDefault(v => v.VersionName == av.Version);
                        // if (existingVersion == null)
                        // {
                        //     var newVersion = new ApplicationVersion
                        //     {
                        //         AppId = application.AppId,
                        //         VersionName = av.Version,
                        //         UpdatedBy = "E11111", // Adjust this based on actual updater
                        //         Date = av.InstalledAt.ToDateTime()
                        //     };
                        //     
                        //     application.Versions.Add(newVersion);
                        //     _logger.LogInformation("Added new available version: {0} for machine {1}", av.Version,
                        //         dv.DeviceUuid);
                        // }
                        // else
                        // {
                        //     _logger.LogInformation("Version {0} already exists for machine {1}", av.Version,
                        //         dv.DeviceUuid);
                        // }
                        var newVersion = new ApplicationVersions
                        {
                            AppId = application.AppId,
                            VersionNames = versions,
                            UpdatedBy = "E11111", // Adjust this based on actual updater
                            DeviceUuid = dv.DeviceUuid
                        };

                        await notificationService.SendMessageToUriAsync("versions",
                            JsonConvert.SerializeObject(newVersion));
                        await applicationRepository.SaveChangesAsync();
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
        public async Task SendCommandToGatewayAsync(string gatewayId,
            string commandId,
            CommandType cmdType,
            string parameters,
            params string[] targetDevices)
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
            string[] targetDevices, string appName, string version)
        {
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
                FileName = fileName
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
                SenderId = "CloudSim",
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
}

public class UpdateMetadata
{
    public List<string> TargetDevices { get; set; } = new();
    public string AppName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

#endregion