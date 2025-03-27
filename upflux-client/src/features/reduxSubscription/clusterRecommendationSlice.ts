import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface IClusterRecommendation {
    ClusterId: string;
    DeviceUuids: string[];
    UpdateTime: {
        Seconds: number;
        Nanos: number;
    };
}

// Initial state
const initialState: IClusterRecommendation[] = [];

// Create the slice
const clusterRecommendationSlice = createSlice({
    name: "clusterRecommendation",
    initialState,
    reducers: {
        updateClusterRecommendation: (state, action: PayloadAction<IClusterRecommendation>) => {
            const index = state.findIndex(cluster => cluster.ClusterId === action.payload.ClusterId);

            if (index !== -1) {
                // Update existing cluster
                state[index] = action.payload;
            } else {
                // Add new cluster
                state.push(action.payload);
            }
        },
    },
});

export const { updateClusterRecommendation } = clusterRecommendationSlice.actions;
const clusterRecommendationReducer = clusterRecommendationSlice.reducer
export default clusterRecommendationReducer;
