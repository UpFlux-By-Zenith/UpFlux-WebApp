import { Table, Stack, Text, Button } from "@mantine/core";
import { useEffect, useState } from "react";
import { IPackagesOnCloud, getAvailablePackages } from "../../api/applicationsRequest";
import "./admin-dashboard.css";
import { useDisclosure } from "@mantine/hooks";
import { PackageFileInput } from "./PackageFileInput";


export const UploadedApplications = () => {

    const [availableApps, setAvailableApps] = useState<IPackagesOnCloud[]>([])
    const [opened, { open, close }] = useDisclosure(false);

    useEffect(() => {
        getPackages()

    }, [])

    const getPackages = () => {
        getAvailablePackages().then(res => {
            setAvailableApps(res as IPackagesOnCloud[])
        })

    }

    return (
        <>
            <PackageFileInput opened={opened} close={close} getPackages={getPackages} />
            <Stack align="center" className="form-stack">
                <Text className="form-title">Uploaded Applications</Text>
                <Table>
                    <Button variant="filled" color="rgba(0, 3, 255, 1)" style={{ float: "right" }} onClick={open}>Upload Package</Button>
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
                </Table>
            </Stack>
        </>
    );
};
