import React, { useState } from "react";
import { Box, Button, Group, Stack, Table, Text, Select, Modal } from "@mantine/core";
import { useLocation } from "react-router-dom";
import "./versionControl.css";

export const VersionControl: React.FC = () => {
  // State for Modal visibility
  const [modalOpened, setModalOpened] = useState(false);

  // Retrieve machine ID from the route state
  const location = useLocation();
  const machineId = location.state?.machineId || "Unknown Machine";

  // Hardcoded data for the table
  const appVersions = [
    { appName: "App 1", appVersion: "2.5.0", addedBy: "John Doe", lastUpdate: "2025-01-10 12:30:00", updatedBy: "John Doe" },
    { appName: "App 2", appVersion: "1.8.2", addedBy: "Jane Smith", lastUpdate: "2024-12-15 09:45:00", updatedBy: "Mark Johnson" },
    { appName: "App 3", appVersion: "3.1.0", addedBy: "Emily Davis", lastUpdate: "2025-01-05 16:20:00", updatedBy: "John Doe" },
    { appName: "App 4", appVersion: "4.0.1", addedBy: "Chris Wilson", lastUpdate: "2025-01-01 14:00:00", updatedBy: "Jane Smith" },
    { appName: "App 5", appVersion: "2.9.7", addedBy: "Alex Brown", lastUpdate: "2024-11-20 10:30:00", updatedBy: "Emily Davis" },
  ];

  return (
    <Stack className="version-control-content">
      {/* Header */}
      <Box className="header">
        <Text size="xl" fw={700}>
          Version Control
        </Text>
      </Box>

      <Box className="content-wrapper">
        <Box className="machine-id-box">
          {/* Display the machine ID */}
          <Text>{`Machine ${machineId}`}</Text>
        </Box>

        {/* Overview Section */}
        <Group className="overview-section">
          {/* Action Buttons */}
          <Button className="softwareDropdown" onClick={() => setModalOpened(true)}>
            Configure App Version
          </Button>
        </Group>

        {/* Table Section */}
        <Box>
          <Table className="version-table" highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>App Name</Table.Th>
                <Table.Th>Version</Table.Th>
                <Table.Th>Added By</Table.Th>
                <Table.Th>Last Updated</Table.Th>
                <Table.Th>Updated By</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {appVersions.map((appVersion) => (
                <Table.Tr key={appVersion.appVersion}>
                  <Table.Td>{appVersion.appName}</Table.Td>
                  <Table.Td>{appVersion.appVersion}</Table.Td>
                  <Table.Td>{appVersion.addedBy}</Table.Td>
                  <Table.Td>{appVersion.lastUpdate}</Table.Td>
                  <Table.Td>{appVersion.updatedBy}</Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Box>
      </Box>

      {/* Modal for Configure Update */}
      <Modal
        opened={modalOpened}
        onClose={() => setModalOpened(false)}
        title="Configure Update"
        centered
      >
        <Box>
          <Text>Select App*</Text>
          <Select
            data={["App 1", "App 2", "App 3", "App 4", "App 5"]} 
            placeholder="Select App"
          />
          <Text mt="md">Select App Version*</Text>
          <Select
            data={["Version 2.5.0", "Version 1.8.2", "Version 3.1.0"]}
            placeholder="Select App Version"
          />
          <Button mt="md" fullWidth>
            Deploy
          </Button>
        </Box>
      </Modal>
    </Stack>
  );
};
