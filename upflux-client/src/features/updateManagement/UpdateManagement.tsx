import React from "react";
import { Box, Button, Group, Stack, Table, Text, Avatar, Badge } from "@mantine/core";
import "./updateManagement.css";
import view from "../../assets/images/view.png";
import scheduling from "../../assets/images/scheduling.jpg";

export const UpdateManagement: React.FC = () => {
  // Hardcoded data for the table
  const machines = [
    { id: "001", lastUpdate: "02/08/2024", status: "Alive" },
    { id: "002", lastUpdate: "02/08/2024", status: "Alive" },
    { id: "003", lastUpdate: "02/08/2024", status: "Alive" },
    { id: "004", lastUpdate: "02/08/2024", status: "Alive" },
    { id: "005", lastUpdate: "02/08/2024", status: "Alive" },
    { id: "006", lastUpdate: "03/08/2024", status: "Shutdown" },
    { id: "007", lastUpdate: "10/09/2024", status: "Unknown" },
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
        {/* Chart and Legend */}
          <img
            src={scheduling}
            alt="Chart"
          />
          <Stack className="legend">
            <Group className="legend-item">
              <Box className="circle blue"></Box>
              <Text size="sm">300 Updated</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle red"></Box>
              <Text size="sm">50 Pending Updates</Text>
            </Group>
          </Stack>

        {/* Action Buttons */}
        <Stack className="button-group">
          <Button className="configure-button">Configure Update</Button>
          <Button className="smart-button">Smart Update</Button>
        </Stack>
      </Group>

      {/* Table Section */}
      <Box>
        <Table>
          <thead>
            <tr>
              <th>Machine ID</th>
              <th>Last Update</th>
              <th>Current Status</th>
              <th>View</th>
            </tr>
          </thead>
          <tbody>
            {machines.map((machine) => (
              <tr key={machine.id}>
                <td>{machine.id}</td>
                <td>{machine.lastUpdate}</td>
                <td>
                  <Badge
                    color={
                      machine.status === "Alive"
                        ? "green"
                        : machine.status === "Shutdown"
                        ? "red"
                        : "gray"
                    }
                  >
                    {machine.status}
                  </Badge>
                </td>
                <td>
                  <img src={view} alt="view" className="view" />
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      </Box>
      </Box>
    </Stack>
  );
};
