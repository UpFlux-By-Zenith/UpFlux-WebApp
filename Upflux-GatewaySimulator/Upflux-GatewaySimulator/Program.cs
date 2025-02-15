using Grpc.Core;
using Grpc.Net.Client;
using UpFlux_GatewaySimulator.Protos;
using UpFlux_GatewaySimulator;
using Upflux_GatewaySimulator;

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

// A= full success B=partial success C=failure
string commandSuccessResponse = "C";
string updateSuccessResponse = "A";

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
    Console.WriteLine("6. Exit");
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
            Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
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

    // Prepare pre-defined device UUIDs and version information
    Console.WriteLine("Preparing mock data for VersionDataResponse...");

    // Example UUIDs and version configuration
    var deviceUuids = new[]
        { "13f04e54-f3ea-4071-bf3b-85077155d845", "be489eee-d175-43d8-8cb8-2a03aba05566", "should not be there" };
    var versionDataResponse = new VersionDataResponse
    {
        Success = true,
        Message = "Version data successfully retrieved."
    };

    foreach (var deviceUuid in deviceUuids)
    {
        /// set current version here
        var currentVersion = new UpFlux_GatewaySimulator.Protos.VersionInfo
        {
            Version = "v100",
            InstalledAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-7))
        };

        var availableVersions = new List<UpFlux_GatewaySimulator.Protos.VersionInfo>
        {
            new()
            {
                Version = "v1.1.0-alpha",
                InstalledAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-5))
            },
            new()
            {
                Version = "v1.2.0-test",
                InstalledAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-2))
            }
        };

        versionDataResponse.DeviceVersionsList.Add(new DeviceVersions
        {
            DeviceUuid = deviceUuid,
            Current = currentVersion,
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

// HandleCommandRequest now uses the global currentExecutionMode
async Task HandleCommandRequest(CommandRequest commandRequest)
{
	try
	{
		Console.WriteLine($"Received CommandRequest: CommandId={commandRequest.CommandId}, CommandType={commandRequest.CommandType}, Parameters={commandRequest.Parameters}");

		var targetDevices = commandRequest.TargetDevices.ToList();
		var devicesSucceeded = new List<string>();
		var devicesFailed = new List<string>();

		switch (commandSuccessResponse)
		{
			case "A": // Full success
				devicesSucceeded = targetDevices;
				break;
			case "B": // Partial success
				devicesSucceeded = targetDevices.Take(targetDevices.Count / 2).ToList();
				devicesFailed = targetDevices.Skip(targetDevices.Count / 2).ToList();
				break;
			case "C": // No success
				devicesFailed = targetDevices;
				break;
			default:
				devicesSucceeded = targetDevices;
				break;
		}

		string details = devicesFailed.Count == 0
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

        // Traverse to the root directory of the project (two levels up from bin/Debug/netX.X)
        var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));
        var saveDirectory = Path.Combine(rootDirectory, "ReceivedPackages");

        // Ensure the directory exists
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
            Console.WriteLine($"Created directory: {saveDirectory}");
        }

        // Save the update package to the directory
        var filePath = Path.Combine(saveDirectory, updatePackage.FileName);
        await File.WriteAllBytesAsync(filePath, updatePackage.PackageData.ToByteArray());

        Console.WriteLine($"UpdatePackage saved to: {filePath}");

		// Simulate success/failure for target devices
		List<string> succeededDevices = new List<string>();
		List<string> failedDevices = new List<string>();
        var targetDevices = updatePackage.TargetDevices;

		switch (updateSuccessResponse)
		{
			case "A":
				succeededDevices.AddRange(targetDevices);
				break;
			case "B":
				int halfCount = targetDevices.Count / 2;
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
		string detailMsg = $"Succeeded on: {string.Join(", ", succeededDevices)}; Failed on: {string.Join(", ", failedDevices)}.";

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