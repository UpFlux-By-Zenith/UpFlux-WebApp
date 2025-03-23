export interface IMachine {
    machineId: string;
    machineName: string;
    dateAddedOn: string;
    ipAddress: string;
    appName: string;
    currentVersion: string | null;
    isOnline?: boolean;
    lastSeen?: {
        seconds: number;
        nanos: number;
    };
    lastUpdatedBy: string
    x?: number;
    y?: number;
    clusterId?: string;
}