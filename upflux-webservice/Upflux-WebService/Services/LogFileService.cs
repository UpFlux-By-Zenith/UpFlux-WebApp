using System.Globalization;
using System.IO;
using System.IO.Compression;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
	/// <summary>
	/// Service related to log file
	/// </summary>
	public class LogFileService : ILogFileService
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="logDirectory"></param>
		/// <returns></returns>
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
	}
}