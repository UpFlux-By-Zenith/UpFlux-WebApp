import { Modal, Box, Select, Button, Text, MultiSelect } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useEffect, useState } from "react";
import { doRollback, getMachineStoredVersions, IStoredVersionsResponse } from "../../api/applicationsRequest";
import { IMachine } from "../../api/reponseTypes";

interface Props {
    rollbackModalOpened: boolean;
    setRollbackModalOpened: (opened: boolean) => void;
    machines: IMachine[];
}

export const ConfigureRollback = ({ rollbackModalOpened, setRollbackModalOpened, machines }: Props) => {
    const [selectedMachines, setSelectedMachines] = useState<string[]>([]);
    const [selectedVersion, setSelectedVersion] = useState<string | null>(null);
    const [availableVersions, setAvailableVersions] = useState<string[]>([]);
    const [allAvailableVersions, setAllAvailableVersions] = useState<IStoredVersionsResponse[]>([]);

    useEffect(() => {
        const fetchVersions = async () => {
            const res = await getMachineStoredVersions();
            setAllAvailableVersions(res);
        };

        if (rollbackModalOpened) {
            fetchVersions();
        }
    }, [rollbackModalOpened]);

    useEffect(() => {
        if (selectedMachines.length > 0) {
            const machineVersions = selectedMachines.map(machineId =>
                allAvailableVersions.filter(v => v.machineId === machineId).map(v => v.versionName)
            );

            const commonVersions = machineVersions.reduce((a, b) => a.filter(c => b.includes(c)), machineVersions[0] || []);

            setAvailableVersions(commonVersions);
        } else {
            setAvailableVersions([]);
        }
    }, [selectedMachines, allAvailableVersions]);

    const handleRollback = async () => {
        if (selectedMachines.length === 0 || !selectedVersion) return;

        setRollbackModalOpened(false);
        notifications.show({
            loading: true,
            title: "Rolling back",
            position: "top-right",
            message: `${selectedMachines.join(", ")} is being rolled back to ${selectedVersion}`,
            autoClose: 5000,
            withCloseButton: false,
        });

        try {
            await doRollback(selectedVersion, selectedMachines);

        } catch (error) {
            console.error("Rollback failed:", error);
            notifications.show({
                title: "Rollback Failed",
                message: "An error occurred during rollback.",
                color: "red",
                autoClose: 3000,
            });
        }
    };


    const handleMachineChange = (value: string[]) => {
        setSelectedMachines(value);
        setAvailableVersions([]); // Reset available versions
        setSelectedVersion("")
    };

    const selectedApp = [{
        value: "upflux-monitoring-service",
        label: "UpFlux-Monitoring-Service",
    }]

    return (
        <Modal
            opened={rollbackModalOpened}
            onClose={() => setRollbackModalOpened(false)}
            title="Configure Rollback"
            centered
        >
            <Box>
                <Text>Select Machine*</Text>
                <MultiSelect
                    data={machines.map((machine) => ({
                        value: machine.machineId,
                        label: machine.machineId,
                    }))}
                    placeholder="Select Machines"
                    onChange={handleMachineChange}
                />

                {selectedMachines.length > 0 && (
                    <>
                        <Text mt="md">Select Application*</Text>
                        <Select
                            data={selectedApp}
                            value={selectedApp[0].value}
                            placeholder="Select Application"
                            disabled
                        />

                        <Text mt="md">Select Software Version*</Text>
                        <Select
                            data={availableVersions.map((version) => ({
                                value: version,
                                label: `${version}`
                            }))}
                            value={selectedVersion}
                            placeholder="Select Version"
                            onChange={(value) => setSelectedVersion(value || null)}
                        />
                    </>
                )}

                <Button
                    color="rgba(0, 3, 255, 1)"
                    mt="md"
                    fullWidth
                    onClick={handleRollback}
                    disabled={selectedMachines.length === 0 || !selectedVersion}
                >
                    Deploy
                </Button>
            </Box>
        </Modal>
    );
};
