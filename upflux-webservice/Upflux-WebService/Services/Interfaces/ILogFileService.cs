namespace Upflux_WebService.Services.Interfaces
{
	public interface ILogFileService
	{
		MemoryStream CreateLogArchive(string logDirectory);
	}
}
