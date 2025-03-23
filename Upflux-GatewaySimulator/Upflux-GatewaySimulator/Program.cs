using Grpc.Core;
using Grpc.Net.Client;
using UpFlux_GatewaySimulator.Protos;
using Upflux_GatewaySimulator;
using Google.Protobuf.WellKnownTypes;

Console.WriteLine("Starting gRPC Client...");

// Create the gRPC channel
var channel = GrpcChannel.ForAddress("http://localhost:5002", new GrpcChannelOptions
{
    //Credentials = ChannelCredentials.Insecure, 
    MaxReceiveMessageSize = 200 * 1024 * 1024,
    MaxSendMessageSize = 200 * 1024 * 1024
});

// Create the client from the generated code
var client = new ControlChannel.ControlChannelClient(channel);

// Define a single SenderId to use across all requests
var senderId = "gateway-patrick-1234";
Console.WriteLine($"Using SenderId: {senderId}");

// CancellationToken to allow clean termination
var cts = new CancellationTokenSource();

// Start the streaming RPC
using var call = client.OpenControlChannel(cancellationToken: cts.Token);

// Instantiate the mock data generator
var mockDataGenerator = new MockDataGenerator();

// Dictionary to store and reuse UUIDs for monitoring data
var monitoringDataDict = new Dictionary<string, MonitoringDataMessage>();

// ---------------------------------------------Update/Rollback Success type Related Debugging Variables--------------------------------------------
// A= full success B=partial success C=failure
var commandSuccessResponse = "A";
var updateSuccessResponse = "A";

//----------------------------------------------AI Recommendation Related Debugging Variables--------------------------------------------------------
// Device UUID
List<string> deviceUUIDs =
[
    "13f04e54-f3ea-4071-bf3b-85077155d845", "7f36d549-23fb-4e2c-99fb-0f791d493dd1",
    "d3b0580e-db6b-11ef-bd3e-2ccf677985c6"
];

// Ai recommendation generator
AIRecommendationSender aIRecommendationSender = new(call, senderId, deviceUUIDs);

// Task for receiving messages from the server
var receiveTask = Task.Run(async () =>
{
    try
    {
        await foreach (var response in call.ResponseStream.ReadAllAsync(cts.Token))
            if (response.PayloadCase == ControlMessage.PayloadOneofCase.LicenseResponse)
            {
                var licenseResponse = response.LicenseResponse;
                Console.WriteLine(
                    $"Received LicenseResponse: DeviceUuid={licenseResponse.DeviceUuid}, Approved={licenseResponse.Approved}, " +
                    $"License={licenseResponse.License}, ExpirationDate={licenseResponse.ExpirationDate.ToDateTime()}");
            }
            else if (response.PayloadCase == ControlMessage.PayloadOneofCase.LogRequest)
            {
                await HandleLogRequest(response.LogRequest);
            }
            else if (response.PayloadCase == ControlMessage.PayloadOneofCase.CommandRequest)
            {
                await HandleCommandRequest(response.CommandRequest);
            }
            else if (response.PayloadCase == ControlMessage.PayloadOneofCase.UpdatePackage)
            {
                await HandleUpdatePackage(response.UpdatePackage);
            }
            else if (response.PayloadCase == ControlMessage.PayloadOneofCase.VersionDataRequest)
            {
                await HandleVersionDataRequest(response.VersionDataRequest);
            }
            else if (response.PayloadCase == ControlMessage.PayloadOneofCase.ScheduledUpdate)
            {
                await HandleScheduledUpdate(response.ScheduledUpdate);
            }
            else
            {
                Console.WriteLine($"Received: SenderId={response.SenderId}, Description={response.Description}");
            }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Receive task canceled.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error receiving messages: {ex.Message}");
    }
});

