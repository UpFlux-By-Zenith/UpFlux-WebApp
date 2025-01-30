import { Stack, TextInput, CopyButton, ActionIcon, Tooltip, Text, Button } from "@mantine/core"
import { IconAdjustments, IconCheck, IconCopy, IconRotateClockwise } from "@tabler/icons-react"
import { useState } from "react"
import { generateMachineId } from "../../api/applicationsRequest"

export const ManageMachines: React.FC = () => {

    const [generatedId, setGeneratedId] = useState("Click to Generate")

    const handleGenerateClick = () => {
        generateMachineId().then(res => setGeneratedId(res))
    }

    return <>
        <Stack align="center" className="form-stack">
            <Text className="form-title"> Manage Machines</Text>
            <div style={{ display: 'flex', alignItems: 'center' }}>

                {/* Disabled Textbox */}
                <TextInput
                    value={generatedId}
                    disabled
                    style={{ marginLeft: '10px', width: '300px' }}
                    placeholder="Placeholder text"
                />

                {/* Copy Button beside the Textbox */}
                <CopyButton value={generatedId} timeout={2000}>
                    {({ copied, copy }) => (
                        <Tooltip label={copied ? "Copied" : "Copy"} withArrow position="bottom">
                            <ActionIcon
                                color={copied ? "teal" : "gray"}
                                variant="subtle"
                                onClick={copy}
                                style={{ marginLeft: '10px' }}
                            >
                                {copied ? <IconCheck size={16} /> : <IconCopy size={16} />}
                            </ActionIcon>
                        </Tooltip>
                    )}
                </CopyButton>
                {/* Button */}
                <Tooltip label="Generate Machine Id" withArrow position="bottom">
                    <ActionIcon color="rgba(0, 3, 255, 1)" variant="filled" aria-label="Settings" onClick={handleGenerateClick}>
                        <IconRotateClockwise style={{ width: '70%', height: '70%' }} stroke={1.5} />
                    </ActionIcon>
                </Tooltip>

            </div>
        </Stack>
    </>
}