import React, { useState, useEffect } from "react";
import {
  Box,
  Button,
  Group,
  Stack,
  Table,
  Text,
  Select,
  Modal,
  RingProgress,
  SimpleGrid,
} from "@mantine/core";
import { useLocation } from "react-router-dom";
import "./versionControl.css";
import { getMachineDetails } from "../../api/applicationsRequest";
import { useSelector } from "react-redux";
import { RootState } from "../reduxSubscription/store";
import { IApplications } from "../reduxSubscription/applicationVersions";

export const VersionControl: React.FC = () => {
  // State for Modal visibility
  const [modalOpened, setModalOpened] = useState(false);
  const applications: Record<string, IApplications> = useSelector((state: RootState) => state.applications.messages)
  const machineMetrics = useSelector(
    (state: RootState) => state.metrics.metrics
  );
  // Retrieve machine ID from the route state
  const location = useLocation();
  const machineId = location.state?.machineId || "Unknown Machine";

  // State for app versions and loading status
  const [appVersions, setAppVersions] = useState<
    { appName: string; appVersion: string; lastUpdate: string }[]
  >([]);
  const [isLoading, setIsLoading] = useState(true);

  const formatUptime = (seconds: number): string => {
    const days = Math.floor(seconds / (24 * 3600));
    const hours = Math.floor((seconds % (24 * 3600)) / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);

    return `${days}d ${hours}h ${minutes}m`;
  };


  // Mocked machine metrics data
  const metrics = [
    {
      label: "CPU",
      value: parseInt(machineMetrics[machineId]?.metrics.cpuUsage.toFixed()) || 0,
    },
    {
      label: "CPU Temp",
      value: parseInt(
        machineMetrics[machineId]?.metrics.cpuTemperature.toFixed()
      ) || 0,
    },
    {
      label: "Memory Usage",
      value: parseInt(machineMetrics[machineId]?.metrics.memoryUsage.toFixed()) || 0,
    },
    {
      label: "Disk Usage",
      value: parseInt(machineMetrics[machineId]?.metrics.diskUsage.toFixed()) || 0,
    },
  ];

  // Determine the color based on the metric value
  const getColor = (value: number) => {
    if (value <= 50) return "green";
    if (value > 50 && value <= 80) return "orange";
    return "red";
  };

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
          {/* <Button
            className="softwareDropdown"
            onClick={() => setModalOpened(true)}
          >
            Configure App Version
          </Button> */}
        </Group>

        {/* Machine Metrics Section */}
        <Box className="metrics-container">
          <SimpleGrid cols={4}>
            {metrics.map((metric, index) => (
              <RingProgress
                key={index}
                roundCaps
                size={150}
                thickness={10}
                sections={[
                  { value: metric.value, color: getColor(metric.value) },
                ]}
                transitionDuration={250}
                label={
                  <Box
                    style={{
                      display: "flex",
                      flexDirection: "column",
                      justifyContent: "center",
                      alignItems: "center",
                      height: "100%",
                    }}
                  >
                    <Text size="sm" fw="bold">
                      {metric.value}%
                    </Text>
                    <Text size="xs" mt="xs" fw="bold">
                      {metric.label}
                    </Text>
                  </Box>
                }
              />
            ))}
          </SimpleGrid>
          <center>
            <h2 style={{ textAlign: "center" }}>
              System Uptime: {formatUptime(machineMetrics[machineId]?.metrics.systemUptime || 0)}
            </h2>
          </center>
        </Box>

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
                {applications[machineId].VersionNames.map((appVersion, index) => (
                  <Table.Tr key={index}>
                    <Table.Td>UpFlux-Monitoring-Service</Table.Td>
                    <Table.Td>{appVersion}</Table.Td>
                    <Table.Td>Jane Smith</Table.Td>
                    <Table.Td>Jan 10 2024</Table.Td>
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
