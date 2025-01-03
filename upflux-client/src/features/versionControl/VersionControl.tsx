import React from "react";
import { Box, Button, Group, Stack, Table, Text, Badge, Select } from "@mantine/core";
import { DonutChart } from '@mantine/charts';
import "./versionControl.css";
import view from "../../assets/images/view.png";
import "@mantine/core/styles.css";
import "@mantine/charts/styles.css";
import "@mantine/dates/styles.css";


export const VersionControl: React.FC = () => {
  // Hardcoded data for the table
  const appVersions = [
    { appVersion: "2.5.0", lastUpdate: "06/06/2024", engineer: "Peter McDonald", timestamp: "10:30 AM", status: "Success" },
    { appVersion: "2.0.0", lastUpdate: "01/01/2024", engineer: "Dean Farrell", timestamp: "12:30 PM", status: "Success" },
    { appVersion: "1.6.0", lastUpdate: "06/06/2023", engineer: "David Purcell", timestamp: "08:45 AM", status: "Failed" },
    { appVersion: "1.5.0", lastUpdate: "01/01/2023", engineer: "John Doe", timestamp: "09:00 AM", status: "Success" },
    { appVersion: "1.4.0", lastUpdate: "06/06/2022", engineer: "Martin Murphy", timestamp: "02:00 PM", status: "Success" },
  ];
  

  return (
    <Stack className="update-management-content">
      {/* Header */}
      <Box className="header">
        <Text size="xl" fw={700}>
          Version Control
        </Text>
      </Box>

      <Box className="content-wrapper">

      <Box className="machine-id-box">
          <Text>Machine 001</Text>
        </Box>

        {/* Overview Section */}
        <Group className="overview-section">

          {/* Action Buttons */}
          <Select
          data={["Cluster State", "Cluster Details"]}
          defaultValue="Cluster State"
          rightSection={null}
          className="softwareDropdown"
        />
        </Group>

        {/* Table Section */}
        <Box>
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>App Version</Table.Th>
                <Table.Th>Last Update</Table.Th>
                <Table.Th>Engineer</Table.Th>
                <Table.Th>Timestamp</Table.Th>
                <Table.Th>Status</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {appVersions.map((appVersion) => (
                <Table.Tr key={appVersion.appVersion}>
                  <Table.Td>{appVersion.appVersion}</Table.Td>
                  <Table.Td>{appVersion.lastUpdate}</Table.Td>
                  <Table.Td>{appVersion.engineer}</Table.Td>
                  <Table.Td>{appVersion.timestamp}</Table.Td>
                    <Table.Td>
                        <Badge
                        color={appVersion.status === "Success" ? "green" : "red"}
                        >
                        {appVersion.status}
                        </Badge>
                    </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Box>
      </Box>
    </Stack>
  );
};