// Console menu for sending data
void ShowMenu()
{
    Console.WriteLine("\nChoose an action:");
    Console.WriteLine("1. Send Monitoring Data");
    Console.WriteLine("2. Send Monitoring Data Batch");
    Console.WriteLine("3. Send License Request");
    Console.WriteLine("4. Send Log Upload");
    Console.WriteLine("5. Send Alert");
    Console.WriteLine("6. Send AI Recommendations");
    Console.WriteLine("7. Send Device Status");
    Console.WriteLine("8. Exit");
}

async Task SendMonitoringData()
{
    Console.WriteLine("Enter UUID for monitoring data (leave blank to create a new one):");
    var inputUuid = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(inputUuid))
    {
        // Generate a new UUID
        inputUuid = Guid.NewGuid().ToString();
        Console.WriteLine($"New UUID generated: {inputUuid}");
    }

    // Create a MockDataGenerator instance for this UUID
    var mockDataGenerator = new MachineDataGenerator(inputUuid);

    // Generate initial data and store it
    var monitoringData = mockDataGenerator.GenerateMockData();

    try
    {
        Console.WriteLine($"Started sending MonitoringData for UUID={inputUuid}...");

        while (true)
        {
            var controlMessage = new ControlMessage
            {
                SenderId = senderId,
                MonitoringData = monitoringData
            };

            // Send the control message
            await call.RequestStream.WriteAsync(controlMessage);
            Console.WriteLine($"Sent MonitoringData: UUID={inputUuid}, Timestamp={DateTime.UtcNow}");

            // Update the mock data for the next send
            foreach (var data in monitoringData.AggregatedData) mockDataGenerator.UpdateMockData(data);

            // Wait for 5 seconds before the next update
            await Task.Delay(5000);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending monitoring data: {ex.Message}");
    }
}

async Task SendLicenseRequest()
{
    try
    {
        Console.WriteLine("Enter Device UUID (leave blank for random):");
        var deviceUuid = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(deviceUuid)) deviceUuid = Guid.NewGuid().ToString();

        Console.WriteLine("Is this a renewal request? (y/n):");
        var renewalInput = Console.ReadLine();
        var isRenewal = renewalInput?.Trim().ToLower() == "y";

        var licenseRequest = new LicenseRequest
        {
            DeviceUuid = deviceUuid,
            IsRenewal = isRenewal
        };

        var controlMessage = new ControlMessage
        {
            SenderId = senderId,
            LicenseRequest = licenseRequest
        };

        await call.RequestStream.WriteAsync(controlMessage);
        Console.WriteLine($"Sent LicenseRequest: UUID={deviceUuid}, IsRenewal={isRenewal}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending license request: {ex.Message}");
    }
}

async Task SendLogUpload()
{
    try
    {
        Console.WriteLine("Enter Device UUID for log upload:");
        var deviceUuid = Console.ReadLine() ?? Guid.NewGuid().ToString();

        Console.WriteLine("Enter file name for the log (e.g., 'log.txt'):");
        var fileName = Console.ReadLine() ?? "log.txt";

        Console.WriteLine("Enter the size of the log data (in bytes):");
        if (!int.TryParse(Console.ReadLine(), out var logSize)) logSize = 1024; // Default size

        // Generate mock log data
        var logData = new byte[logSize];
        new Random().NextBytes(logData);

        var logUpload = new LogUpload
        {
            DeviceUuid = deviceUuid,
            FileName = fileName,
            Data = Google.Protobuf.ByteString.CopyFrom(logData)
        };

        var controlMessage = new ControlMessage
        {
            SenderId = senderId,
            LogUpload = logUpload
        };

        await call.RequestStream.WriteAsync(controlMessage);
        Console.WriteLine($"Sent LogUpload: DeviceUuid={deviceUuid}, FileName={fileName}, Size={logSize} bytes");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending log upload: {ex.Message}");
    }
}

