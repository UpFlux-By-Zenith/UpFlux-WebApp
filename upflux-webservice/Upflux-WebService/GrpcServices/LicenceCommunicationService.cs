using Grpc.Core;
using GrpcServer;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Upflux_WebService.GrpcServices.Interfaces;
using Upflux_WebService.Services;
using Upflux_WebService.Services.Interfaces;
using static GrpcServer.LicenceCommunication;

namespace Upflux_WebService.GrpcServices
{
	/// <summary>
	/// gRPC service to send licence update to gateway
	/// </summary>
	public class LicenceCommunicationService : LicenceCommunicationBase, ILicenceCommunicationService
	{
		#region private members

		private readonly ConcurrentDictionary<string, IServerStreamWriter<LicenceFileUpdate>> _clients = new();
		private readonly ILogger<LicenceCommunicationService> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		#endregion
		#region public methods

		public LicenceCommunicationService(ILogger<LicenceCommunicationService> logger, IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger;
			_serviceScopeFactory = serviceScopeFactory;
		}

		/// <summary>
		/// Handles client subscription to licence updates
		/// </summary>
		/// <param name="request">empty request that triggers subscription process</param>
		/// <param name="responseStream">stream that is used by the server to send real time data to the client</param>
		/// <param name="context">gRPC server data</param>
		/// <returns>task that keeps connection alive until client disconnects</returns>
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

		/// <summary>
		/// Send licence update to the clients
		/// </summary>
		/// <param name="licenceFileUpdate">the licence metadata which will be used as load</param>
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

		public override async Task<LicenceResponse> ValidateLicence(LicenceRequest request, ServerCallContext context)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var licenceManagementService = scope.ServiceProvider.GetRequiredService<ILicenceManagementService>();

			bool valid = await licenceManagementService.ValidateLicence(request.LicenseXml);
			LicenceResponse response = new();
			foreach (var client in _clients.Values)
			{
				Console.WriteLine();
			}
			if (valid)
			{
				response.IsValid = true;
			}
			return response;
		}

		#endregion
	}
}
