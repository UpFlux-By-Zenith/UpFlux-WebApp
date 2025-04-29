import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { IMachine } from "../../api/reponseTypes";
import { IClusterResponse, IMachineStatus } from "./subscriptionConsts";

interface PlotRecommendation {
    x?: number;
    y?: number;
    clusterId?: string;
}

interface AlertState {
    messages: Record<string, IMachine>;
    syntheticMachines: Record<string, PlotRecommendation>
}

// Initial state
const initialState: AlertState = {
    messages: {}, // Start with an empty object
    syntheticMachines: {},

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
        updateMachineVersion: (state, action: PayloadAction<IMachine>) => {
            const { machineId } = action.payload;
            state.messages[machineId] = {
                ...state.messages[machineId],

                currentVersion: action.payload.currentVersion
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

            // Map ClusterId to a proper name
            let clusterName = "";
            if (ClusterId === "0") clusterName = "Cluster A";
            else if (ClusterId === "1") clusterName = "Cluster B";
            else if (ClusterId === "2") clusterName = "Cluster C";
            else clusterName = "Unknown Cluster"; // fallback if needed

            if (state.messages[DeviceUuid]) {
                // Device exists in messages — update it
                state.messages[DeviceUuid] = {
                    ...state.messages[DeviceUuid],
                    x: X,
                    y: Y,
                    clusterId: clusterName
                };
            } else {
                // Device does NOT exist — store it in syntheticMachines
                state.syntheticMachines[DeviceUuid] = {
                    x: X,
                    y: Y,
                    clusterId: clusterName
                };
            }
        }

    },
});

export const { updateMachine, updateMachineVersion, updateMachineStatus, updatePlotValues } = machineSlice.actions;
const machineReducer = machineSlice.reducer;
export default machineReducer;