async Task SendAlert()
{
    try
    {
        Console.WriteLine("Enter alert level (e.g., 'INFO', 'WARNING', 'ERROR'):");
        var level = Console.ReadLine() ?? "INFO";

        Console.WriteLine("Enter alert message:");
        var message = Console.ReadLine() ?? "Default alert message";

        Console.WriteLine("Enter source of the alert (e.g., 'Sensor', 'System'):");
        var source = Console.ReadLine() ?? "System";

        var alertMessage = new AlertMessage
        {
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            Level = level,
            Message = message,
            Source = source
        };

        var controlMessage = new ControlMessage
        {
            SenderId = senderId,
            AlertMessage = alertMessage
        };

        await call.RequestStream.WriteAsync(controlMessage);
        Console.WriteLine($"Sent AlertMessage: Level={level}, Message={message}, Source={source}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending alert message: {ex.Message}");
    }
}

async Task SendLogRequest()
{
    try
    {
        Console.WriteLine("Enter UUIDs of devices for the log request, separated by commas:");
        var inputUuids = Console.ReadLine();
        var deviceUuids = (inputUuids ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (deviceUuids.Length == 0)
        {
            Console.WriteLine("No device UUIDs entered. Aborting log request.");
            return;
        }

        var logRequestMessage = new LogRequestMessage();
        logRequestMessage.DeviceUuids.AddRange(deviceUuids);

        var controlMessage = new ControlMessage
        {
            SenderId = senderId,
            LogRequest = logRequestMessage
        };

        await call.RequestStream.WriteAsync(controlMessage);
        Console.WriteLine($"Sent LogRequest for devices: {string.Join(", ", deviceUuids)}");

        // Wait for server to respond with logs or success messages
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending log request: {ex.Message}");
    }
}

async Task HandleLogRequest(LogRequestMessage logRequest)
{
    try
    {
        foreach (var deviceUuid in logRequest.DeviceUuids)
        {
            Console.WriteLine($"Generating log for device: {deviceUuid}");

            // Generate a mock log file
            var logContent = $"Mock log data for device {deviceUuid} at {DateTime.UtcNow}";
            var logData = System.Text.Encoding.UTF8.GetBytes(logContent);

            // Simulate saving the log to a file (if required)
            var fileName = $"log-{deviceUuid}.txt";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            await File.WriteAllBytesAsync(filePath, logData);

            Console.WriteLine($"Log saved locally at {filePath}.");

            // Send LogUpload message back to the server
            var logUpload = new LogUpload
            {
                DeviceUuid = deviceUuid,
                FileName = fileName,
                Data = Google.Protobuf.ByteString.CopyFrom(logData)
            };

            var controlMessage = new ControlMessage
            {
                SenderId = senderId,
                LogUpload = logUpload
            };

            await call.RequestStream.WriteAsync(controlMessage);
            Console.WriteLine($"Sent LogUpload for device: {deviceUuid}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error handling LogRequest: {ex.Message}");
    }
}

/// this will send
async Task HandleVersionDataRequest(VersionDataRequest request)
{
    Console.WriteLine("Received VersionDataRequest.");

    // Traverse to the root directory of the project
    var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));

    // Define directories
    var currentDirectory = Path.Combine(rootDirectory, "Current");
    var availableDirectory = Path.Combine(rootDirectory, "Available");

    // Ensure directories exist
    Directory.CreateDirectory(currentDirectory);
    Directory.CreateDirectory(availableDirectory);

    // Extract the version number from the "Current" package
    var currentFiles = Directory.GetFiles(currentDirectory);
    var currentVersion = currentFiles.Length > 0
        ? ExtractVersionFromFilename(Path.GetFileName(currentFiles[0]))
        : "No current package";

    // Extract the version numbers from "Available" packages
    var availableFiles = Directory.GetFiles(availableDirectory);
    var availableVersions = availableFiles
        .Select(file => new UpFlux_GatewaySimulator.Protos.VersionInfo
        {
            Version = ExtractVersionFromFilename(Path.GetFileName(file)),
            InstalledAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-2)) // Example timestamp
        })
        .ToList();

    // Example UUIDs and version configuration
    var deviceUuids = new[]
    {
        "13f04e54-f3ea-4071-bf3b-85077155d845", "7f36d549-23fb-4e2c-99fb-0f791d493dd1",
        "d3b0580e-db6b-11ef-bd3e-2ccf677985c6"
    };

    var versionDataResponse = new VersionDataResponse
    {
        Success = true,
        Message = "Version data successfully retrieved."
    };

    foreach (var deviceUuid in deviceUuids)
    {
        // Set current version using the extracted version
        var currentVersionInfo = new UpFlux_GatewaySimulator.Protos.VersionInfo
        {
            Version = currentVersion,
            InstalledAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-7)) // Example timestamp
        };

        versionDataResponse.DeviceVersionsList.Add(new DeviceVersions
        {
            DeviceUuid = deviceUuid,
            Current = currentVersionInfo,
            Available = { availableVersions }
        });
    }

    // Send the response back to the server
    var controlMessage = new ControlMessage
    {
        SenderId = senderId,
        VersionDataResponse = versionDataResponse
    };

    try
    {
        await call.RequestStream.WriteAsync(controlMessage);
        Console.WriteLine("Sent VersionDataResponse to server.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending VersionDataResponse: {ex.Message}");
    }
}

