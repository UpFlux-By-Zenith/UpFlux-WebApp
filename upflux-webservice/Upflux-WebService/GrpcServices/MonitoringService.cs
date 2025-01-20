using Grpc.Core;
using Monitoring;
using Upflux_WebService.GrpcServices.Interfaces;
using Upflux_WebService.Services.Interfaces;
using static Monitoring.MonitoringService;
namespace Upflux_WebService.GrpcServices
{
	public class MonitoringService: MonitoringServiceBase, IMonitoringService
	{
		private readonly INotificationService _notificationService;

		public MonitoringService(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public override async Task<AggregatedDataResponse> SendAggregatedData(AggregatedDataRequest request, ServerCallContext context)
		{
			foreach (var aggregatedData in request.AggregatedDataList)
			{
				// Prepare the data to send
				var dataToSend = new
				{
					Uuid = aggregatedData.Uuid,
					Timestamp = aggregatedData.Timestamp.ToDateTime(),
					Metrics = new
					{
						CpuUsage = aggregatedData.Metrics.CpuUsage,
						MemoryUsage = aggregatedData.Metrics.MemoryUsage,
						DiskUsage = aggregatedData.Metrics.DiskUsage,
						NetworkUsage = new
						{
							BytesSent = aggregatedData.Metrics.NetworkUsage.BytesSent,
							BytesReceived = aggregatedData.Metrics.NetworkUsage.BytesReceived
						},
						CpuTemperature = aggregatedData.Metrics.CpuTemperature,
						SystemUptime = aggregatedData.Metrics.SystemUptime
					},
					SensorData = new
					{
						RedValue = aggregatedData.SensorData.RedValue,
						GreenValue = aggregatedData.SensorData.GreenValue,
						BlueValue = aggregatedData.SensorData.BlueValue
					}
				};

				// Use SendMessageToUriAsync to send data to groups
				await _notificationService.SendMessageToUriAsync(aggregatedData.Uuid, dataToSend.ToString());
			}

			return new AggregatedDataResponse
			{
				Success = true,
				Message = "Data received and processed successfully."
			};
		}
	}
}
