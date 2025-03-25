import { Stack, TextInput, CopyButton, ActionIcon, Tooltip, Text, Button, Checkbox, Table } from "@mantine/core";
import { IconCheck, IconCopy, IconLicense, IconRotateClockwise } from "@tabler/icons-react";
import { useEffect, useState } from "react";
import { generateMachineId, getMachinesWithLicense } from "../../api/applicationsRequest";
import { formatTimestamp } from "../../common/appUtils";
import { createMachineLicense } from "../../api/adminApiActions";
import "./admin-dashboard.css";
export interface IMachineLicense {
    machineId: string;
    machineName: string;
    dateAddedOn: string;
    ipAddress: string;
    licenceKey: string;
    validityStatus: string;
    expirationDate: string;
}

export const ManageMachines: React.FC = () => {
    const [generatedId, setGeneratedId] = useState("Click to Generate");
    const [machines, setMachines] = useState<IMachineLicense[]>([]);
    const [refresh, setRefresh] = useState(0);

    const handleRefresh = () => {
        setRefresh(prev => prev + 1); // Changing state triggers a re-render
    };

    const handleGenerateClick = () => {
        generateMachineId().then((res) => setGeneratedId(res));
    };

    useEffect(() => {
        getMachinesWithLicense().then((res: IMachineLicense[]) => {
            setMachines(res);
        });
    }, []);

    const handleCreateLicense = (machineId: string) => {
        createMachineLicense(machineId).then(() =>
            handleRefresh()
        )
    };

    return (

        <Stack align="center" className="form-stack">
            <Text className="form-title">Manage Machines</Text>
            <div style={{ display: "flex", alignItems: "center" }}>
                <TextInput
                    label="Generate Machine ID"
                    value={generatedId}
                    disabled
                    style={{ marginLeft: "10px", width: "300px" }}
                    placeholder="Placeholder text"
                />
                <CopyButton value={generatedId} timeout={2000}>
                    {({ copied, copy }) => (
                        <Tooltip label={copied ? "Copied" : "Copy"} withArrow position="bottom">
                            <ActionIcon color={copied ? "teal" : "gray"} variant="subtle" onClick={copy} style={{ marginLeft: "10px" }}>
                                {copied ? <IconCheck size={16} /> : <IconCopy size={16} />}
                            </ActionIcon>
                        </Tooltip>
                    )}
                </CopyButton>
                <Tooltip label="Generate Machine Id" withArrow position="bottom">
                    <ActionIcon color="rgba(0, 3, 255, 1)" variant="filled" aria-label="Settings" onClick={handleGenerateClick}>
                        <IconRotateClockwise style={{ width: "70%", height: "70%" }} stroke={1.5} />
                    </ActionIcon>
                </Tooltip>
            </div>
            <Table>
                <Table.Thead>
                    <Table.Tr>
                        <Table.Th>Machine Ids</Table.Th>
                        <Table.Th>Machine Name</Table.Th>
                        <Table.Th>Added On</Table.Th>
                        <Table.Th>IP Address</Table.Th>
                        <Table.Th>License Key</Table.Th>
                        <Table.Th>Validity</Table.Th>
                        <Table.Th>Expires On</Table.Th>
                        <Table.Th>Create License</Table.Th>
                    </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                    {machines.map((element) => (
                        <Table.Tr key={element.machineId}>
                            <Table.Td>{element.machineId}</Table.Td>
                            <Table.Td>{element.machineName}</Table.Td>
                            <Table.Td>{formatTimestamp(element.dateAddedOn)}</Table.Td>
                            <Table.Td>{element.ipAddress}</Table.Td>
                            <Table.Td>{element.licenceKey ?? "Unlicensed"}</Table.Td>
                            <Table.Td>{element.validityStatus ?? "NA"}</Table.Td>
                            <Table.Td>{element.expirationDate ?? "NA"}</Table.Td>
                            <Table.Td>
                                <ActionIcon onClick={() => handleCreateLicense(element.machineId)} color="rgba(0, 3, 255, 1)" variant="filled" style={{ marginLeft: "10px" }} disabled={element.validityStatus !== null}>
                                    <IconLicense size={16} />
                                </ActionIcon>
                            </Table.Td>
                        </Table.Tr>
                    ))}
                </Table.Tbody>
            </Table>
        </Stack>

    );
};