// Helper function to extract version number from filename
string ExtractVersionFromFilename(string filename)
{
    // Example filename: "upflux-monitoring-service_1.1.21_armhf.deb"
    var match = System.Text.RegularExpressions.Regex.Match(filename, @"_(\d+\.\d+\.\d+)_");
    return match.Success ? match.Groups[1].Value : "Unknown";
}

// HandleCommandRequest now uses the global currentExecutionMode
async Task HandleCommandRequest(CommandRequest commandRequest)
{
    try
    {
        Console.WriteLine(
            $"Received CommandRequest: CommandId={commandRequest.CommandId}, CommandType={commandRequest.CommandType}, Parameters={commandRequest.Parameters}");

        var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));
        var currentDirectory = Path.Combine(rootDirectory, "Current");
        var availableDirectory = Path.Combine(rootDirectory, "Available");

        // Ensure directories exist
        Directory.CreateDirectory(currentDirectory);
        Directory.CreateDirectory(availableDirectory);

        var targetDevices = commandRequest.TargetDevices.ToList();
        var devicesSucceeded = new List<string>();
        var devicesFailed = new List<string>();

        if (commandRequest.CommandType == CommandType.Rollback && !string.IsNullOrEmpty(commandRequest.Parameters))
        {
            var rollbackVersion = commandRequest.Parameters.Trim(); // Version number e.g., "1.1.21"

            // Find the rollback package in "Available" that matches the version
            var availableFiles = Directory.GetFiles(availableDirectory);
            var rollbackFilePath =
                availableFiles.FirstOrDefault(f => ExtractVersionFromFilename(Path.GetFileName(f)) == rollbackVersion);

            if (!string.IsNullOrEmpty(rollbackFilePath) && File.Exists(rollbackFilePath))
            {
                var currentFiles = Directory.GetFiles(currentDirectory);
                if (currentFiles.Length > 0)
                {
                    var currentFilePath = currentFiles[0]; // Assuming only one file in "Current"
                    var currentFileName = Path.GetFileName(currentFilePath);
                    var archivedFilePath = Path.Combine(availableDirectory, currentFileName);

                    // Ensure we don't duplicate versions in "Available"
                    if (!File.Exists(archivedFilePath))
                    {
                        File.Move(currentFilePath, archivedFilePath);
                        Console.WriteLine($"Moved current package to Available: {archivedFilePath}");
                    }
                    else
                    {
                        // Delete the current file instead of moving it (as it's already in Available)
                        File.Delete(currentFilePath);
                        Console.WriteLine(
                            $"Deleted current package as it already exists in Available: {currentFilePath}");
                    }
                }

                // Move the rollback package from "Available" to "Current"
                var newCurrentFilePath = Path.Combine(currentDirectory, Path.GetFileName(rollbackFilePath));
                File.Move(rollbackFilePath, newCurrentFilePath);
                Console.WriteLine($"Rollback successful! Moved {Path.GetFileName(rollbackFilePath)} to Current.");

                devicesSucceeded = targetDevices;
            }
            else
            {
                Console.WriteLine(
                    $"Rollback failed: No matching file found in Available for version {rollbackVersion}");
                devicesFailed = targetDevices;
            }
        }
        else
        {
            Console.WriteLine("Invalid rollback command or missing rollback version.");
            devicesFailed = targetDevices;
        }

        // Generate response message
        var details = devicesFailed.Count == 0
            ? $"Rollback succeeded on {string.Join(", ", devicesSucceeded)}"
            : devicesSucceeded.Count == 0
                ? $"Rollback failed on {string.Join(", ", devicesFailed)}"
                : $"Rollback partial success: succeeded on {string.Join(", ", devicesSucceeded)}, failed on {string.Join(", ", devicesFailed)}";

        var commandResponse = new CommandResponse
        {
            CommandId = commandRequest.CommandId,
            Success = devicesFailed.Count == 0,
            Details = details
        };

        var controlMessage = new ControlMessage
        {
            SenderId = senderId,
            CommandResponse = commandResponse
        };

        await call.RequestStream.WriteAsync(controlMessage);
        Console.WriteLine($"Sent CommandResponse for CommandId={commandRequest.CommandId} with details: {details}");

        //simulate successfull update
        foreach (var uuid in devicesSucceeded)
        {
            var alert = new AlertMessage
            {
                Timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                Level = "Information",
                Message = "Update to version " + commandRequest.Parameters + " installed successfully",
                Source = $"Device-{uuid}"
            };

            var message = new ControlMessage
            {
                SenderId = senderId,
                AlertMessage = alert
            };
            await call.RequestStream.WriteAsync(message);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error handling CommandRequest: {ex.Message}");
    }
}

