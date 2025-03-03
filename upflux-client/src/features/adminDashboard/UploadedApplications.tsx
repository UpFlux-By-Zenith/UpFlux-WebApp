import { Table, Stack, Text } from "@mantine/core";
import { useEffect, useState } from "react";
import { IPackagesOnCloud, getAvailablePackages } from "../../api/applicationsRequest";

const applications = [
    {
        name: "upflux-monitoring-service",
        versions: ["1.1.5"],
    },
];

export const UploadedApplications = () => {

    const [availableApps, setAvailableApps] = useState<IPackagesOnCloud[]>([])

    useEffect(() => {

        getAvailablePackages().then(res => {
            setAvailableApps(res as IPackagesOnCloud[])
        })

    }, [])
    return (
        <Stack align="center" className="form-stack">
            <Text className="form-title">Uploaded Applications</Text>
            <Table>
                <Table.Thead>
                    <Table.Tr>
                        <Table.Th>Application Name</Table.Th>
                        <Table.Th>Versions</Table.Th>
                    </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                    {availableApps.map((app) => (
                        <Table.Tr key={app.name}>
                            <Table.Td>{app.name}</Table.Td>
                            <Table.Td>{app.versions.join(", ")}</Table.Td>
                        </Table.Tr>
                    ))}
                </Table.Tbody>
            </Table>
        </Stack>
    );
};
