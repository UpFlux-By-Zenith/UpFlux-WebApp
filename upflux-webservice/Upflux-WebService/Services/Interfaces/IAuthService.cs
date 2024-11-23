namespace Upflux_WebService.Services.Interfaces
{
    public interface IAuthService
    {
        string AdminLogin(string email, string password);
        List<string> EngineerLogin(string email);
        string GenerateEngineerToken(string engineerEmail,List<string> machineIds, string? enginnerName);
        Dictionary<string, string> ParseToken(string token);
    }
}
