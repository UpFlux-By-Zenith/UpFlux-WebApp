using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Upflux_WebService.Controllers;

[ApiController]
[Route("[controller]")]
public class UpFluxController : ControllerBase
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
    [HttpGet("about", Name = "About")]
    public string About()
    {
        var appName = Assembly.GetExecutingAssembly().GetName().Name;
        var appVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

        return $"Application Name: {appName}, Version: {appVersion}";
    }
}