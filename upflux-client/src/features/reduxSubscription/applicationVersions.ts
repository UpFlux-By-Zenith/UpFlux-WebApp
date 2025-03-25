import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface IApplications {
    VersionNames: string[];
    VersionId: any,
    AppId: any,
    UpdatedBy: string,
    Date: string,
    Application: string,
    DeviceUuid: string;
    CurrentVersion: string
}

interface AlertState {
    messages: Record<string, IApplications>;
}

// Initial state
const initialState: AlertState = {
    messages: {}, // Start with an empty object
};

// Create the slice
const applicationSlice = createSlice({
    name: "apps",
    initialState,
    reducers: {
        updateApps: (state, action: PayloadAction<IApplications>) => {
            const { DeviceUuid } = action.payload;
            state.messages[DeviceUuid] = action.payload; // Store using UUID as key
        },
    },
});

export const { updateApps } = applicationSlice.actions;
const applicationReducer = applicationSlice.reducer;
export default applicationReducer;
