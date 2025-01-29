import React, { useState, useEffect } from "react";
import { Box, Button, Group, Stack, Table, Text, Badge, Modal, Select } from "@mantine/core";
import { DonutChart } from "@mantine/charts";
import { Link, useNavigate } from "react-router-dom";
import { getAccessibleMachines } from "../../api/accessMachinesRequest";
import "./update-management.css";
import view from "../../assets/images/view.png";
import updateIcon from "../../assets/images/updateIcon.jpg";	
import { useSubscription } from "../reduxSubscription/useSubscription";
import { useAuth } from "../../common/authProvider/AuthProvider";

export const UpdateManagement: React.FC<{ addNotification: any }> = ({
  addNotification,
}) => {
  const [modalOpened, setModalOpened] = useState(false);
  const [machines, setMachines] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedMachine, setSelectedMachine] = useState<string | null>(null);
  const [selectedVersion, setSelectedVersion] = useState<string | null>(null);
  const navigate = useNavigate();
  const { authToken } = useAuth();

  useSubscription(authToken);

  // Fetch accessible machines on component load
  useEffect(() => {
    const fetchMachines = async () => {
      const result = await getAccessibleMachines();
      if (typeof result === "object" && result?.accessibleMachines?.result) {
        setMachines(result.accessibleMachines.result);
      } else {
        console.error("Failed to fetch machines:", result);
        setMachines([]); // Clear machines in case of failure
      }
      setLoading(false);
    };
    fetchMachines();
  }, []);

  // Handle Deploy Button Click
  const handleDeploy = () => {
    if (!selectedMachine || !selectedVersion) {
      alert("Please select both a machine and a version.");
      return;
    }

    // Create a new notification
    const newNotification = {
      id: Date.now(),
      message: `Update started for ${selectedMachine} to ${selectedVersion}`,
      image: updateIcon,
      timestamp: new Date().toLocaleTimeString(),
    };

    // Add the new notification via the passed down addNotification function
    addNotification(newNotification);

    // Close modal
    setModalOpened(false);
  };

  // Chart data for multiple measures
  const chartData = [
    { name: "Alive", value: machines.length, color: "#40C057" },
    { name: "Shutdown", value: 0, color: "#FA5252" },
    { name: "Unknown", value: 0, color: "#6c757d" },
  ];

  return (
    <Stack className="update-management-content">
      <Box className="header">
        <Text size="xl" fw={700}>
          Update Management
        </Text>
      </Box>

      <Box className="content-wrapper">
        <Group className="overview-section">
          <Box className="chart"></Box>

          <Stack className="legend">
            <Group className="legend-item">
              <Box className="circle green"></Box>
              <Text size="sm">{1} Alive</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle red"></Box>
              <Text size="sm">{0} Shutdown</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle gray"></Box>
              <Text size="sm">{0} Unknown</Text>
            </Group>
          </Stack>

          <Stack className="button-group">
            <Button
              className="configure-button"
              onClick={() => setModalOpened(true)}
            >
              Configure Update
            </Button>
            <Button
              className="smart-button"
              onClick={() => navigate("/clustering")}
            >
              Smart Update
            </Button>
          </Stack>
        </Group>

        <Box>
          {loading ? (
            <Text>Loading machines...</Text>
          ) : (
            <Table className="machine-table" highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Machine ID</Table.Th>
                  <Table.Th>IP Address</Table.Th>
                  <Table.Th>Last Update</Table.Th>
                  <Table.Th>Updated By</Table.Th>
                  <Table.Th>Current Status</Table.Th>
                  <Table.Th>View</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {machines.map((machine) => (
                  <Table.Tr key={machine.machineId}>
                    <Table.Td>{machine.machineId}</Table.Td>
                    <Table.Td>{machine.ipAddress || "N/A"}</Table.Td>
                    <Table.Td>{"02/08/2024"}</Table.Td>
                    <Table.Td>{"John Doe"}</Table.Td>
                    <Table.Td>
                      <Badge color="green">{machine.status || "Alive"}</Badge>
                    </Table.Td>
                    <Table.Td>
                      <Link
                        to="/version-control"
                        state={{ machineId: machine.machineId }}
                      >
                        <img src={view} alt="view" className="view" />
                      </Link>
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          )}
        </Box>
      </Box>

      <Modal
        opened={modalOpened}
        onClose={() => setModalOpened(false)}
        title="Configure Update"
        centered
      >
        <Box>
          <Text>Select Machines*</Text>
          <Select
            data={machines.map((machine) => `Machine ${machine.machineId}`)}
            placeholder="Select Machines"
            onChange={(value) => setSelectedMachine(value || null)}
          />
          <Text mt="md">Select Software Version*</Text>
          <Select
            data={["Version 2.5.0", "Version 1.8.2", "Version 3.1.0"]}
            placeholder="Select Version"
            onChange={(value) => setSelectedVersion(value || null)}
          />
          <Button mt="md" fullWidth onClick={handleDeploy}>
            Deploy
          </Button>
        </Box>
      </Modal>
    </Stack>
  );
};
