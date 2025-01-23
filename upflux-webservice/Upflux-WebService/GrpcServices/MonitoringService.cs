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
		private readonly ILogger<MonitoringService> _logger;

		public MonitoringService(INotificationService notificationService, ILogger<MonitoringService> logger)
		{
			_notificationService = notificationService;
			_logger = logger;
		}

		public override async Task<AggregatedDataResponse> SendAggregatedData(AggregatedDataRequest request, ServerCallContext context)
		{
			_logger.LogInformation("Received aggregated data request with {Count} data items.", request.AggregatedDataList.Count);

			foreach (var aggregatedData in request.AggregatedDataList)
			{
				_logger.LogInformation("Processing aggregated data for MachineId {Uuid} at {Timestamp}.",
					aggregatedData.Uuid, aggregatedData.Timestamp.ToDateTime());

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

				try
				{
					_logger.LogDebug("Sending data to notification service for MachineId {Uuid}.", aggregatedData.Uuid);
					await _notificationService.SendMessageToUriAsync(aggregatedData.Uuid, dataToSend.ToString());
					_logger.LogInformation("Successfully sent data for MachineId {Uuid}.", aggregatedData.Uuid);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to send data for MachineId {Uuid}.", aggregatedData.Uuid);
				}
			}

			_logger.LogInformation("All data items processed successfully.");

			return new AggregatedDataResponse
			{
				Success = true,
				Message = "Data received and processed successfully."
			};
		}
	}
}
