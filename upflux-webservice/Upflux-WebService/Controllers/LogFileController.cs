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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
    public class LogFileController : ControllerBase
    {
        private readonly ILogFileService _logFileService;
        private readonly string _logDirectoryPath;
        private readonly string _machineLogDirectoryPath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logFileService"></param>
        /// <param name="configuration"></param>
        public LogFileController(ILogFileService logFileService, IConfiguration configuration)
        {
            _logFileService = logFileService;
            _logDirectoryPath = configuration["Logging:LogsDirectory"]!;
            _machineLogDirectoryPath = configuration["Logging:MachineLogsDirectory"]!;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("admin/machine/download")]
        public async Task<IActionResult> DownloadMachineLogs([FromBody] DeviceUuidRequest request)
        {
            try
            {
                var archiveStream =
                    await _logFileService.RequestLogsAndCreateArchiveAsync(_machineLogDirectoryPath,
                        request.DeviceUuids);

                return File(archiveStream, "application/zip", "machine-logs.zip");
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
                return StatusCode(500,
                    new { message = "An error occurred while processing the logs.", error = ex.Message });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("admin/machine/download-all")]
        public async Task<IActionResult> ProcessAllMachines()
        {
            try
            {
                var archiveStream =
                    await _logFileService.ProcessAllMachinesAndCreateArchiveAsync(_machineLogDirectoryPath);

                return File(archiveStream, "application/zip", "machine-logs.zip");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing logs.");
            }
        }
    }
}

public class DeviceUuidRequest
{
    public string[] DeviceUuids { get; set; }
}