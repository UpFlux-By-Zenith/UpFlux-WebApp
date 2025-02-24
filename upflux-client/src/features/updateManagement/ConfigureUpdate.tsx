import { Modal, Box, Select, Button, Text, MultiSelect } from "@mantine/core"
import { useEffect, useState } from "react"
import { deployPackage, getAvailablePackages, IPackagesOnCloud } from "../../api/applicationsRequest"
import { notifications } from "@mantine/notifications"

export const ConfigureUpdate = ({ modalOpened, setModalOpened, machineIds }) => {

    const [selectedMachineIds, setSelectedMachineIds] = useState<string[]>()
    const [selectedApp, setSelectedApp] = useState()
    const [selectedVersion, setSelectionVersion] = useState<string>("")
    const [availableApps, setAvailableApps] = useState<IPackagesOnCloud[]>([])

    useEffect(() => {

        getAvailablePackages().then(res => {
            setAvailableApps(res as IPackagesOnCloud[])
        })

    }, [])

    const handleMachineChange = (val) => {
        setSelectedMachineIds(val)
    }

    const handleAppChange = (val) => {
        setSelectedApp(val)
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
        deployPackage(selectedApp, selectedVersion, selectedMachineIds)
    }



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
                    label: machineid, // Use machineName if available, otherwise use machineId
                }))}
                placeholder="Select Machines"
                onChange={handleMachineChange}
            />

            {selectedMachineIds && (
                <>
                    <Text mt="md">Select Application*</Text>
                    <Select
                        data={[{
                            value: "upflux-monitoring-service",
                            label: "UpFlux-Monitoring-Service",
                        }]}
                        placeholder="Select Application"
                        onChange={handleAppChange}
                    />
                </>
            )}

            {selectedApp && (
                <>
                    <Text mt="md">Select Software Version*</Text>
                    <Select
                        data={availableApps[0].versions.map((version) => ({
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