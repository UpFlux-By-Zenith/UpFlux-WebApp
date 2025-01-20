using Grpc.Core;
using CloudLogs;
using static CloudLogs.CloudLogService;
using Upflux_WebService.GrpcServices.Interfaces;


namespace Upflux_WebService.GrpcServices
{
	public class CloudLogService : CloudLogServiceBase, ICloudLogService
	{
		public override async Task<LogUploadResponse> UploadDeviceLogs(IAsyncStreamReader<LogUploadRequest> requestStream, ServerCallContext context)
		{
			string deviceUuid = string.Empty;
			string fileName = string.Empty;
			string tempFilePath = Path.GetTempFileName();

			try
			{
				using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
				{
					while (await requestStream.MoveNext())
					{
						var request = requestStream.Current;

						if (request.ContentCase == LogUploadRequest.ContentOneofCase.Metadata)
						{
							deviceUuid = request.Metadata.DeviceUuid;
							fileName = request.Metadata.FileName;
							Console.WriteLine($"Received metadata: DeviceUuid={deviceUuid}, FileName={fileName}");
						}

						else if (request.ContentCase == LogUploadRequest.ContentOneofCase.Data)
						{
							var data = request.Data.ToByteArray();
							await fileStream.WriteAsync(data, 0, data.Length);
						}
					}
				}

				var finalPath = Path.Combine("Logs", $"{deviceUuid}_{fileName}");
				Directory.CreateDirectory("Logs");
				File.Move(tempFilePath, finalPath);

				Console.WriteLine($"Log file saved to: {finalPath}");

				return new LogUploadResponse
				{
					Success = true,
					Message = $"Logs uploaded successfully for device {deviceUuid}"
				};
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error processing logs: {ex.Message}");
				return new LogUploadResponse
				{
					Success = false,
					Message = "Failed to process log upload."
				};
			}
			finally
			{
				if (File.Exists(tempFilePath))
				{
					File.Delete(tempFilePath);
				}
			}
		}
	}
}
