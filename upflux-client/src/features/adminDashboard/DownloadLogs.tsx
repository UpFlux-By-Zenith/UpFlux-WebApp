import { Button, Select, Stack, Text, Loader } from "@mantine/core";
import { useEffect, useState } from "react";
import { getAllMachineDetails } from "../../api/applicationsRequest";

export const DownloadLogs = () => {
    const [machineId, setMachineId] = useState<string | null>(null);
    const [multiSelectOptions, setMultiSelectOptions] = useState<{ value: string; label: string }[]>([]);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        const fetchMachineDetails = async () => {
            try {
                const res = await getAllMachineDetails();
                const options = res.map((val) => ({
                    value: val.machineId,
                    label: val.machineId,
                }));
                setMultiSelectOptions(options);
            } catch (error) {
                console.error("Error fetching machine details:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchMachineDetails();
    }, []);

    return (
        <Stack align="center" className="form-stack logs">
            <Text className="form-title">QC Machine Logs</Text>


            <Button color="rgba(0, 3, 255, 1)" >Download Web Service Logs</Button>

            {loading ? (
                <Loader size="sm" />
            ) : (
                <Select
                    value={machineId}
                    onChange={setMachineId}
                    data={multiSelectOptions}
                    label="Machine ID"
                    placeholder="Select a machine"
                    clearable
                />
            )}

            <Button color="rgba(0, 3, 255, 1)" disabled={!machineId}>
                Download Machine Logs
            </Button>

            <Button color="rgba(0, 3, 255, 1)" >Download All Machine Logs</Button>
        </Stack>
    );
};
