import { Modal, Box, Select, Button, Text } from "@mantine/core"
import { notifications } from "@mantine/notifications";
import { useEffect, useState } from "react";
import { doRollback } from "../../api/applicationsRequest";
import { useSelector } from "react-redux";
import { IApplications } from "../reduxSubscription/applicationVersions";
import { RootState } from "../reduxSubscription/store";

export const ConfigureRollback = ({ rollbackModalOpened, setRollbackModalOpened, machines }) => {

    const [selectedMachine, setSelectedMachine] = useState<string | null>(null);
    const [selectedVersion, setSelectedVersion] = useState<string | null>(null);
    const [selectedApp, setSelectedApp] = useState<string | null>(null);
    const [availableApps, setAvailableApps] = useState<any[]>([]);
    const [availableVersions, setAvailableVersions] = useState<any[]>([]);

    const applications: Record<string, IApplications> = useSelector((state: RootState) => state.applications.messages)

    useEffect(() => {
        // Fetch available applications and their versions on modal open
        if (rollbackModalOpened) {
            fetchPackages();
        }
    }, [rollbackModalOpened]);

    // Simulate an API call to fetch the application list and versions
    const fetchPackages = async () => {
        try {
            const response = await fetch('http://localhost:5000/api/PackageManagement/packages', {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${sessionStorage.getItem('authToken')}`, // Get the token from localStorage (or sessionStorage)
                },
            });
            const data = await response.json();
            setAvailableApps(data); // Set available apps and versions in state
        } catch (error) {
            console.error('Error fetching packages:', error);
            setAvailableApps([]); // Clear available apps on error
        }
    };

    // Handle Deploy Button Click
    const handleRollback = () => {
        setRollbackModalOpened(false);
        notifications.show({
            loading: true,
            title: 'Rolling back',
            position: "top-right",
            message: `${selectedMachine} is being rolled back to ${selectedVersion}`,
            autoClose: 5000,
            withCloseButton: false,
        });
        doRollback(selectedVersion, selectedMachine)

    };

    // Handle machine selection
    const handleMachineChange = (value: string | null) => {
        setSelectedMachine(value);
        if (value) {
            // Find the selected machine and set its applications
            const selectedMachineData = machines?.find(
                (machine) => machine.machineId === value
            );
            if (selectedMachineData) {
                setAvailableApps(selectedMachineData.appName);
            } else {
                setAvailableApps([]);
            }
        } else {
            setAvailableApps([]);
        }
        setSelectedApp(null); // Reset selected app
        setAvailableVersions([]); // Reset available versions
    };



    // Handle application selection
    const handleAppChange = (value: string | null) => {
        setSelectedApp(value);
        if (value) {
            // Find the selected application and set its versions
            const selectedAppData = true
            if (selectedAppData) {
                setAvailableVersions(applications[selectedMachine].VersionNames);
            } else {
                setAvailableVersions([]);
            }
        } else {
            setAvailableVersions([]);
        }
    };

    return <Modal
        opened={rollbackModalOpened}
        onClose={() => setRollbackModalOpened(false)}
        title="Configure Rollback"
        centered
    >
        <Box>
            <Text>Select Machines*</Text>
            <Select
                data={Object.keys(applications).map((machineid) => ({
                    value: machineid,
                    label: machineid, // Use machineName if available, otherwise use machineId
                }))}
                placeholder="Select Machines"
                onChange={handleMachineChange}
            />

            {selectedMachine && (
                <>
                    <Text mt="md">Select Application*</Text>
                    <Select
                        data={[{
                            value: "UpFlux-Monitoring-Service",
                            label: "UpFlux-Monitoring-Service",
                        }]}
                        placeholder="Select Application"
                        onChange={handleAppChange}
                    />
                </>
            )}

            {selectedApp && (
                <>
                    <Text mt="md">Select Software Version* (Current: {applications[selectedMachine].CurrentVersion})</Text>
                    <Select
                        data={availableVersions.map((version) => ({
                            value: version,
                            label: `${version} (on device)`,
                        }))}
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
            >
                Deploy
            </Button>
        </Box>
    </Modal>
}