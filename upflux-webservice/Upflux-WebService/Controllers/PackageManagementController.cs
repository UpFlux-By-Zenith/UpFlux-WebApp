using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackageManagementController : ControllerBase
{
    //private readonly string _uploadPath = "/tmp/uploads"; // Base upload directory
    //private readonly string _uploadedPackagesPath = "/tmp/uploaded-
    //"; // Packages storage directory
    //private readonly string _signedFilesPath = "/tmp/signed"; // Path to save signed files

    // Path.GetTempPath()
    // on windows points to ==> C:\Users\<Username>\AppData\Local\Temp 
    // on Linux points to   ==> /tmp 
    private readonly string _uploadPath = Path.Combine(Path.GetTempPath(), "uploads");
    private readonly string _uploadedPackagesPath = Path.Combine(Path.GetTempPath(), "uploaded-packages");
    private readonly string _signedFilesPath = Path.Combine(Path.GetTempPath(), "signed");
    private readonly string _gatewayId;
    private readonly IControlChannelService _controlChannelService;
    private readonly ILogger<PackageManagementController> _logger;
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Constructor`
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="configuration"></param>
    /// <param name="controlChannelService"></param>
    public PackageManagementController(ILogger<PackageManagementController> logger, IConfiguration configuration,
        IControlChannelService controlChannelService, ApplicationDbContext context)
    {
        _context = context;
        _logger = logger;
        _gatewayId = configuration["GatewayId"]!;
        _controlChannelService = controlChannelService;

        // Ensure directories exist
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_uploadedPackagesPath);
        Directory.CreateDirectory(_signedFilesPath);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("sign")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> SignFile(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var adminEmail = GetClaimValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(adminEmail)) return Unauthorized("Admin email not found in token.");

        // Find UserID from Users table using admin email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (user == null) return NotFound("Admin user not found.");

        // Find AdminID from Admin_Details table using UserID
        var adminDetails = await _context.Admin_Details.FirstOrDefaultAsync(a => a.UserId == user.UserId);
        if (adminDetails == null) return Unauthorized("Admin ID not found.");

        var userId = adminDetails.UserId;


        var packageName = Path.GetFileNameWithoutExtension(file.FileName).Split('_')[0];
        var packageDirectory = Path.Combine(_uploadedPackagesPath, packageName);
        Directory.CreateDirectory(packageDirectory);

        var filePath = Path.Combine(packageDirectory, file.FileName);
        var signedFilePath = filePath + ".sig";

        try
        {
            _logger.LogInformation($"Saving file {file.FileName} to {filePath}");
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var gpgPassphrase = "UpFlux123";
            var gpgKeyId = "3ADAF9875CB265A5C41016D2B1902804CC7BF808";

            Console.WriteLine($"GPG_KEY_ID: {gpgKeyId}");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "gpg",
                Arguments =
                    $"--homedir \"/home/gursimar/.gnupg\" --batch --pinentry-mode=loopback --passphrase \"{gpgPassphrase}\" --yes --armor --output \"{signedFilePath}\" --detach-sign -u \"{gpgKeyId}\" \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (var process = Process.Start(processStartInfo))
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                _logger.LogInformation($"GPG Output: {output}");
                _logger.LogError($"GPG Error: {error}");
            }

            // Add version record to database
            var newVersion = new ApplicationVersion
            {
                UploadedBy = userId, // Store Admin ID
                VersionName = file.FileName.Split('_')[1],
                Date = DateTime.UtcNow
            };

            await _context.ApplicationVersions.AddAsync(newVersion);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"File signed successfully: {signedFilePath}");
            return Ok("File signed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Internal server error: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends available list of application and its versions to be deployed
    /// </summary>
    /// <returns></returns>
    [HttpGet("packages")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GetPackages()
    {
        try
        {
            if (!Directory.Exists(_uploadedPackagesPath))
                return Ok(new List<object>());

            // Log the base directory being checked
            _logger.LogInformation($"Checking directory: {_uploadedPackagesPath}");

            var packages = Directory.GetDirectories(_uploadedPackagesPath)
                .Select(pkgDir =>
                {
                    // Log each package directory being read
                    _logger.LogInformation($"Reading package directory: {pkgDir}");

                    var versionFiles = Directory.GetFiles(pkgDir)
                        .Where(file => !file.EndsWith(".sig")) // Exclude .sig files
                        .Select(file =>
                        {
                            // Log each file being read
                            _logger.LogInformation($"Reading file: {file}");

                            return Path.GetFileName(file); // Extract file name
                        })
                        .Select(fileName =>
                        {
                            // Extract version from file name (assumes format "package_name_version.extension")
                            var version = fileName.Split('_')[1];
                            _logger.LogInformation($"Extracted version: {version} from file: {fileName}");
                            return version;
                        })
                        .ToList();

                    return new
                    {
                        Name = Path.GetFileName(pkgDir),
                        Versions = versionFiles
                    };
                })
                .ToList();

            return Ok(packages);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving packages: {ex.Message}");
            return StatusCode(500, $"Error retrieving packages: {ex.Message}");
        }
    }


    /// <summary>
    /// Mocks the process of upload the package to a machine
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("packages/check")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult CheckPackageExists([FromBody] PackageCheckRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Version))
            return BadRequest("Package name and version are required.");

        var packageDirectory = Path.Combine(_uploadedPackagesPath, request.Name);
        if (!Directory.Exists(packageDirectory)) return NotFound("Package not found.");

        var packageFile = Directory.GetFiles(packageDirectory).FirstOrDefault(f => f.Contains(request.Version));
        if (packageFile == null) return NotFound("Package version not found.");

        return Ok("Package exists.");
    }

    /// <summary>
    /// upload package to gateway
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("packages/upload")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
    public async Task<IActionResult> UploadToGateway([FromBody] PackageUploadRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Version))
            return BadRequest("Package name and version are required.");

        var engineerEmail = GetClaimValue(ClaimTypes.Email);
        var machineIds = GetClaimValue("MachineIds");

        //Ensure claims exist
        if (string.IsNullOrWhiteSpace(engineerEmail) || string.IsNullOrWhiteSpace(machineIds))
            return BadRequest(new { Error = "Invalid claims: Engineer email or machine IDs are missing." });

        var packageDirectory = Path.Combine(_uploadedPackagesPath, request.Name);
        if (!Directory.Exists(packageDirectory)) return NotFound("Package not found.");

        // Look for a .deb file matching the package version
        var packageFile = Directory.GetFiles(packageDirectory)
            .FirstOrDefault(f => f.Contains(request.Version) && f.EndsWith(".deb"));

        // Look for a .sig file matching the package version
        var signatureFile = Directory.GetFiles(packageDirectory)
            .FirstOrDefault(f => f.Contains(request.Version) && f.EndsWith(".sig"));

        if (packageFile == null)
            return NotFound("Matching .deb package version not found.");

        if (signatureFile == null)
            return NotFound("Matching .sig version signature not found.");

        try
        {
            var packageData = await System.IO.File.ReadAllBytesAsync(packageFile);
            var signatureData = await System.IO.File.ReadAllBytesAsync(signatureFile);

            await _controlChannelService.SendUpdatePackageAsync(
                _gatewayId,
                Path.GetFileName(packageFile),
                packageData,
                signatureData,
                request.TargetDevices,
                request.Name,
                request.Version,
                engineerEmail
            );


            return Ok($"Package [{request.Name}] version [{request.Version}] uploaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending package: {ex.Message}");
            return StatusCode(500, $"Error sending package: {ex.Message}");
        }
    }

    [HttpPost("packages/schedule-update")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
    public async Task<IActionResult> UploadScheduledPackageToGateway([FromBody] ScheduledUpdateRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Version))
            return BadRequest("Package name and version are required.");

        var engineerEmail = GetClaimValue(ClaimTypes.Email);
        var machineIds = GetClaimValue("MachineIds");

        //Ensure claims exist
        if (string.IsNullOrWhiteSpace(engineerEmail) || string.IsNullOrWhiteSpace(machineIds))
            return BadRequest(new { Error = "Invalid claims: Engineer email or machine IDs are missing." });

        var packageDirectory = Path.Combine(_uploadedPackagesPath, request.Name);
        if (!Directory.Exists(packageDirectory)) return NotFound("Package not found.");

        // Look for a .deb file matching the package version
        var packageFile = Directory.GetFiles(packageDirectory)
            .FirstOrDefault(f => f.Contains(request.Version) && f.EndsWith(".deb"));

        // Look for a .sig file matching the package version
        var signatureFile = Directory.GetFiles(packageDirectory)
            .FirstOrDefault(f => f.Contains(request.Version) && f.EndsWith(".sig"));

        if (packageFile == null)
            return NotFound("Matching .deb package version not found.");

        if (signatureFile == null)
            return NotFound("Matching .sig version signature not found.");

        try
        {
            var packageData = await System.IO.File.ReadAllBytesAsync(packageFile);
            var signatureData = await System.IO.File.ReadAllBytesAsync(signatureFile);
            var scheduleId = Guid.NewGuid().ToString("N");

            await _controlChannelService.SendScheduledUpdateAsync(
                _gatewayId,
                scheduleId,
                request.TargetDevices,
                Path.GetFileName(packageFile),
                packageData, signatureData,
                request.startTimeUtc,
                engineerEmail
            );

            return Ok($"Package [{request.Name}] version [{request.Version}] uploaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending package: {ex.Message}");
            return StatusCode(500, $"Error sending package: {ex.Message}");
        }
    }

    public class PackageCheckRequest
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }

    public class PackageUploadRequest
    {
        public string Name { get; set; } // Package name
        public string Version { get; set; } // Package version
        public string[] TargetDevices { get; set; } // Target devices
    }

    public class ScheduledUpdateRequest
    {
        public string Name { get; set; } // Package name
        public string Version { get; set; } // Package version
        public string[] TargetDevices { get; set; } // Target devices
        public DateTime startTimeUtc { get; set; }
    }

    #region helper methods

    private string? GetClaimValue(string claimType)
    {
        return User.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    #endregion
}