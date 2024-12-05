using Grpc.Core;
using GrpcServer;
using System.Collections.Concurrent;
using Upflux_WebService.GrpcServices.Interfaces;
using static GrpcServer.LicenceCommunication;

namespace Upflux_WebService.GrpcServices
{
	public class LicenceCommunicationService : LicenceCommunicationBase, ILicenceCommunicationService
	{
		private readonly ConcurrentDictionary<string, IServerStreamWriter<LicenceFileUpdate>> _clients = new();
		private readonly ILogger<LicenceCommunicationService> _logger;

		public LicenceCommunicationService(ILogger<LicenceCommunicationService> logger)
		{
			_logger = logger;
		}

		public override async Task SubscribeToLicenceUpdates(EmptyRequest request, IServerStreamWriter<LicenceFileUpdate> responseStream, ServerCallContext context)
		{
			// Generate a unique ID for the client
			var clientId = Guid.NewGuid().ToString();
			_clients.TryAdd(clientId, responseStream);

			_logger.LogInformation($"Client {clientId} subscribed to licence updates.");

			// Remove client on disconnection
			context.CancellationToken.Register(() =>
			{
				_clients.TryRemove(clientId, out _);
				_logger.LogInformation($"Client {clientId} disconnected.");
			});

			// Keep the connection alive
			try
			{
				await Task.Delay(Timeout.Infinite, context.CancellationToken);
			}
			catch (TaskCanceledException)
			{
				_logger.LogWarning($"Streaming for client {clientId} was cancelled.");
			}
		}
		public async Task PushLicenceUpdateAsync(LicenceFileUpdate licenceFileUpdate)
		{
			foreach (var client in _clients.Values)
			{
				try
				{
					await client.WriteAsync(licenceFileUpdate);
					_logger.LogInformation($"Successfully sent update to client {client}.");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to send update to client {client}. Marking client for removal.");
				}
			}
		}
	}
}
