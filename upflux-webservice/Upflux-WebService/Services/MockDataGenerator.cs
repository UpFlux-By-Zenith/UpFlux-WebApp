namespace Upflux_WebService.Services
{
    public class AggregatedData
    {
        public string Uuid { get; set; }
        public DateTime Timestamp { get; set; }
        public Metrics Metrics { get; set; }
        public SensorData SensorData { get; set; }
    }

    public class Metrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public NetworkUsage NetworkUsage { get; set; }
        public double CpuTemperature { get; set; }
        public double SystemUptime { get; set; }
    }

    public class NetworkUsage
    {
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
    }

    public class SensorData
    {
        public int redValue { get; set; }
        public int greenValue { get; set; }
        public int blueValue { get; set; }
    }
    public class MockDataGenerator
    {
        private static readonly Random Random = new Random();

        public AggregatedData GenerateMockData(string uuid)
        {
            return new AggregatedData
            {
                Uuid = uuid,
                Timestamp = DateTime.UtcNow,
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
                    redValue = Random.Next() * 255,
                    blueValue = Random.Next() * 255,
                    greenValue = Random.Next() * 255,
                }
            };
        }

        public void UpdateMockData(AggregatedData data)
        {
            data.Timestamp = DateTime.UtcNow;
            data.Metrics.CpuUsage = Random.NextDouble() * 100;
            data.Metrics.MemoryUsage = Random.NextDouble() * 100;
            data.Metrics.DiskUsage = Random.NextDouble() * 100;
            data.Metrics.NetworkUsage.BytesSent += Random.Next(100, 1000);
            data.Metrics.NetworkUsage.BytesReceived += Random.Next(100, 1000);
            data.Metrics.CpuTemperature = Random.NextDouble() * 100;
            data.Metrics.SystemUptime += 1; // Increment uptime
        }
    }

}
