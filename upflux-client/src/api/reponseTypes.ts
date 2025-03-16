export interface IMachine {
    machineId: string;
    machineName: string;
    dateAddedOn: string;
    ipAddress: string;
    appName: string;
    currentVersion: string | null;
}