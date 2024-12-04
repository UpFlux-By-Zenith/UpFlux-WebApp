import React from "react";
import { Box, Button, Group, Stack, Table, Text, Badge } from "@mantine/core";
import { Doughnut } from "react-chartjs-2";
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend,
} from "chart.js";
import "./updateManagement.css";
import view from "../../assets/images/view.png";

ChartJS.register(ArcElement, Tooltip, Legend);

export const UpdateManagement: React.FC = () => {
  // Hardcoded data for the table
  const machines = [
    { id: "001", lastUpdate: "02/08/2024", status: "Alive" },
    { id: "002", lastUpdate: "02/08/2024", status: "Alive" },
    { id: "003", lastUpdate: "02/08/2024", status: "Alive" },
    { id: "004", lastUpdate: "02/08/2024", status: "Alive" },
    { id: "005", lastUpdate: "02/08/2024", status: "Alive" },
    { id: "006", lastUpdate: "03/08/2024", status: "Shutdown" },
    { id: "007", lastUpdate: "10/09/2022", status: "Unknown" },
  ];

  // Helper to check if a machine is "up to date" (within last 6 months)
  const isUpToDate = (dateString: string): boolean => {
    // Parse the date string manually
    const [day, month, year] = dateString.split('/').map(Number);
    const lastUpdate = new Date(year, month - 1, day); // month is 0-indexed
    const today = new Date();
    
    // Subtract 6 months from the current date
    const sixMonthsAgo = new Date(today.getFullYear(), today.getMonth() - 6, today.getDate());
  
    return lastUpdate >= sixMonthsAgo;
  };
  

  // Count updated and pending machines
  const updatedMachines = machines.filter((machine) =>
    isUpToDate(machine.lastUpdate)
  ).length;
  const pendingMachines = machines.length - updatedMachines;

  // Chart data
  const chartData = {
    labels: ["Updated", "Pending Updates"],
    datasets: [
      {
        data: [updatedMachines, pendingMachines],
        backgroundColor: ["#007bff", "#ff0000"], // Blue and Red colors
        hoverBackgroundColor: ["#0056b3", "#b30000"],
      },
    ],
  };

  // Chart options
const chartOptions = {
    cutout: '60%',
    plugins: {
      legend: {
        display: false, // Disable the default legend
      },
    },
  };

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
            <Doughnut data={chartData} options={chartOptions}/>
            {/* Custom Text in the center of the doughnut */}
            <Text className="chart-text">
              Machines <br /> <br /> {machines.length}
            </Text>
          </Box>

          {/* Legend */}
          <Stack className="legend">
            <Group className="legend-item">
              <Box className="circle blue"></Box>
              <Text size="sm">{updatedMachines} Updated</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle red"></Box>
              <Text size="sm">{pendingMachines} Pending Updates</Text>
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
