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
  Tabs
} from "@mantine/core";
import { Link, useNavigate } from "react-router-dom";
import { useLocation } from "react-router-dom";
import "./versionControl.css";
import ReactSpeedometer, { Transition } from "react-d3-speedometer";
import { getSpeedometerProps } from './speedometerUtils';
import { getMachineDetails } from "../../api/applicationsRequest";
import { useSelector } from "react-redux";
import { RootState } from "../reduxSubscription/store";
import { IApplications } from "../reduxSubscription/applicationVersions";

export const VersionControl: React.FC = () => {
  const navigate = useNavigate();

  const cpuColors = [
    "#00FF00", // Green
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00", 
    "#33FF00", 
    "#FFFF00", // Yellow 
    "#FFFF00",
    "#FF0000", 
    "#FF0000", // Red 
  ];
  
  const tempColors = [
    "#00FF00", // Green
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00", 
    "#33FF00", 
    "#FFFF00", // Yellow 
    "#FFFF00",
    "#FF0000", 
    "#FF0000", // Red 
  ];

  const memoryColors = [
    "#00FF00", // Green
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#33FF00", 
    "#FFFF00",
    "#FFFF00", // Yellow 
    "#FFFF00", 
    "#FF0000", // Red 
  ];

  const diskColors = [
    "#00FF00", // Green
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00",
    "#00FF00", 
    "#33FF00", 
    "#FFFF00", // Yellow 
    "#FFFF00",
    "#FF0000", 
    "#FF0000", // Red 
  ];

  //  // Hardcoded machine metrics data
  //   const machineMetrics = {
  //     "M01": {
  //       metrics: {
  //         cpuUsage: 45,
  //         cpuTemperature: 60,
  //         memoryUsage: 70,
  //         diskUsage: 30,
  //         systemUptime: 123456, // Uptime in seconds
  //       },
  //     },
  //   };
  
  //   // Hardcoded applications data
  //   const applications = {
  //     "M01": {
  //       VersionNames: ["1.0.0", "1.1.0", "1.2.0"],
  //     },
  //   };
  
  //   // Hardcoded app versions data
  //   const appVersions = [
  //     {
  //       appName: "UpFlux-Monitoring-Service",
  //       appVersion: "1.0.0",
  //       lastUpdate: "Jan 10 2024",
  //     },
  //     {
  //       appName: "UpFlux-Monitoring-Service",
  //       appVersion: "1.1.0",
  //       lastUpdate: "Feb 15 2024",
  //     },
  //     {
  //       appName: "UpFlux-Monitoring-Service",
  //       appVersion: "1.2.0",
  //       lastUpdate: "Mar 20 2024",
  //     },
  //   ];

  // State for Modal visibility
  const [modalOpened, setModalOpened] = useState(false);
  const applications: Record<string, IApplications> = useSelector((state: RootState) => state.applications.messages)
  const machineMetrics = useSelector(
    (state: RootState) => state.metrics.metrics
  );
  // Retrieve machine ID from the route state
  const location = useLocation();
  const machineId = "M01"; // Hardcoded to M01 for demonstration

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

  return (
    <Stack className="version-control-content">
      {/* Header */}

              {/* Tabs Section */}
              <Tabs defaultValue="applications" className="custom-tabs">
              <Tabs.List>
                <Tabs.Tab value="dashboard" className="custom-tab" onClick={() => navigate("/update-management")}>
                  Dashboard
                </Tabs.Tab>
                <Tabs.Tab value="applications" className="custom-tab">
                  Applications
                </Tabs.Tab>
              </Tabs.List>
            </Tabs>

      <Box className="content-wrapper">
      <Box className="machine-id-box">
        <Select
          data={Object.keys(machineMetrics)} 
          value={machineId}
          onChange={(value) => navigate(".", { state: { machineId: value } })}
          placeholder="Select Machine"
        />
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
              <Box key={index} style={{ textAlign: "center" }}>
                <ReactSpeedometer
                  minValue={0}
                  maxValue={100}
                  segments={cpuColors.length} 
                  segmentColors={
                    index % 4 === 0
                      ? cpuColors    
                      : index % 4 === 1
                      ? tempColors   
                      : index % 4 === 2
                      ? memoryColors  
                      : diskColors 
                  }
                  value={metric.value}
                  needleColor="black"
                  width={250}
                  height={150}
                  ringWidth={30}
                  currentValueText={""}
                  forceRender={true}
                  maxSegmentLabels={4}
                />
                <Text className="metrics-labels" size="sm" fw="bold" mt="sm">
                  {metric.label}
                </Text>
              </Box>
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