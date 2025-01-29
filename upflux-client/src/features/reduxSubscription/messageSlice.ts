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

interface MessagesState {
  messages: Record<string, MonitoringData>; // Use Record with uuid as key
}

// Initial state
const initialState: MessagesState = {
  messages: {}, // Start with an empty object
};

// Create the slice
const messagesSlice = createSlice({
  name: "messages",
  initialState,
  reducers: {
    addMessage: (state, action: PayloadAction<MonitoringData>) => {
      const { uuid } = action.payload;

      // Replace or add the message using uuid as the key
      state.messages[uuid] = action.payload;
    },
    clearMessages: (state) => {
      state.messages = {}; // Reset the messages object
    },
  },
});

export const { addMessage, clearMessages } = messagesSlice.actions;
const messageReducer = messagesSlice.reducer;
export default messageReducer;
