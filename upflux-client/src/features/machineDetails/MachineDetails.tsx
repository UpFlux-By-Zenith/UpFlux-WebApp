import React, { useState, useEffect } from "react";
import {
  Box,
  Button,
  Stack,
  Text,
  Select,
  Modal,
  SimpleGrid,
  Indicator
} from "@mantine/core";
import { useMediaQuery } from "@mantine/hooks";
import "./machineDetails.css";
import ReactSpeedometer from "react-d3-speedometer";
import { useDispatch, useSelector } from "react-redux";
import { useLocation } from "react-router-dom";
import { RootState } from "../reduxSubscription/store";
import { getMachineDetails } from "../../api/applicationsRequest";
import { guestLoginSubmit } from "../../api/loginRequests";
import { useSubscription } from "../reduxSubscription/useSubscription";

export const MachineDetails: React.FC = () => {
  const storedMachines = useSelector((root: RootState) => root.machines.messages);
  const machineMetrics = useSelector((state: RootState) => state.metrics.metrics);

  const [modalOpened, setModalOpened] = useState(false);
  const [selectedMachineName, setSelectedMachineName] = useState("");
  const [selectedMachineId, setSelectedMachineId] = useState("");
  const [appVersions, setAppVersions] = useState<{ appName: string; appVersion: string; lastUpdate: string }[]>([]);
  const [availableVersions, setAvailableVersions] = useState<string[]>([]);
  const [authToken, setAuthToken] = useState<string | null>(null);

  useEffect(() => {
    const getGuestToken = async () => {
      const token = await guestLoginSubmit();
      if (token) {
        sessionStorage.setItem('authToken', token);
        setAuthToken(token);
      }
      else{
        console.log("Failed to fetch guest token");
      }
    };

    getGuestToken();
  }, []);

  useSubscription(authToken);

  const location = useLocation();
  const isMobile = useMediaQuery("(max-width: 768px)");

  const fetchAvailableVersions = async () => {
    const machineDetails = await getMachineDetails();
  
    if (typeof machineDetails === "string" || machineDetails === null) {
      console.error("Error fetching machine details:", machineDetails);
      setAvailableVersions([]);
      return;
    }
  
    const matchingApps = machineDetails.applications.filter(
      (app) => app.machineId === selectedMachineId
    );
  
    // Flatten versions from all apps on this machine
    const versions: string[] = matchingApps.flatMap((app) =>
      app.versions.map((version) => version.versionName)
    );
  
    setAvailableVersions(versions);
  };
  
  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const machineId = params.get("machineId");
  
    if (machineId) {
      setSelectedMachineId(machineId);
  
      // Look for machine name from Redux store
      const matchedMachine = storedMachines[machineId];
      if (matchedMachine) {
        setSelectedMachineName(matchedMachine.machineName);
      } else {
        setSelectedMachineName("Unknown Machine");
      }
    }
  }, [location.search, storedMachines]);
  
  
  useEffect(() => {
    if (selectedMachineId) {
      fetchAvailableVersions();
    }
  }, [selectedMachineId]);

  const cpuColors = ["#00FF00", "#00FF00", "#00FF00", "#00FF00", "#00FF00", "#00FF00", "#00FF00", "#00FF00", "#33FF00", "#FFFF00", "#FFFF00", "#FF0000", "#FF0000"];
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

  const formatUptime = (seconds: number): string => {
    const days = Math.floor(seconds / (24 * 3600));
    const hours = Math.floor((seconds % (24 * 3600)) / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    return `${days}d ${hours}h ${minutes}m`;
  };

  const metrics = [
    {
      label: "CPU (%)",
      value: parseInt(machineMetrics[selectedMachineId]?.metrics.cpuUsage.toFixed()) || 0,
    },
    {
      label: "CPU Temp (°C)",
      value: parseInt(machineMetrics[selectedMachineId]?.metrics.cpuTemperature.toFixed()) || 0,
    },
    {
      label: "Memory Usage (%)",
      value: parseInt(machineMetrics[selectedMachineId]?.metrics.memoryUsage.toFixed()) || 0,
    },
    {
      label: "Disk Usage (%)",
      value: parseInt(machineMetrics[selectedMachineId]?.metrics.diskUsage.toFixed()) || 0,
    },
  ];

  return (
    <Stack className="version-control-content">
      <Box className="content-wrapper">
      <Box
  className="machine-id-box"
  style={{
    display: "flex",
    justifyContent: "center",
    width: "100%",
  }}
>
  {selectedMachineName ? (
    <Indicator

      color={storedMachines[selectedMachineId]?.isOnline ? "green" : "red"}
      label={storedMachines[selectedMachineId]?.isOnline ? "Online" : "Offline"}
      size={16}
    >
    <Text fw={700} style={{ textAlign: "center", fontSize: "1rem" }} size="lg">
    {selectedMachineName}
    </Text>
    <Text fw={700} style={{ textAlign: "center", fontSize: "1rem" }} size="md">
        ID: {selectedMachineId}
    </Text>
       {/* Display Current Version */}
       <Text fw={500} style={{ textAlign: "center" }} size="sm">
        Current Version: {storedMachines[selectedMachineId]?.currentVersion || "N/A"}
      </Text>
      <Text fw={500} style={{ textAlign: "center" }} size="sm">
        Available Versions: {availableVersions.length > 0 ? availableVersions.join(", ") : "No available versions"}
      </Text>
    </Indicator>
  ) : (
    <Text color="red" fw={700}>Invalid or Missing Machine ID</Text>
  )}
</Box>

        {/* Metrics Section */}
        <Box className="new-metrics-container">
          <SimpleGrid
            cols={isMobile ? 2 : 4}
          >
            {metrics.map((metric, index) => (
              <Box key={index} style={{ textAlign: "center", padding: "0.5rem" }}>
                <ReactSpeedometer
                  minValue={0}
                  maxValue={100}
                  segments={cpuColors.length}
                  segmentColors={
                    index === 0
                      ? cpuColors
                      : index === 1
                        ? tempColors
                        : index === 2
                          ? memoryColors
                          : diskColors
                  }
                  value={metric.value}
                  needleColor="black"
                  width={200}
                  height={130}
                  ringWidth={30}
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
              System Uptime: {formatUptime(machineMetrics[selectedMachineId]?.metrics.systemUptime || 0)}
            </h2>
          </center>
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