async Task SendMonitoringDataBatch()
{
    Console.WriteLine("Enter the number of AggregatedData objects to send in this batch:");
    if (!int.TryParse(Console.ReadLine(), out var batchSize) || batchSize <= 0)
    {
        Console.WriteLine("Invalid input. Aborting.");
        return;
    }

    Console.WriteLine("Enter UUID for the batch (leave blank to create a new one):");
    var batchUuid = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(batchUuid))
    {
        batchUuid = Guid.NewGuid().ToString();
        Console.WriteLine($"Generated new UUID for batch: {batchUuid}");
    }

    // Generate a batch of AggregatedData
    var monitoringDataMessage = new MonitoringDataMessage();
    for (var i = 0; i < batchSize; i++)
    {
        var aggregatedData = mockDataGenerator.GenerateMockAggregatedData(batchUuid);
        monitoringDataMessage.AggregatedData.Add(aggregatedData);
    }

    try
    {
        var controlMessage = new ControlMessage
        {
            SenderId = senderId,
            MonitoringData = monitoringDataMessage
        };

        await call.RequestStream.WriteAsync(controlMessage);
        Console.WriteLine($"Sent batch of MonitoringData: UUID={batchUuid}, BatchSize={batchSize}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending monitoring data batch: {ex.Message}");
    }
}

