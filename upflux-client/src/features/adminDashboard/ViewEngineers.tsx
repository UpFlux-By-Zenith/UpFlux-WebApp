import { Table, Stack, Text, ActionIcon } from "@mantine/core";
import { useEffect, useState } from "react";
import { getAllEngineers } from "../../api/applicationsRequest";
import { formatTimestamp } from "../../common/appUtils";
import "./admin-dashboard.css";
import { IconCopyXFilled } from "@tabler/icons-react";
export interface IEngineers {
    userId: string,
    name: string,
    email: string,
    lastLogin: string,
    role: 1,
}

export const ViewEngineers = () => {
    const [rows, setRows] = useState<JSX.Element[]>([]);

    useEffect(() => {
        getAllEngineers().then((res: IEngineers[]) => {
            const mappedRows = res.map((engineer) => (
                <Table.Tr key={engineer.userId}>
                    <Table.Td>{engineer.userId}</Table.Td>
                    <Table.Td>{engineer.name}</Table.Td>
                    <Table.Td>{engineer.email}</Table.Td>
                    <Table.Td>{formatTimestamp(engineer.lastLogin)}</Table.Td>
                    <Table.Td>
                        <ActionIcon>
                            <IconCopyXFilled />
                        </ActionIcon>
                    </Table.Td>
                </Table.Tr>
            ));
            setRows(mappedRows);
        });
    }, []);

    return (
        <Stack align="center" className="form-stack">
            <Text className="form-title">View Engineers</Text>
            <Table.ScrollContainer minWidth={500} type="native">
                <Table>
                    <Table.Thead>
                        <Table.Tr>
                            <Table.Th>Engineer ID</Table.Th>
                            <Table.Th>Name</Table.Th>
                            <Table.Th>Email</Table.Th>
                            <Table.Th>Last Login</Table.Th>
                            <Table.Td>Revoke Token</Table.Td>
                        </Table.Tr>
                    </Table.Thead>
                    <Table.Tbody>{rows}</Table.Tbody>
                </Table>
            </Table.ScrollContainer>
        </Stack>
    );
}
