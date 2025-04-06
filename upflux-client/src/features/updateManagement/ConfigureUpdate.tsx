import { Modal, Box, Select, Button, Text, MultiSelect } from "@mantine/core"
import { useEffect, useState } from "react"
import { deployPackage, getAvailablePackages, IPackagesOnCloud } from "../../api/applicationsRequest"
import { notifications } from "@mantine/notifications"

export const ConfigureUpdate = ({ modalOpened, setModalOpened, machines }) => {

    const [selectedMachineIds, setSelectedMachineIds] = useState<string[]>()
    const [selectedVersion, setSelectionVersion] = useState<string>("")
    const [availableApps, setAvailableApps] = useState<IPackagesOnCloud[]>([])

    const machineIds = Object.keys(machines)

    useEffect(() => {

        getAvailablePackages().then(res => {
            setAvailableApps(res as IPackagesOnCloud[])
        })

    }, [])

    const handleMachineChange = (val) => {
        setSelectedMachineIds(val)
    }

    const handleDeploy = () => {
        setModalOpened(false);
        notifications.show({
            loading: true,
            title: 'Updating QC Machines',
            position: "top-right",
            message: `${selectedMachineIds} is being updated to ${selectedVersion}`,
            autoClose: 5000,
            withCloseButton: false,
        });

        deployPackage(selectedApp[0].value, selectedVersion, selectedMachineIds)
            .then(() => {
            })
            .catch(err => {
                notifications.show({
                    title: "Update Failed",
                    message: "An error occurred during update.",
                    color: "red",
                    autoClose: 3000,
                });
            });
    };

    const selectedApp = [{
        value: "upflux-monitoring-service",
        label: "UpFlux-Monitoring-Service",
    }]

    return <Modal
        opened={modalOpened}
        onClose={() => setModalOpened(false)}
        title="Configure Updates"
        centered
    >
        <Box>
            <Text>Select Machines*</Text>
            <MultiSelect
                data={machineIds.map((machineid) => ({
                    value: machineid,
                    label: machineid,
                    disabled: !machines[machineid].isOnline // Use machineName if available, otherwise use machineId
                }))}
                placeholder="Select Machines"
                onChange={handleMachineChange}
            />

            {selectedMachineIds && (
                <>
                    <Text mt="md">Select Application*</Text>
                    <Select
                        data={selectedApp}
                        value={selectedApp[0].value}
                        placeholder="Select Application"
                        disabled
                    />
                </>
            )}

            {selectedApp && (
                <>
                    <Text mt="md">Select Software Version*</Text>
                    <Select
                        data={availableApps[0]?.versions.map((version) => ({
                            value: version,
                            label: version,
                        }))}
                        placeholder="Select Version"
                        onChange={(value) => setSelectionVersion(value)}
                    />
                </>
            )}

            <Button
                color="rgba(0, 3, 255, 1)"
                mt="md"
                fullWidth
                onClick={handleDeploy}
            >
                Deploy
            </Button>
        </Box>
    </Modal>
}