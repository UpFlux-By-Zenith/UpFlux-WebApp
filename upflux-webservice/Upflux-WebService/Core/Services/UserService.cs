using MySqlConnector;
using Upflux_WebService.Core.Objects;

namespace Upflux_WebService.Core.Services
{
    public class UserService
    {
        private readonly MySqlConnection _dbConnection;

        public UserService(MySqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Method to get an engineer by email
        public User GetEngineerByEmail(string email)
        {
           throw new NotImplementedException();
        }

        // Method to get a user by their email (could be used for Admin, Engineer, etc.)
        public User GetUserByEmail(string email)
        {
            throw new NotImplementedException();
        }

        // Method to update a user's machine access (for an engineer)
        public void UpdateMachineAccess(string email, List<int> machineIds)
        {
            throw new NotImplementedException();
        }

        // Additional methods to handle user-related operations (e.g., creating, updating, etc.)
    }
}

