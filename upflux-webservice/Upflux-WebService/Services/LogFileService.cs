using System.IO.Compression;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
	/// <summary>
	/// Service related to log file
	/// </summary>
	public class LogFileService : ILogFileService
	{

		private readonly IControlChannelService _controlChannelService;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="controlChannelService"></param>
		public LogFileService(IControlChannelService controlChannelService)
		{
			_controlChannelService = controlChannelService;
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
			if (!Directory.Exists(logDirectory))
			{
				throw new DirectoryNotFoundException("Log directory does not exist.");
			}

			var logFiles = Directory.GetFiles(logDirectory, "log-*.txt");

			if (!logFiles.Any())
			{
				throw new FileNotFoundException("No log files found in the directory.");
			}

			var memoryStream = new MemoryStream();

			using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
			{
				foreach (var logFile in logFiles)
				{
					var entry = archive.CreateEntry(Path.GetFileName(logFile));

					// Use FileShare.ReadWrite to handle files that are in use
					using var entryStream = entry.Open();
					using var logFileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					logFileStream.CopyTo(entryStream);
				}
			}

			memoryStream.Position = 0; // Reset stream position for reading
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
			if (Directory.Exists(logDirectory))
			{
				Directory.Delete(logDirectory, true);
			}
			Directory.CreateDirectory(logDirectory);

			// Call the ControlChannelService to request logs
			await _controlChannelService.SendLogRequestAsync("gateway-patrick-1234", deviceUuids);

			// Wait until files are received or timeout after a maximum period
			int maxRetries = 10; // 10 retries with a 1-second delay = 10 seconds max wait
			int retryCount = 0;
			while (!Directory.EnumerateFiles(logDirectory).Any())
			{
				if (retryCount++ >= maxRetries)
				{
					throw new TimeoutException("No logs were received within the expected time frame.");
				}

				await Task.Delay(TimeSpan.FromSeconds(1));
			}

			// Create a ZIP archive from the logs directory
			return CreateLogArchive(logDirectory);
		}
	}
}