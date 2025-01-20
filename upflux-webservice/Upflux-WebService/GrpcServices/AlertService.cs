using Grpc.Core;
using Alert;
using static Alert.AlertService;
using Upflux_WebService.GrpcServices.Interfaces;
namespace Upflux_WebService.GrpcServices
{
	public class AlertService : AlertServiceBase, IAlertService
	{
		public override Task<AlertResponse> SendAlert(AlertRequest request, ServerCallContext context)
		{
			// replace with real logic
			Console.WriteLine($"Alert received:");
			Console.WriteLine($"Timestamp: {request.Timestamp.ToDateTime()}");
			Console.WriteLine($"Level: {request.Level}");
			Console.WriteLine($"Message: {request.Message}");
			Console.WriteLine($"Exception: {request.Exception}");
			Console.WriteLine($"Source: {request.Source}");

			return Task.FromResult(new AlertResponse
			{
				Success = true,
				Message = "Alert processed successfully"
			});
		}
	}
}
