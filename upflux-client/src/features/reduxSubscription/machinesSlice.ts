import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { IMachine } from "../../api/reponseTypes";
import { IClusterResponse, IMachineStatus } from "./subscriptionConsts";

interface AlertState {
    messages: Record<string, IMachine>;
}

// Initial state
const initialState: AlertState = {
    messages: {}, // Start with an empty object
};

// Create the slice
const machineSlice = createSlice({
    name: "machines",
    initialState,
    reducers: {
        updateMachine: (state, action: PayloadAction<IMachine>) => {
            const { machineId } = action.payload;
            state.messages[machineId] = {
                ...action.payload,

                dateAddedOn: new Date(action.payload.dateAddedOn).toUTCString()
            }; // Store using UUID as key
        },
        updateMachineStatus: (state, action: PayloadAction<IMachineStatus>) => {
            const { LastSeen, IsOnline, DeviceUuid } = action.payload;
            if (state.messages[DeviceUuid]) {
                state.messages[DeviceUuid] = {
                    ...state.messages[DeviceUuid],
                    isOnline: IsOnline,
                    lastSeen: LastSeen as any
                };
            }
        },

        updatePlotValues: (state, action: PayloadAction<IClusterResponse>) => {
            const { DeviceUuid, X, Y, ClusterId } = action.payload;
            if (state.messages[DeviceUuid]) {
                state.messages[DeviceUuid] = {
                    ...state.messages[DeviceUuid],
                    x: X,
                    y: Y,
                    clusterId: ClusterId as any === "0" ? "Cluster A" : "Cluster B"
                };
            }
        }
    },
});

export const { updateMachine, updateMachineStatus, updatePlotValues } = machineSlice.actions;
const machineReducer = machineSlice.reducer;
export default machineReducer;
