import { Action, configureStore, ThunkAction } from "@reduxjs/toolkit";
import metricsReducer from "./metricsSlice";
import alertReducer from "./alertSlice";
import connectionReducer from "./connectionSlice";
import applicationReducer from "./applicationVersions";
import machineReducer from "./machinesSlice";


const store = configureStore({
  reducer: {
    metrics: metricsReducer,
    alerts: alertReducer,
    machines: machineReducer,
    connectionStatus: connectionReducer,
    applications: applicationReducer
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
export type AppThunk<ReturnType = void> = ThunkAction<
  ReturnType,
  RootState,
  unknown,
  Action<string>
>;
export default store;
