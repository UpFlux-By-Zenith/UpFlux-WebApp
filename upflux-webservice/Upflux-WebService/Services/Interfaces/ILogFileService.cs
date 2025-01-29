namespace Upflux_WebService.Services.Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface ILogFileService
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="logDirectory"></param>
		/// <returns></returns>
		MemoryStream CreateLogArchive(string logDirectory);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logDirectory"></param>
		/// <param name="deviceUuids"></param>
		/// <returns></returns>
		Task<MemoryStream> RequestLogsAndCreateArchiveAsync(string logDirectory, string[] deviceUuids);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logDirectory"></param>
		/// <returns></returns>
		Task<MemoryStream> ProcessAllMachinesAndCreateArchiveAsync(string logDirectory);
	}
}
