using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Controllers
{
	/// <summary>
	/// Log files related controller
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
	public class LogFileController : ControllerBase
	{
		private readonly ILogFileService _logFileService;
		private readonly string _logDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logFileService"></param>
		public LogFileController(ILogFileService logFileService, IConfiguration configuration)
		{
			_logFileService = logFileService;
			_logDirectoryPath = configuration["Logging:LogsDirectory"];
		}

		/// <summary>
		/// Downloads all log files as a ZIP archive.
		/// </summary>
		/// <returns>A ZIP file containing all log files.</returns>
		[HttpGet("admin/download-all")]
		public IActionResult DownloadAllLogs()
		{
			try
			{
				var archiveStream = _logFileService.CreateLogArchive(_logDirectoryPath);
				return File(archiveStream, "application/zip", "logs.zip");
			}
			catch (DirectoryNotFoundException ex)
			{
				return NotFound(ex.Message);
			}
			catch (FileNotFoundException ex)
			{
				return NotFound(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"An error occurred while creating the log archive: {ex.Message}");
			}
		}
	}
}
