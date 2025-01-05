import React from "react";
import { Box, Button, Group, Stack, Table, Text, Badge, Select } from "@mantine/core";
import "./clusterManagement.css";
import "@mantine/core/styles.css";
import "@mantine/charts/styles.css";
import "@mantine/dates/styles.css";


export const ClusterManagement: React.FC = () => {

  return (
    <Stack className="cluster-management-content">
      {/* Header */}
      <Box className="header">
        <Text size="xl" fw={700}>
          Cluster Management
        </Text>
      </Box>

      <Box className="cluster-content-wrapper">

      <Text className="chart-text">
              Cluster 001
      </Text>

      <Select
          data={["Update Status", "No of Devices", "Memory", "Network Status", "Suggested time to update"]}
          defaultValue="Cluster State"
          rightSection={null}
          className="dropdown"
        />

        {/* Action Buttons */}
          <Stack className="button-group">
             <Button className="configure-button">Simulate</Button>
             <Button className="smart-button">Update Now</Button>
          </Stack>
       
      </Box>
    </Stack>
  );
};
