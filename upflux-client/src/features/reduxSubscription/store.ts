import { configureStore } from "@reduxjs/toolkit";
import metricsReducer from "./metricsSlice";
import alertReducer from "./alertSlice";


const store = configureStore({
  reducer: {
    metrics: metricsReducer,
    alerts:alertReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export default store;
