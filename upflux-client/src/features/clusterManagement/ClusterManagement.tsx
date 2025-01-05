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

      <Stack className="select-group">
      <Text className="cluster-text" style={{fontWeight: "bold"}}>
              Cluster 001
      </Text>

      <Select
          data={["Update Status", "No of Devices", "Memory", "Network Status", "Suggested time to update"]}
          placeholder="Selection"
          rightSection={null}
          className="dropdown"
        />

      </Stack>

        {/* Action Buttons */}
          <Stack className="cluster-button-group">
             <Button className="simulate-button">Simulate</Button>
             <Button className="update-button">Update Now</Button>
          </Stack>
       
      </Box>
    </Stack>
  );
};
