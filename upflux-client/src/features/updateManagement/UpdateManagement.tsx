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
import { deployPackage, getRunningMachinesApplications } from "../../api/applicationsRequest";
import { notifications } from "@mantine/notifications";

export const UpdateManagement = () => {
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

    const getRunningMachines = async () => {
      await getRunningMachinesApplications().then(res => {
        console.log(res)
      })
    }

    getRunningMachines()
    fetchMachines();
  }, []);

  // Handle Deploy Button Click
  const handleDeploy = () => {
    if (!selectedMachine || !selectedVersion) {
      alert("Please select both a machine and a version.");
      return;
    }

    deployPackage(selectedApp, selectedVersion, [selectedMachine]).then(() => {

      // Create a new notification
      const newNotification = {
        id: Date.now(),
        message: `Update started for ${selectedMachine} to ${selectedVersion}`,
        image: updateIcon,
        timestamp: new Date().toLocaleTimeString(),
      };


      // Close modal
      setModalOpened(false);
    }).catch(() => {
      alert("Err")
    })

  };

  // Chart data for multiple measures
  const chartData = [
    { name: "Alive", value: machines.length, color: "#40C057" },
    { name: "Shutdown", value: 0, color: "#FA5252" },
    { name: "Unknown", value: 0, color: "#6c757d" },
  ];

  const [selectedApp, setSelectedApp] = useState(null);
  const [availableVersions, setAvailableVersions] = useState([]);
  const [availableApps, setAvailableApps] = useState([]);

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

  useEffect(() => {
    // Fetch available applications and their versions on modal open
    if (modalOpened) {
      fetchPackages();
    }
  }, [modalOpened]);

  const handleAppChange = (value) => {
    setSelectedApp(value);

    // Find the selected app and set available versions
    const app = availableApps.find((app) => app.name === value);
    if (app) {
      setAvailableVersions(app.versions);
      setSelectedVersion(app.versions[0]); // Optionally, set the first available version as default
    } else {
      setAvailableVersions([]);
    }
  };

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

      <Modal opened={modalOpened} onClose={() => setModalOpened(false)} title="Configure Update" centered>
        <Box>
          <Text>Select Machines*</Text>
          <Select
            data={machines.map((machine) => `${machine.machineId}`)}
            placeholder="Select Machines"
            onChange={(value) => setSelectedMachine(value || null)}
          />

          <Text mt="md">Select Application*</Text>
          <Select
            data={availableApps.map((app) => app.name)} // Populate based on available apps
            placeholder="Select Application"
            onChange={handleAppChange}
          />

          <Text mt="md">Select Software Version*</Text>
          <Select
            data={availableVersions} // Available versions will be updated based on app selection
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
