namespace Upflux_WebService.Core.DTOs
{
    /// <summary>
    /// Represents the request data for admin login.
    /// </summary>
    public class AdminCreateRequest
    {
        /// <summary>
        /// Gets or sets the admin's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the admin's email address.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the admin's password.
        /// </summary>
        public string Password { get; set; }
    }
    /// <summary>
    /// Represents the request data for admin login.
    /// </summary>
    public class AdminCreateLoginRequest
    {
        /// <summary>
        /// Gets or sets the admin's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the admin's password.
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Request DTO for password change
    /// </summary>
    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// Represents the request data for generating a token for an engineer.
    /// </summary>
    public class EngineerTokenRequest
    {
        /// <summary>
        /// Gets or sets the email address of the engineer.
        /// </summary>
        public string EngineerEmail { get; set; }

        /// <summary>
        /// Gets or sets the name of the engineer.
        /// </summary>
        public string EngineerName { get; set; }

        /// <summary>
        /// Gets or sets the list of machine IDs that the engineer will have access to.
        /// </summary>
        public List<string> MachineIds { get; set; }
    }

    /// <summary>
    /// Represents the request data for engineer login, including the email and engineer token.
    /// </summary>
    public class EngineerLoginRequest
    {
        /// <summary>
        /// Gets or sets the email address of the engineer.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the token associated with the engineer, which is used for authentication.
        /// </summary>
        public string EngineerToken { get; set; }
    }

}
