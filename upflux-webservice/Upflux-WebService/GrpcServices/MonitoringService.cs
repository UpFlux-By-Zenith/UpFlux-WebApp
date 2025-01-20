using Grpc.Core;
using Monitoring;
using Upflux_WebService.GrpcServices.Interfaces;
using static Monitoring.MonitoringService;
namespace Upflux_WebService.GrpcServices
{
	public class MonitoringService: MonitoringServiceBase, IMonitoringService
	{

		public override async Task<AggregatedDataResponse> SendAggregatedData(AggregatedDataRequest request, ServerCallContext context)
		{
			foreach (var aggregatedData in request.AggregatedDataList)
			{
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

				// Use SignalR to send data to connected clients
				// signalFunction.Send();
			}

			// Return a success response to the gRPC client
			return new AggregatedDataResponse
			{
				Success = true,
				Message = "Data received and processed successfully."
			};
		}
	}
}
