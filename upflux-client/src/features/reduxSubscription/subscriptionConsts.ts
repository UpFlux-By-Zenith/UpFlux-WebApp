export const GROUP_TYPES = {
    GENERIC_ALERT: "Alert",
    LICENSE_ALERT: "Alert/Licence",
    UPDATE_ALERT: "Alert/Update",
    SCHEDULED_UPDATE_ALERT: "Alert/ScheduledUpdate",
    ROLLBACK_ALERT: "Alert/Rollback",
    RECOMMENDATION_ALERT: "Recommendations/Cluster",
    STATUS_ALERT: `Status/`,
    RECOMMENDATION_PLOT: `Recommendations/Plot`
}

export interface IMachineStatus {
    DeviceUuid: string;
    IsOnline: boolean;
    LastSeen: {
        Seconds: number;
        Nanos: number;
    }
}

export interface IClusterResponse {
    DeviceUuid: string;
    X: number;
    Y: number;
    ClusterId: string;
}

