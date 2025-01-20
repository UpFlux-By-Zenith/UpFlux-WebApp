using Grpc.Core;
using LicenceCommunication;
using System.Collections.Concurrent;
using Upflux_WebService.Core.Models;
using Upflux_WebService.GrpcServices.Interfaces;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Services.Interfaces;
using static LicenceCommunication.LicenceCommunicationService;

namespace Upflux_WebService.GrpcServices
{
	/// <summary>
	/// gRPC service to send licence update to gateway
	/// </summary>
	public class LicenceCommunicationService : LicenceCommunicationServiceBase, ILicenceCommunicationService
	{
		#region private members

		private readonly ConcurrentDictionary<string, IServerStreamWriter<LicenceUpdateEvent>> _clients = new();
		private readonly ILogger<LicenceCommunicationService> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;


		#endregion
		#region public methods

		public LicenceCommunicationService(
			ILogger<LicenceCommunicationService> logger,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger;
			_serviceScopeFactory = serviceScopeFactory;

		}

		/// <summary>
		/// Handles client subscription to license updates by establishing a streaming connection.
		/// </summary>
		/// <param name="request">An empty request that triggers the subscription process.</param>
		/// <param name="responseStream">A server stream used to send real-time updates to the client.</param>
		/// <param name="context">Context information for the gRPC server call, including cancellation tokens.</param>
		/// <returns>
		/// A <see cref="Task"/> that keeps the connection alive until the client disconnects.
		/// </returns>
		/// <remarks>
		/// - Each client is assigned a unique identifier upon subscription.
		/// - The connection remains open indefinitely unless explicitly cancelled by the client.
		/// - On disconnection, the client is removed from the subscription list.
		/// </remarks>
		public override async Task SubscribeToLicenceUpdates(EmptyRequest request, IServerStreamWriter<LicenceUpdateEvent> responseStream, ServerCallContext context)
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
		/// Sends a license update to all subscribed clients.
		/// </summary>
		/// <param name="licenceFileUpdate">The license metadata to be sent as the payload.</param>
		/// <remarks>
		/// - This method iterates through all active client subscriptions and attempts to send the update.
		/// - If sending fails for a client, an error is logged, and the client may be marked for removal.
		/// </remarks>
		public async Task PushLicenceUpdateAsync(LicenceUpdateEvent licenceFileUpdate)
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

		/// <summary>
		/// Validates a license using the provided license content and returns the validation result.
		/// </summary>
		/// <param name="request">
		/// The <see cref="LicenceValidationRequest"/> containing the license content to be validated.
		/// </param>
		/// <param name="context">
		/// The gRPC server call context, which includes metadata and a cancellation token for managing the request lifecycle.
		/// </param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> representing the asynchronous operation. The result is a 
		/// <see cref="LicenceValidationResponse"/> indicating whether the license is valid and any associated details.
		/// </returns>
		/// <remarks>
		/// This method:
		/// - Creates a scoped instance of the <see cref="ILicenceManagementService"/> to handle the license validation process.
		/// - Asynchronously validates the license content provided in the request.
		/// </remarks>
		public override async Task<LicenceValidationResponse> ValidateLicence(LicenceValidationRequest request, ServerCallContext context)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var licenceManagementService = scope.ServiceProvider.GetRequiredService<ILicenceManagementService>();

			var response = await licenceManagementService.ValidateLicence(request.LicenseContent);

			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public override async Task<AddUnregisteredDeviceResponse> AddUnregisteredDevice(AddUnregisteredDeviceRequest request, ServerCallContext context)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
			var generatedMachineIdRepository = scope.ServiceProvider.GetRequiredService<IGeneratedMachineIdRepository>();

			var generatedId = await generatedMachineIdRepository.GetByMachineId(request.DeviceUuid);
			if (generatedId is null)
				return new AddUnregisteredDeviceResponse
				{
					IsSuccesful = false,
					Message = "Device UUID Unrecognized."
				};

			Machine newMachine = new() 
			{ 
				MachineId = request.DeviceUuid, 
				dateAddedOn = DateTime.UtcNow,
				ipAddress = "NA" 
			};
			await machineRepository.AddAsync(newMachine);
			await machineRepository.SaveChangesAsync();

			return new AddUnregisteredDeviceResponse
			{
				IsSuccesful = true,
				Message = "Saved Unregistered Device."
			};
		}

		#endregion
	}
}
