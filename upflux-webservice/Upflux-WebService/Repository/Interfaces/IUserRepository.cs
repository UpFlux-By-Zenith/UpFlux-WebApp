namespace Upflux_WebService.Repository.Interfaces
{
	public interface IUserRepository : IRepository<User>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		Task<User?> GetUserByEmail(string email);
	}
}
