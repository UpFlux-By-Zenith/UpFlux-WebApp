import React, { useState, useEffect } from "react";
import { Box, Button, Group, Stack, Table, Text, Select, Modal } from "@mantine/core";
import { useLocation } from "react-router-dom";
import "./versionControl.css";
import { getMachineDetails } from "../../api/applicationsRequest"; 

export const VersionControl: React.FC = () => {
  // State for Modal visibility
  const [modalOpened, setModalOpened] = useState(false);

  // Retrieve machine ID from the route state
  const location = useLocation();
  const machineId = location.state?.machineId || "Unknown Machine";

  // State for app versions and loading status
  const [appVersions, setAppVersions] = useState<
    { appName: string; appVersion: string; lastUpdate: string }[]
  >([]);
  const [isLoading, setIsLoading] = useState(true);

  // Fetch data from API
  useEffect(() => {
    const fetchMachineDetails = async () => {
      try {
        const data = await getMachineDetails();
        if (data && typeof data !== "string" && data.applications) {
          // Filter applications for the current machineId
          const filteredApps = data.applications
            .filter((app) => app.machineId === machineId)
            .map((app) => ({
              appName: app.appName,
              appVersion: app.currentVersion,
              lastUpdate: app.versions?.[0]?.date || "N/A",
            }));

          setAppVersions(filteredApps);
        } else {
          console.error("Failed to fetch or parse machine details.");
        }
      } catch (error) {
        console.error("Error fetching machine details:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchMachineDetails();
  }, [machineId]);

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
          {isLoading ? (
            <Text>Loading app versions...</Text>
          ) : (
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
                {appVersions.map((appVersion, index) => (
                  <Table.Tr key={index}>
                    <Table.Td>{appVersion.appName}</Table.Td>
                    <Table.Td>{appVersion.appVersion}</Table.Td>
                    <Table.Td>Jane Smith</Table.Td>
                    <Table.Td>{appVersion.lastUpdate}</Table.Td>
                    <Table.Td>Alice Cole</Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          )}
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
            data={appVersions.map((app) => app.appName)}
            placeholder="Select App"
          />
          <Text mt="md">Select App Version*</Text>
          <Select
            data={appVersions.map((app) => app.appVersion)}
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
