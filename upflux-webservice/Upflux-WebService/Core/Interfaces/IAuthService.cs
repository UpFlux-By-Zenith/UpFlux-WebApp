using Upflux_WebService.Controllers;

public interface IAuthService
{
    Task<string> AdminLoginAsync(AdminLoginRequest loginRequest);
    Task<bool> AdminResetPasswordAsync(AdminResetPasswordRequest resetRequest);
    Task<string> EngineerLoginAsync(AdminLoginRequest loginRequest);
}