using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Upflux_WebService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class PackageSignController : ControllerBase
{
    private readonly ILogger<PackageSignController> _logger;
    private readonly string _uploadPath = "/tmp/uploads"; // Adjust path as needed

    private readonly string _certPath =
        "/home/gursimar/RiderProjects/UpFlux-WebApp/upflux-webservice/Upflux-WebService/Cert/cert.pem"; // Path to custom cert

    private readonly string _keyPath =
        "/home/gursimar/RiderProjects/UpFlux-WebApp/upflux-webservice/Upflux-WebService/Cert/privatekey.pem"; // Path to private key

    private readonly string _signedFilesPath = "/tmp/signed"; // Path to save signed files

    public PackageSignController(ILogger<PackageSignController> logger)
    {
        _logger = logger;
        // Ensure directories exist
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_signedFilesPath);
    }

    [HttpPost("sign")]
    public async Task<IActionResult> SignFile(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var filePath = Path.Combine(_uploadPath, file.FileName);
        var signedFilePath = Path.Combine(_signedFilesPath, $"signed-{file.FileName}.sig");

        try
        {
            // Save the uploaded file
            _logger.LogInformation($"Saving file {file.FileName} to {filePath}");
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Log the signing process
            _logger.LogInformation($"Signing file {file.FileName} using GPG with key from the keyring");

            // Prepare the GPG signing process
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "gpg",
                Arguments =
                    $"--batch --yes --armor --output \"{signedFilePath}\" --detach-sign --default-key \"49FA55F02ADF3C914E0ABBC4E5C81D52A866B46B\" \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            var process = new Process { StartInfo = processStartInfo };
            process.Start();

            // Read output and error streams
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            // Log the output and error
            _logger.LogInformation($"gpg output: {output}");
            if (!string.IsNullOrEmpty(error)) _logger.LogError($"gpg error: {error}");

            // Check for errors in the process execution
            if (process.ExitCode != 0)
            {
                _logger.LogError($"File signing failed with exit code {process.ExitCode}");
                return StatusCode(500, $"Error signing file: {error}");
            }

            // Log success
            _logger.LogInformation($"File signed successfully. Returning signed file: {signedFilePath}");

            // Return the signed file
            var signedFileBytes = await System.IO.File.ReadAllBytesAsync(signedFilePath);
            return File(signedFileBytes, "application/octet-stream", Path.GetFileName(signedFilePath));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Internal server error: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
        finally
        {
            // Clean up
            if (System.IO.File.Exists(filePath))
            {
                _logger.LogInformation($"Deleting uploaded file {filePath}");
                System.IO.File.Delete(filePath);
            }

            if (System.IO.File.Exists(signedFilePath))
            {
                _logger.LogInformation($"Deleting signed file {signedFilePath}");
                System.IO.File.Delete(signedFilePath);
            }
        }
    }
}