async Task HandleUpdatePackage(UpdatePackage updatePackage)
{
    try
    {
        Console.WriteLine(
            $"Received UpdatePackage: FileName={updatePackage.FileName}, Size={updatePackage.PackageData.Length} bytes");

        // Traverse to the root directory of the project
        var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));

        // Define directories
        var currentDirectory = Path.Combine(rootDirectory, "Current");
        var availableDirectory = Path.Combine(rootDirectory, "Available");

        // Ensure directories exist
        Directory.CreateDirectory(currentDirectory);
        Directory.CreateDirectory(availableDirectory);

        // Get any existing file in "Current" (since only one should exist)
        var existingFiles = Directory.GetFiles(currentDirectory);

        if (existingFiles.Length > 0)
        {
            var existingFilePath = existingFiles[0]; // Assuming only one file exists in "Current"
            var existingFileName = Path.GetFileName(existingFilePath);
            var archivedFilePath = Path.Combine(availableDirectory, existingFileName);

            // Check if the same package already exists in "Available"
            if (!File.Exists(archivedFilePath))
            {
                // Move the current package to "Available" only if it's not there already
                File.Move(existingFilePath, archivedFilePath);
                Console.WriteLine($"Moved existing package to Available: {archivedFilePath}");
            }
            else
            {
                // Delete the existing "Current" package since it's already in "Available"
                File.Delete(existingFilePath);
                Console.WriteLine($"Deleted existing Current package as it's already in Available: {existingFilePath}");
            }
        }

        // Save the new package to "Current" (ensuring only one package remains)
        var newFilePath = Path.Combine(currentDirectory, updatePackage.FileName);
        await File.WriteAllBytesAsync(newFilePath, updatePackage.PackageData.ToByteArray());
        Console.WriteLine($"UpdatePackage saved to: {newFilePath}");

        // Simulate success/failure for target devices
        List<string> succeededDevices = new();
        List<string> failedDevices = new();
        var targetDevices = updatePackage.TargetDevices;

        switch (updateSuccessResponse)
        {
            case "A":
                succeededDevices.AddRange(targetDevices);
                break;
            case "B":
                var halfCount = targetDevices.Count / 2;
                succeededDevices.AddRange(targetDevices.Take(halfCount));
                failedDevices.AddRange(targetDevices.Skip(halfCount));
                break;
            case "C":
                failedDevices.AddRange(targetDevices);
                break;
            default:
                Console.WriteLine("Invalid simulatedOutcome provided. Defaulting to success.");
                succeededDevices.AddRange(targetDevices);
                break;
        }

        // Create detail message
        var detailMsg =
            $"Succeeded on: {string.Join(", ", succeededDevices)}; Failed on: {string.Join(", ", failedDevices)}.";

        // Send an acknowledgment back to the server
        var updateAck = new UpdateAck
        {
            FileName = updatePackage.FileName,
            Success = failedDevices.Count == 0, // Success if no failures
            Details = detailMsg
        };

        var ackMessage = new ControlMessage
        {
            SenderId = senderId,
            UpdateAck = updateAck
        };

        await call.RequestStream.WriteAsync(ackMessage);
        Console.WriteLine($"Sent UpdateAck for FileName={updatePackage.FileName}: {detailMsg}");

        foreach (var uuid in succeededDevices)
        {
            var alert = new AlertMessage
            {
                Timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                Level = "Information",
                Message = "Update to version " + updatePackage.FileName + " installed successfully",
                Source = $"Device-{uuid}"
            };

            var message = new ControlMessage
            {
                SenderId = senderId,
                AlertMessage = alert
            };
            await call.RequestStream.WriteAsync(message);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error handling UpdatePackage: {ex.Message}");

        // Send a failure acknowledgment back to the server
        var updateAck = new UpdateAck
        {
            FileName = updatePackage.FileName,
            Success = false,
            Details = $"Failed to process the update package: {ex.Message}"
        };

        var ackMessage = new ControlMessage
        {
            SenderId = senderId,
            UpdateAck = updateAck
        };

        await call.RequestStream.WriteAsync(ackMessage);
        Console.WriteLine($"Sent failure UpdateAck for FileName={updatePackage.FileName}");
    }
}


