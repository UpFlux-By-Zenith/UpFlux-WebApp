import { useState } from "react";
import { Table, ScrollArea, TextInput, Button } from "@mantine/core";
import { useSelector, useDispatch } from "react-redux";
import { IconMenu3, IconSearch, IconTrash } from "@tabler/icons-react";
import { IAlertMessage, clearAlerts } from "../reduxSubscription/alertSlice";
import { useDisclosure } from "@mantine/hooks";
import { DownloadLogs } from "../adminDashboard/DownloadLogs";

export const NotificationLogger = () => {
    const dispatch = useDispatch();
    const allAlerts = useSelector((state: any) => state.alerts.messages); // Redux store
    const [search, setSearch] = useState("");
    const [sortBy, setSortBy] = useState<keyof IAlertMessage | null>(null);
    const [sortOrder, setSortOrder] = useState<"asc" | "desc">("desc");

    const [opened, { open, close }] = useDisclosure(false);

    // **LIMIT TO 100 ALERTS**
    const alerts = allAlerts.slice(-100);

    // **FILTER & SORT ALERTS**
    const filteredAlerts = alerts
        .filter((alert) =>
            Object.values(alert).some((value: any) =>
                value.toLowerCase().includes(search.toLowerCase())
            )
        )
        .sort((a, b) => {
            if (!sortBy) return 0;
            return sortOrder === "asc"
                ? a[sortBy].localeCompare(b[sortBy])
                : b[sortBy].localeCompare(a[sortBy]);
        });

    return (
        <>
            <DownloadLogs opened={opened} close={close} />
            <ScrollArea style={{ maxHeight: "500px" }}>
                <div style={{ display: "flex", gap: "10px", margin: "10px" }} >
                    <TextInput
                        placeholder="Search alerts..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)} />
                    <Button
                        color="red"
                        onClick={() => dispatch(clearAlerts())}
                    >
                        Clear Alerts
                    </Button>
                    <Button
                        rightSection={<IconMenu3 size={14} />}
                        color="rgba(0, 3, 255, 1)"
                        onClick={open}
                    >
                        Export Logs
                    </Button>
                </div>

                <Table striped highlightOnHover withColumnBorders>
                    <Table.Thead>
                        <Table.Tr>
                            {["timestamp", "level", "message", "source"].map((key) => (
                                <Table.Th
                                    key={key}
                                    onClick={() => {
                                        setSortBy(key as keyof IAlertMessage);
                                        setSortOrder(sortOrder === "asc" ? "desc" : "asc");
                                    }}
                                    style={{ cursor: "pointer" }}
                                >
                                    {key.toUpperCase()} {sortBy === key ? (sortOrder === "asc" ? "▲" : "▼") : ""}
                                </Table.Th>
                            ))}
                        </Table.Tr>
                    </Table.Thead>
                    <Table.Tbody>
                        {filteredAlerts.length > 0 ? (
                            filteredAlerts.map((alert, index) => (
                                <Table.Tr key={index}>
                                    <Table.Td>{new Date(alert.timestamp).toUTCString()}</Table.Td>
                                    <Table.Td>{alert.level}</Table.Td>
                                    <Table.Td>{alert.message}</Table.Td>
                                    <Table.Td>{alert.source}</Table.Td>
                                </Table.Tr>
                            ))
                        ) : (
                            <Table.Tr>
                                <Table.Td colSpan={4} style={{ textAlign: "center", fontWeight: "bold" }}>
                                    No alerts found.
                                </Table.Td>
                            </Table.Tr>
                        )}
                    </Table.Tbody>
                </Table>
            </ScrollArea ></>
    );
};
