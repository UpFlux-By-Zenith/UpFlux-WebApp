using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using UpFlux_GatewaySimulator.Protos;

public class AIRecommendationSender
{
	private readonly AsyncDuplexStreamingCall<ControlMessage, ControlMessage> _call;
	private readonly string _senderId;
	private bool _isRunning;
	private readonly List<string> _clusterIds = new List<string> { "ClusterA", "ClusterB" }; // Example clusters
	private readonly List<string> _deviceUuids; // Stores up to 3 user-entered UUIDs
	private readonly Random _random = new Random();

	public AIRecommendationSender(AsyncDuplexStreamingCall<ControlMessage, ControlMessage> call, string senderId, List<string> deviceUuids)
	{
		_call = call;
		_senderId = senderId;
		_deviceUuids = deviceUuids; // Store multiple UUIDs
	}

	public async Task SendAIRecommendationsAsync()
	{
		_isRunning = true;
		var requestStream = _call.RequestStream;

		Console.WriteLine($"Started sending AI recommendations for devices: {string.Join(", ", _deviceUuids)}");

		while (_isRunning)
		{
			var clusters = GenerateClusters();
			var plotPoints = GeneratePlotPoints(clusters);

			var message = new ControlMessage
			{
				SenderId = _senderId,
				Description = "AI Recommendation Data",
				AiRecommendations = new AIRecommendations
				{
					Clusters = { clusters },
					PlotData = { plotPoints }
				}
			};

			try
			{
				await requestStream.WriteAsync(message);
				Console.WriteLine($"Sent AI recommendation data with {clusters.Count} clusters and {plotPoints.Count} plot points.");
			}
			catch (RpcException ex)
			{
				Console.WriteLine($"gRPC Error: {ex.Status}");
				_isRunning = false;
			}

			await Task.Delay(2000); // Wait 2 seconds before sending again
		}

		await requestStream.CompleteAsync();
		Console.WriteLine("Stopped sending AI recommendations.");
	}

	public void StopSending()
	{
		_isRunning = false;
	}

	/// <summary>
	/// Generates clusters, ensuring devices are properly grouped.
	/// </summary>
	private List<AIScheduledCluster> GenerateClusters()
	{
		var clusters = new List<AIScheduledCluster>();
		var assignedClusters = new Dictionary<string, string>(); // Track each device's cluster

		foreach (var clusterId in _clusterIds)
		{
			var devicesInCluster = GetRandomSubset(_deviceUuids);
			foreach (var uuid in devicesInCluster)
			{
				assignedClusters[uuid] = clusterId; // Store the cluster assignment
			}

			clusters.Add(new AIScheduledCluster
			{
				ClusterId = clusterId,
				DeviceUuids = { devicesInCluster },
				UpdateTime = Timestamp.FromDateTime(DateTime.UtcNow) // Current timestamp
			});
		}

		return clusters;
	}

	/// <summary>
	/// Generates AI plot points ensuring devices match their assigned clusters.
	/// </summary>
	private List<AIPlotPoint> GeneratePlotPoints(List<AIScheduledCluster> clusters)
	{
		var plotPoints = new List<AIPlotPoint>();
		var deviceClusterMap = new Dictionary<string, string>();

		// Map each device to its assigned cluster
		foreach (var cluster in clusters)
		{
			foreach (var deviceUuid in cluster.DeviceUuids)
			{
				deviceClusterMap[deviceUuid] = cluster.ClusterId;
			}
		}

		// Generate AIPlotPoint data for each device, using its correct cluster
		foreach (var uuid in _deviceUuids)
		{
			if (deviceClusterMap.TryGetValue(uuid, out var clusterId))
			{
				plotPoints.Add(new AIPlotPoint
				{
					DeviceUuid = uuid,
					X = _random.NextDouble() * 100, // Random X coordinate
					Y = _random.NextDouble() * 100, // Random Y coordinate
					ClusterId = clusterId
				});
			}
		}

		return plotPoints;
	}

	/// <summary>
	/// Selects a random subset of UUIDs from the given list.
	/// </summary>
	private List<string> GetRandomSubset(List<string> uuids)
	{
		var subset = new List<string>();
		int subsetSize = _random.Next(1, uuids.Count + 1); // Choose at least one device per cluster

		for (int i = 0; i < subsetSize; i++)
		{
			string randomUuid = uuids[_random.Next(uuids.Count)];
			if (!subset.Contains(randomUuid))
				subset.Add(randomUuid);
		}

		return subset;
	}
}
