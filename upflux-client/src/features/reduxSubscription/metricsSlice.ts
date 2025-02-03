import { createSlice, PayloadAction } from "@reduxjs/toolkit";

// Define the structure of the monitoring data
interface NetworkUsage {
  bytesSent: string;
  bytesReceived: string;
}

interface Metrics {
  cpuUsage: number;
  memoryUsage: number;
  diskUsage: number;
  networkUsage: NetworkUsage;
  cpuTemperature: number;
  systemUptime: number;
}

interface SensorData {
  redValue: number;
  greenValue: number;
  blueValue: number;
}

export interface MonitoringData {
  uuid: string;
  timestamp: string;
  metrics: Metrics;
  sensorData: SensorData;
}

interface MetricsState {
  metrics: Record<string, MonitoringData>; // Use Record with uuid as key
}

// Initial state
const initialState: MetricsState = {
  metrics: {}, // Start with an empty object
};

// Create the slice
const metricsSlice = createSlice({
  name: "metrics",
  initialState,
  reducers: {
    addMetrics: (state, action: PayloadAction<MonitoringData>) => {
      const { uuid } = action.payload;

      // Replace or add the message using uuid as the key
      state.metrics[uuid] = action.payload;
    },
    clearMetrics: (state) => {
      state.metrics = {}; // Reset the messages object
    },
  },
});

export const { addMetrics, clearMetrics } = metricsSlice.actions;
const metricsReducer = metricsSlice.reducer;
export default metricsReducer;
