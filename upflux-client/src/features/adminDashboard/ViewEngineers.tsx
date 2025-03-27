import { Table, Stack, Text, ActionIcon, Button, Modal, Input, CloseButton } from "@mantine/core";
import { useEffect, useState } from "react";
import { getAllEngineers } from "../../api/applicationsRequest";
import { formatTimestamp } from "../../common/appUtils";
import "./admin-dashboard.css";
import { IconCopyXFilled } from "@tabler/icons-react";
import { useDisclosure } from "@mantine/hooks";
import { ROLES, useAuth } from "../../common/authProvider/AuthProvider";

import { GetEngineerToken } from "./GetEngineerToken";

export interface IEngineer {
    userId: string,
    name: string,
    email: string,
    lastLogin: string,
    role: 1 | 0,
}

export const ViewEngineers = () => {
    const [rows, setRows] = useState<JSX.Element[]>([]);
    const [selectedEngineer, setSelectedEngineer] = useState<IEngineer>();
    const { userRole } = useAuth()
    useEffect(() => {
        if (userRole === ROLES.ADMIN)
            getAllEngineers().then((res: IEngineer[]) => {
                const mappedRows = res.map((engineer) => (
                    <Table.Tr key={engineer.userId}>
                        <Table.Td>{engineer.userId}</Table.Td>
                        <Table.Td>{engineer.name}</Table.Td>
                        <Table.Td>{engineer.email}</Table.Td>
                        <Table.Td>{formatTimestamp(engineer.lastLogin)}</Table.Td>
                        <Table.Td>{engineer.role === 1 ? "Engineer" : "Admin"}</Table.Td>
                        <Table.Td>
                            <ActionIcon onClick={() => handleRevokeModal(engineer)} disabled={engineer.role === 0} variant="filled" style={{ marginLeft: "25px", alignContent: "center" }} color="red">
                                <IconCopyXFilled />
                            </ActionIcon>
                        </Table.Td>
                    </Table.Tr>
                ));
                setRows(mappedRows);
            });
    }, []);

    const [opened, { open, close }] = useDisclosure(false);
    const [createTokenOpened, { open: openCreateToken, close: closeCreateToken }] = useDisclosure(false);


    const handleRevokeModal = (engineer: IEngineer) => {
        open()
        setSelectedEngineer(engineer)
    }
    const [value, setValue] = useState('');
    return (
        <Stack align="center" className="form-stack">
            <Text className="form-title">View Engineers</Text>
            <GetEngineerToken opened={createTokenOpened} close={closeCreateToken} />
            <Modal opened={opened} onClose={close} title="Confirmation" centered>
                <Text>Are you sure to Revoke {selectedEngineer?.name} account?</Text>
                <Input
                    placeholder="Reason for revocation (optional)"
                    value={value}
                    onChange={(event) => setValue(event.currentTarget.value)}
                    rightSectionPointerEvents="all"
                    mt="md"
                    rightSection={
                        <CloseButton
                            aria-label="Clear input"
                            onClick={() => setValue('')}
                            style={{ display: value ? undefined : 'none' }}
                        />
                    }
                />

                <Button color="red" style={{ margin: "10px", float: "right" }} >
                    Revoke
                </Button>
            </Modal>
            <Table>
                <Button variant="filled" color="rgba(0, 3, 255, 1)" style={{ float: "right" }} onClick={openCreateToken}>Create Token</Button>
                <Table>
                    <Table.Thead>
                        <Table.Tr>
                            <Table.Th>Engineer ID</Table.Th>
                            <Table.Th>Name</Table.Th>
                            <Table.Th>Email</Table.Th>
                            <Table.Th>Last Login</Table.Th>
                            <Table.Th>Role</Table.Th>
                            <Table.Td>Revoke Token</Table.Td>
                        </Table.Tr>
                    </Table.Thead>
                    <Table.Tbody>{rows}</Table.Tbody>
                </Table>
            </Table>
        </Stack>
    );
}
