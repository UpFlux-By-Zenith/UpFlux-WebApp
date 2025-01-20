import React, { useState, useEffect } from "react";
import { Box, Button, Group, Stack, Table, Text, Badge, Modal, Select } from "@mantine/core";
import { DonutChart } from '@mantine/charts';
import { Link, useNavigate } from "react-router-dom";
import { getAccessibleMachines } from "../../api/accessMachinesRequest";
import "./update-management.css";
import view from "../../assets/images/view.png";

export const UpdateManagement: React.FC = () => {
  const [modalOpened, setModalOpened] = useState(false);
  const [machines, setMachines] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

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

  // Count machines based on their status (mocked for now)
  var aliveMachines = machines.filter((machine) => machine.status === "Alive").length;
  var shutdownMachines = machines.filter((machine) => machine.status === "Shutdown").length;
  var unknownMachines = machines.filter((machine) => machine.status === "Unknown").length;

  //For simulation purposes
  aliveMachines = 2;
  shutdownMachines = 0;
  unknownMachines = 0;

  // Chart data for multiple measures
  const chartData = [
    { name: "Alive", value: aliveMachines, color: "#40C057" }, // Green for Alive
    { name: "Shutdown", value: shutdownMachines, color: "#FA5252" }, // Red for Shutdown
    { name: "Unknown", value: unknownMachines, color: "#6c757d" }, // Grey for Unknown
  ];

  return (
    <Stack className="update-management-content">
      {/* Header */}
      <Box className="header">
        <Text size="xl" fw={700}>
          Update Management
        </Text>
      </Box>

      <Box className="content-wrapper">
        {/* Overview Section */}
        <Group className="overview-section">
          {/* Chart Section */}
          <Box className="chart">
            <DonutChart className="chart" withTooltip={false} data={chartData} />;
            {/* Custom Text in the center of the doughnut */}
            <Text className="chart-text">
              Machines  <br /> {machines.length}
            </Text>
          </Box>

          {/* Legend */}
          <Stack className="legend">
            <Group className="legend-item">
              <Box className="circle green"></Box>
              <Text size="sm">{aliveMachines} Alive</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle red"></Box>
              <Text size="sm">{shutdownMachines} Shutdown</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle gray"></Box>
              <Text size="sm">{unknownMachines} Unknown</Text>
            </Group>
          </Stack>

          {/* Action Buttons */}
          <Stack className="button-group">
            <Button className="configure-button" onClick={() => setModalOpened(true)}>Configure Update</Button>
            <Button className="smart-button" onClick={() => navigate('/clustering')}>Smart Update</Button>
          </Stack>
        </Group>

        {/* Table Section */}
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
                  <Table.Th>Updated By</Table.Th> {/* New column */}
                  <Table.Th>Current Status</Table.Th>
                  <Table.Th>View</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {machines.map((machine) => (
                  <Table.Tr key={machine.machineId}>
                    <Table.Td>{machine.machineId}</Table.Td>
                    <Table.Td>{machine.ipAddress || "N/A"}</Table.Td>
                    <Table.Td>{"02/08/2024"}</Table.Td> {/* Hardcoded */}
                    <Table.Td>{"John Doe"}</Table.Td> {/* Hardcoded */}
                    <Table.Td>
                      <Badge
                        color = "green"
                        // color={machine.status === "Alive"
                        //   ? "green"
                        //   : machine.status === "Shutdown"
                        //   ? "red"
                        //   : "gray"}
                      >
                        {machine.status || "Alive"}
                      </Badge>
                    </Table.Td>
                    <Table.Td>
                      <Link 
                       to="/version-control"
                       state={{ machineId: machine.machineId }}>
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

      {/* Modal for Configure Update */}
      <Modal
        opened={modalOpened}
        onClose={() => setModalOpened(false)}
        title="Configure Update"
        centered
      >
        <Box>
          <Text>Select Machines*</Text>
          <Select
            data={machines.map(machine => `Machine ${machine.machineId}`)}
            placeholder="Select Machines"
          />
          <Text mt="md">Select Software Version*</Text>
          <Select
            data={["Version 2.5.0", "Version 1.8.2", "Version 3.1.0"]}
            placeholder="Select Version"
          />
          <Button mt="md" fullWidth>
            Deploy
          </Button>
        </Box>
      </Modal>

    </Stack>
  );
};
