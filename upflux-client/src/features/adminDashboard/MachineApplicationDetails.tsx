import { useEffect, useState } from "react";
import { Select, Table, Stack, Text } from "@mantine/core";
import { getAllMachinesWithApps } from "../../api/applicationsRequest";

interface IApplicationVersion {
    versionId: number;
    appId: number;
    versionName: string;
    updatedBy: string;
    date: string;
}

interface IApplication {
    appId: number;
    machineId: string;
    appName: string;
    addedBy: string;
    currentVersion: string;
    versions: IApplicationVersion[];
}

interface IMachine {
    machineId: string;
    machineName: string;
    dateAddedOn: string;
    ipAddress: string;
    applications: IApplication[];
}


export const MachineApplicationDetails = () => {
    const [selectedMachine, setSelectedMachine] = useState<string | null>(null);
    const [machines, setMachines] = useState<IMachine[]>([])

    useEffect(() => {

        getAllMachinesWithApps().then(res =>
            setMachines(res)
        )

    }, [])

    const machineOptions = machines?.map((machine) => ({
        value: machine.machineId,
        label: machine.machineName,
    }));

    const selectedMachineData = machines?.find(
        (machine) => machine.machineId === selectedMachine
    );

    return (
        <Stack align="center" className="form-stack">
            <Text className="form-title">Applications History</Text>
            <Select
                label="Select a Machine"
                placeholder="Choose a Machine"
                data={machineOptions}
                value={selectedMachine}
                onChange={setSelectedMachine}
            />
            {selectedMachineData && (
                <Table >
                    <Table.Thead>
                        <Table.Tr>
                            <Table.Th>Application Name</Table.Th>
                            <Table.Th>Added By</Table.Th>
                            <Table.Th>Current Version</Table.Th>
                            <Table.Th>Versions</Table.Th>
                        </Table.Tr>
                    </Table.Thead>
                    <Table.Tbody>
                        {selectedMachineData.applications.map((app) => (
                            <Table.Tr key={app.appId}>
                                <Table.Td>{app.appName}</Table.Td>
                                <Table.Td>{app.addedBy}</Table.Td>
                                <Table.Td>{app.currentVersion}</Table.Td>
                                <Table.Td>
                                    {app.versions.length > 0 ? (
                                        <ul>
                                            {app.versions.map((version) => (
                                                <li key={version.versionId}>
                                                    {version.versionName} (Updated by: {version.updatedBy} on {new Date(version.date).toLocaleDateString()})
                                                </li>
                                            ))}
                                        </ul>
                                    ) : (
                                        "No Versions Available"
                                    )}
                                </Table.Td>
                            </Table.Tr>
                        ))}
                    </Table.Tbody>
                </Table>
            )}
        </Stack>
    );
};
