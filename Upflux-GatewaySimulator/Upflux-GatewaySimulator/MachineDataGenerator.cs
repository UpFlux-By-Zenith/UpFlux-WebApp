using Google.Protobuf.WellKnownTypes;
using UpFlux_GatewaySimulator.Protos;

namespace Upflux_GatewaySimulator
{
    internal class MachineDataGenerator
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// The unique identifier associated with this MockDataGenerator instance.
        /// </summary>
        public string Uuid { get; }

        /// <summary>
        /// Constructor to initialize the MockDataGenerator with a specific UUID.
        /// </summary>
        /// <param name="uuid">The unique identifier for this generator.</param>
        public MachineDataGenerator(string uuid)
        {
            Uuid = uuid ?? throw new ArgumentNullException(nameof(uuid), "UUID cannot be null.");
        }

        /// <summary>
        /// Generates a MonitoringDataMessage with mock data for this instance's UUID.
        /// </summary>
        /// <returns>A MonitoringDataMessage with randomly generated metrics and sensor data.</returns>
        public MonitoringDataMessage GenerateMockData()
        {
            var monitoringDataMessage = new MonitoringDataMessage();

            monitoringDataMessage.AggregatedData.Add(new AggregatedData
            {
                Uuid = Uuid,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                Metrics = new Metrics
                {
                    CpuUsage = Random.NextDouble() * 100, // Random CPU usage (0-100%)
                    MemoryUsage = Random.NextDouble() * 100, // Random Memory usage (0-100%)
                    DiskUsage = Random.NextDouble() * 100, // Random Disk usage (0-100%)
                    NetworkUsage = new NetworkUsage
                    {
                        BytesSent = Random.Next(1000, 100000), // Random bytes sent
                        BytesReceived = Random.Next(1000, 100000) // Random bytes received
                    },
                    CpuTemperature = Random.NextDouble() * 100, // Random CPU temperature
                    SystemUptime = Random.NextDouble() * 10000 // Random system uptime
                },
                SensorData = new SensorData
                {
                    RedValue = Random.Next(0, 256), // Random Red value (0-255)
                    GreenValue = Random.Next(0, 256), // Random Green value (0-255)
                    BlueValue = Random.Next(0, 256) // Random Blue value (0-255)
                }
            });

            return monitoringDataMessage;
        }

        /// <summary>
        /// Generates a new AggregatedData object with mock data for this instance's UUID.
        /// </summary>
        /// <returns>An AggregatedData object with randomly generated metrics and sensor data.</returns>
        public AggregatedData GenerateMockAggregatedData()
        {
            return new AggregatedData
            {
                Uuid = Uuid,
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                Metrics = new Metrics
                {
                    CpuUsage = Random.NextDouble() * 100,
                    MemoryUsage = Random.NextDouble() * 100,
                    DiskUsage = Random.NextDouble() * 100,
                    NetworkUsage = new NetworkUsage
                    {
                        BytesSent = Random.Next(1000, 100000),
                        BytesReceived = Random.Next(1000, 100000)
                    },
                    CpuTemperature = Random.NextDouble() * 100,
                    SystemUptime = Random.NextDouble() * 10000
                },
                SensorData = new SensorData
                {
                    RedValue = Random.Next(0, 256),
                    GreenValue = Random.Next(0, 256),
                    BlueValue = Random.Next(0, 256)
                }
            };
        }

        /// <summary>
        /// Updates an existing AggregatedData object with new random metrics and sensor data.
        /// </summary>
        /// <param name="data">The AggregatedData object to update.</param>
        public void UpdateMockData(AggregatedData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data), "Data cannot be null.");

            data.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            data.Metrics.CpuUsage = Random.NextDouble() * 100;
            data.Metrics.MemoryUsage = Random.NextDouble() * 100;
            data.Metrics.DiskUsage = Random.NextDouble() * 100;
            data.Metrics.NetworkUsage.BytesSent += Random.Next(100, 1000); // Increment bytes sent
            data.Metrics.NetworkUsage.BytesReceived += Random.Next(100, 1000); // Increment bytes received
            data.Metrics.CpuTemperature = Random.NextDouble() * 100;
            data.Metrics.SystemUptime += 1; // Increment uptime

            data.SensorData.RedValue = Random.Next(0, 256);
            data.SensorData.GreenValue = Random.Next(0, 256);
            data.SensorData.BlueValue = Random.Next(0, 256);
        }
    }
}
