using System.IO.Compression;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
	/// <summary>
	/// Service related to log file
	/// </summary>
	public class LogFileService : ILogFileService
	{
		private readonly IControlChannelService _controlChannelService;
		private readonly ILogger<LogFileService> _logger;
		private readonly IMachineRepository _machineRepository;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="controlChannelService"></param>
		/// <param name="logger"></param>
		public LogFileService(IControlChannelService controlChannelService, ILogger<LogFileService> logger, IMachineRepository machineRepository)
		{
			_controlChannelService = controlChannelService;
			_logger = logger;
			_machineRepository = machineRepository;
		}

		/// <summary>
		/// Creates a ZIP archive of log files in the specified directory
		/// </summary>
		/// <param name="logDirectory"></param>
		/// <returns>MemoryStream containing the ZIP archive</returns>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
		public MemoryStream CreateLogArchive(string logDirectory)
		{
			_logger.LogInformation("Starting to create log archive for directory: {LogDirectory}", logDirectory);

			if (!Directory.Exists(logDirectory))
			{
				_logger.LogError("Log directory does not exist: {LogDirectory}", logDirectory);
				throw new DirectoryNotFoundException("Log directory does not exist.");
			}

			var logFiles = Directory.GetFiles(logDirectory, "*");

			if (!logFiles.Any())
			{
				_logger.LogWarning("No log files found in directory: {LogDirectory}", logDirectory);
				throw new FileNotFoundException("No log files found in the directory.");
			}

			var memoryStream = new MemoryStream();

			using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
			{
				foreach (var logFile in logFiles)
				{
					_logger.LogInformation("Adding log file to archive: {LogFile}", logFile);
					var entry = archive.CreateEntry(Path.GetFileName(logFile));

					// Use FileShare.ReadWrite to handle files that are in use
					using var entryStream = entry.Open();
					using var logFileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);	
					logFileStream.CopyTo(entryStream);
				}
			}

			memoryStream.Position = 0; // Reset stream position for reading
			_logger.LogInformation("Log archive creation completed for directory: {LogDirectory}", logDirectory);
			return memoryStream;
		}

		/// <summary>
		/// Requests logs via ControlChannelService and creates a ZIP archive of the logs
		/// </summary>
		/// <param name="logDirectory"></param>
		/// <param name="deviceUuids"></param>
		/// <returns>MemoryStream containing the ZIP archive</returns>
		public async Task<MemoryStream> RequestLogsAndCreateArchiveAsync(string logDirectory, string[] deviceUuids)
		{
			_logger.LogInformation("Requesting logs and creating archive for directory: {LogDirectory}", logDirectory);

			// local testing
			//if (Directory.Exists(logDirectory))
			//{
			//	_logger.LogInformation("Deleting existing log directory: {LogDirectory}", logDirectory);
			//	Directory.Delete(logDirectory, true);
			//}
			//Directory.CreateDirectory(logDirectory);

			// deployment
			//if (!Directory.Exists(logDirectory))
			//{
			//	_logger.LogError("Log directory does not exist: {LogDirectory}", logDirectory);
			//	throw new DirectoryNotFoundException("Log directory does not exist.");
			//}
			ClearDirectoryContents(logDirectory);


			_logger.LogInformation("Sending log request for device UUIDs: {DeviceUuids}", string.Join(", ", deviceUuids));
			await _controlChannelService.SendLogRequestAsync("gateway-patrick-1234", deviceUuids);

			// Wait until files are received or timeout after a maximum period
			int maxRetries = 10; // 10 retries with a 1-second delay = 10 seconds max wait
			int retryCount = 0;
			while (!Directory.EnumerateFiles(logDirectory).Any())
			{
				if (retryCount++ >= maxRetries)
				{
					_logger.LogError("Timeout waiting for logs to be received in directory: {LogDirectory}", logDirectory);
					throw new TimeoutException("No logs were received within the expected time frame.");
				}

				_logger.LogWarning("No logs found yet. Retrying {RetryCount}/{MaxRetries}", retryCount, maxRetries);
				await Task.Delay(TimeSpan.FromSeconds(1));
			}

			_logger.LogInformation("Logs received. Creating ZIP archive for directory: {LogDirectory}", logDirectory);
			// Create a ZIP archive from the logs directory
			return CreateLogArchive(logDirectory);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logDirectory"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public async Task<MemoryStream> ProcessAllMachinesAndCreateArchiveAsync(string logDirectory)
		{
			_logger.LogInformation("Fetching all machine IDs from the repository for creating log request.");

			var machineIds = _machineRepository.GetAllMachineIds();

			if (machineIds == null || !machineIds.Any())
			{
				_logger.LogWarning("No machine IDs were found in the repository.");
				throw new InvalidOperationException("No machine IDs were found.");
			}

			_logger.LogInformation("Found {Count} machine IDs: {MachineIds}", machineIds.Length, string.Join(", ", machineIds));

			return await RequestLogsAndCreateArchiveAsync(logDirectory, machineIds);
		}

		private void ClearDirectoryContents(string directoryPath)
		{
			if (Directory.Exists(directoryPath))
			{
				_logger.LogInformation("Clearing contents of the directory: {DirectoryPath}", directoryPath);

				// Delete all files
				foreach (var file in Directory.GetFiles(directoryPath))
				{
					try
					{
						File.Delete(file);
						_logger.LogInformation("Deleted file: {FileName}", file);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Failed to delete file: {FileName}", file);
						throw;
					}
				}

				// Delete all subdirectories
				foreach (var subDirectory in Directory.GetDirectories(directoryPath))
				{
					try
					{
						Directory.Delete(subDirectory, true);
						_logger.LogInformation("Deleted subdirectory: {SubDirectory}", subDirectory);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Failed to delete subdirectory: {SubDirectory}", subDirectory);
						throw;
					}
				}
			}
			else
			{
				_logger.LogInformation("Directory does not exist. Creating directory: {DirectoryPath}", directoryPath);
				Directory.CreateDirectory(directoryPath);
			}
		}
	}
}