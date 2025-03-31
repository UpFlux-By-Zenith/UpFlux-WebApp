import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface IAlertMessage {
  timestamp: string,
  level: string,
  message: string,
  source: string
}

interface AlertState {
  messages: IAlertMessage[];
}

// Initial state
const initialState: AlertState = {
  messages: [], // Start with an empty object
};

// Create the slice
const alertSlice = createSlice({
  name: "alerts",
  initialState,
  reducers: {
    addAlert: (state, action: PayloadAction<IAlertMessage>) => {
      return { messages: [...state.messages, action.payload] }
    },
    clearAlerts: (state) => {
      state.messages = []; // Reset the messages object
    },
  },
});

export const { addAlert, clearAlerts } = alertSlice.actions;
const alertReducer = alertSlice.reducer;
export default alertReducer;
