import { createSlice, PayloadAction } from "@reduxjs/toolkit";



interface ConnectionState {
    isConnected: boolean;
}

// Initial state
const initialState: ConnectionState = {
    isConnected: false, // Start with an empty object
};

// Create the slice
const connectionSlice = createSlice({
    name: "connection",
    initialState,
    reducers: {
        changeConnectionStatus: (_state, action: PayloadAction<boolean>) => {
            return { isConnected: action.payload }
        },

    },
});

export const { changeConnectionStatus } = connectionSlice.actions;
const connectionReducer = connectionSlice.reducer;
export default connectionReducer;
