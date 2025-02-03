import { configureStore } from "@reduxjs/toolkit";
import metricsReducer from "./metricsSlice";
import alertReducer from "./alertSlice";
import connectionReducer from "./connectionSlice";
import applicationReducer from "./applicationVersions";


const store = configureStore({
  reducer: {
    metrics: metricsReducer,
    alerts: alertReducer,
    connectionStatus: connectionReducer,
    applications: applicationReducer
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export default store;
