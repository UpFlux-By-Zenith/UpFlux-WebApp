namespace Upflux_WebService.Services.Enums
{
	/// <summary>
	/// not to be confused with proto file's CommandType, this enum is to help with command response handling
	/// </summary>
	public enum ControlChannelCommandType
	{
		Rollback = 0,
		ScheduledUpdate = 1
	}
}
