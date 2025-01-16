import React, { useState } from "react";
import { Box, Button, Group, Stack, Table, Text, Badge, Modal, Select } from "@mantine/core";
import { DonutChart } from '@mantine/charts';
import { Link, useNavigate } from "react-router-dom";
import "./update-management.css";
import view from "../../assets/images/view.png";

export const UpdateManagement: React.FC = () => {
  const [modalOpened, setModalOpened] = useState(false);
  const navigate = useNavigate();

  // Hardcoded data for the table with actual names in "Updated By"
  const machines = [
    { id: "001", ipAddress: "192.168.1.1", lastUpdate: "02/08/2024", status: "Alive", updatedBy: "John Doe" },
    { id: "002", ipAddress: "192.168.1.2", lastUpdate: "02/08/2024", status: "Alive", updatedBy: "Jane Smith" },
    { id: "003", ipAddress: "192.168.1.3", lastUpdate: "02/08/2024", status: "Alive", updatedBy: "Michael Johnson" },
    { id: "004", ipAddress: "192.168.1.4", lastUpdate: "02/08/2024", status: "Alive", updatedBy: "Emily Davis" },
    { id: "005", ipAddress: "192.168.1.5", lastUpdate: "02/08/2024", status: "Alive", updatedBy: "David Lee" },
    { id: "006", ipAddress: "192.168.1.6", lastUpdate: "03/08/2024", status: "Shutdown", updatedBy: "Chris Martin" },
    { id: "007", ipAddress: "192.168.1.7", lastUpdate: "10/09/2022", status: "Unknown", updatedBy: "Jessica Wang" },
  ];

  // Count machines based on their status
  const aliveMachines = machines.filter((machine) => machine.status === "Alive").length;
  const shutdownMachines = machines.filter((machine) => machine.status === "Shutdown").length;
  const unknownMachines = machines.filter((machine) => machine.status === "Unknown").length;

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
                <Table.Tr key={machine.id}>
                  <Table.Td>{machine.id}</Table.Td>
                  <Table.Td>{machine.ipAddress}</Table.Td>
                  <Table.Td>{machine.lastUpdate}</Table.Td>
                  <Table.Td>{machine.updatedBy}</Table.Td> {/* Updated column with actual names */}
                  <Table.Td>
                    <Badge
                      color={machine.status === "Alive"
                        ? "green"
                        : machine.status === "Shutdown"
                        ? "red"
                        : "gray"}
                    >
                      {machine.status}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    <Link 
                     to="/version-control"
                     state={{ machineId: machine.id }}>
                      <img src={view} alt="view" className="view" />
                    </Link>
                  </Table.Td>
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
          <Text>Select Machines*</Text>
          <Select
            data={["Machine 001", "Machine 002", "Machine 003"]}
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