async Task SendDeviceStatus()
{
    try
    {
        Console.Write("\nEnter UUID of the device: ");
        var deviceUuid = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(deviceUuid))
        {
            Console.WriteLine("❌ Invalid UUID. Aborting.");
            return;
        }

        Console.Write("Is the device online? (y/n): ");
        var isOnlineInput = Console.ReadLine()?.Trim().ToLower();
        var isOnline = isOnlineInput == "y";

        var deviceStatus = new DeviceStatus
        {
            DeviceUuid = deviceUuid,
            IsOnline = isOnline,
            LastSeen = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        var controlMessage = new ControlMessage
        {
            SenderId = senderId,
            Description = "Device Status Update",
            DeviceStatus = deviceStatus
        };

        await call.RequestStream.WriteAsync(controlMessage);
        Console.WriteLine($"📤 Sent DeviceStatus: UUID={deviceUuid}, Online={isOnline}, LastSeen={DateTime.UtcNow}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error sending DeviceStatus: {ex.Message}");
    }
}

async Task HandleScheduledUpdate(ScheduledUpdate scheduledUpdate)
{
    try
    {
        Console.WriteLine($"\n📦 Received Scheduled Update:");
        Console.WriteLine($"  - Schedule ID: {scheduledUpdate.ScheduleId}");
        Console.WriteLine($"  - Cluster: {scheduledUpdate.ClusterId}");
        Console.WriteLine($"  - File Name: {scheduledUpdate.FileName}");
        Console.WriteLine($"  - Target Devices: {string.Join(", ", scheduledUpdate.DeviceUuids)}");
        Console.WriteLine($"  - Start Time: {scheduledUpdate.StartTime.ToDateTime()}");

        // Save package data to disk

        var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));

        // Define the directory for scheduled updates
        var scheduledUpdateDirectory = Path.Combine(rootDirectory, "ScheduledUpdates");

        // Ensure the directory exists
        Directory.CreateDirectory(scheduledUpdateDirectory);

        // Save package data to disk
        var savePath = Path.Combine(scheduledUpdateDirectory, scheduledUpdate.FileName);
        await File.WriteAllBytesAsync(savePath, scheduledUpdate.PackageData.ToByteArray());

        Console.WriteLine($"✅ Package saved at {savePath}");

        var startUtc = scheduledUpdate.StartTime.ToDateTime();

        // Send acknowledgment back to server
        var resp = new CommandResponse
        {
            CommandId = scheduledUpdate.ScheduleId,
            Success = true,
            Details = $"Scheduled update stored for {startUtc:o}"
        };
        var msg = new ControlMessage
        {
            SenderId = senderId,
            CommandResponse = resp
        };

        await call.RequestStream.WriteAsync(msg);

        Console.WriteLine($"📤 Sent command response for Scheduled Update {scheduledUpdate.ScheduleId}.");

        //simulate successfull update
        foreach (var uuid in scheduledUpdate.DeviceUuids)
        {
            var alert = new AlertMessage
            {
                Timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                Level = "Information",
                Message = "Update to version " + scheduledUpdate.FileName + " installed successfully",
                Source = $"Device-{uuid}"
            };

            var message = new ControlMessage
            {
                SenderId = senderId,
                AlertMessage = alert
            };
            await call.RequestStream.WriteAsync(message);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error processing ScheduledUpdate: {ex.Message}");
    }
}

// Run the menu loop
var running = true;
while (running)
{
    ShowMenu();
    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await SendMonitoringData();
            break;
        case "2":
            await SendMonitoringDataBatch();
            break;
        case "3":
            await SendLicenseRequest();
            break;
        case "4":
            await SendLogUpload();
            break;
        case "5":
            await SendAlert();
            break;
        case "6":
            await aIRecommendationSender.SendAIRecommendationsAsync();
            break;
        case "7":
            await SendDeviceStatus();
            break;
        case "8":
            Console.WriteLine("Exiting...");
            running = false;
            break;
        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;
    }
}

// Complete the request stream and wait for the receive task
cts.Cancel();
await call.RequestStream.CompleteAsync();
await receiveTask;

Console.WriteLine("Client has shut down.");