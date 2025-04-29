import React, { useState, useEffect } from "react";
import {
  Box,
  Stack,
  Text,
  SimpleGrid,
  Indicator
} from "@mantine/core";
import { useMediaQuery } from "@mantine/hooks";
import "./machineDetails.css";
import ReactSpeedometer from "react-d3-speedometer";
import { useDispatch, useSelector } from "react-redux";
import { useLocation } from "react-router-dom";
import { RootState } from "../reduxSubscription/store";
import { getMachineStoredVersions } from "../../api/applicationsRequest";
import { getAccessibleMachines } from "../../api/accessMachinesRequest";
import { useSubscription } from "../reduxSubscription/useSubscription";
import { adminLogin } from "../../api/adminApiActions";
import { updateMachine } from "../reduxSubscription/machinesSlice";
import { ROLES, useAuth } from "../../common/authProvider/AuthProvider";
import { IMachine } from "../../api/reponseTypes";

export const MachineDetails: React.FC = () => {
  const [modalOpened, setModalOpened] = useState(false);
  const [selectedMachineName, setSelectedMachineName] = useState("");
  const [selectedMachineId, setSelectedMachineId] = useState("");
  const [availableVersions, setAvailableVersions] = useState<string[]>([]);
  const [authToken, setAuthToken] = useState<string | null>(null);

  const { login } = useAuth();
  const ADMIN_EMAIL = process.env.REACT_APP_ADMIN_EMAIL!;
  const ADMIN_PASSWORD = process.env.REACT_APP_ADMIN_PASSWORD!;
  
  const storedMachines = useSelector((root: RootState) => root.machines.messages);
  const machineMetrics = useSelector((state: RootState) => state.metrics.metrics);
  const dispatch = useDispatch();
  const location = useLocation();
  const isMobile = useMediaQuery("(max-width: 768px)");

  // 🛠 Fetch machines and dispatch to Redux
  const fetchMachines = async () => {
    const result = await getAccessibleMachines();
    if (typeof result === "object" && result?.accessibleMachines) {
      (result.accessibleMachines as IMachine[]).forEach((m) => dispatch(updateMachine(m)));
    } else {
      console.error("Failed to fetch machines:", result);
    }
  };

  useEffect(() => {
    const getAdminToken = async () => {
      try {
        const response = await adminLogin({
          email: ADMIN_EMAIL,
          password: ADMIN_PASSWORD,
        });

        if (response.error) {
          console.error("Admin login failed:", response.error);
          return;
        }

        if (response.token) {
          console.log("Admin token:", response.token);
          sessionStorage.setItem("authToken", response.token);
          setAuthToken(response.token);
          login(ROLES.ADMIN, response.token);
        }
      } catch (error: any) {
        if (error.response) {
          console.error(error.response.data?.message || "Admin login failed.");
        } else if (error.request) {
          console.error("Network error. Please check your connection.");
        } else {
          console.error("Unexpected error during admin login.");
        }
      }
    };

    getAdminToken();
  }, []);

  // 🛠 Subscribe to updates after login
  useSubscription(authToken);

  // 🛠 Fetch machines once we have the token
  useEffect(() => {
    if (authToken) {
      fetchMachines();
    }
  }, [authToken]);

  // 🛠 Handle machine ID from URL params
  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const machineId = params.get("machineId");

    if (machineId) {
      setSelectedMachineId(machineId);

      // Find machine name from Redux store
      const matchedMachine = storedMachines[machineId];
      if (matchedMachine) {
        setSelectedMachineName(matchedMachine.machineName);
      } else {
        setSelectedMachineName(machineId); // fallback
      }
    }
  }, [location.search, storedMachines]);

  // 🛠 Fetch available versions when machine selected
  useEffect(() => {
    const fetchAvailableVersions = async () => {
      const machineDetails = await getMachineStoredVersions();

      const matchingApps = machineDetails.filter(
        (app) => app.machineId === selectedMachineId
      );

      const versions: string[] = matchingApps.map((app) => app.versionName);
      setAvailableVersions(versions);
    };

    if (selectedMachineId) {
      fetchAvailableVersions();
    }
  }, [selectedMachineId]);

  const formatUptime = (seconds: number): string => {
    const days = Math.floor(seconds / (24 * 3600));
    const hours = Math.floor((seconds % (24 * 3600)) / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    return `${days}d ${hours}h ${minutes}m`;
  };

  const metrics = [
    {
      label: "CPU (%)",
      value: parseInt(machineMetrics[selectedMachineId]?.metrics.cpuUsage?.toFixed() || "0"),
    },
    {
      label: "CPU Temp (°C)",
      value: parseInt(machineMetrics[selectedMachineId]?.metrics.cpuTemperature?.toFixed() || "0"),
    },
    {
      label: "Memory Usage (%)",
      value: parseInt(machineMetrics[selectedMachineId]?.metrics.memoryUsage?.toFixed() || "0"),
    },
    {
      label: "Disk Usage (%)",
      value: parseInt(machineMetrics[selectedMachineId]?.metrics.diskUsage?.toFixed() || "0"),
    },
  ];

  const cpuColors = ["#00FF00", "#00FF00", "#00FF00", "#33FF00", "#FFFF00", "#FFFF00", "#FF0000"];
  const tempColors = [...cpuColors];
  const memoryColors = [...cpuColors];
  const diskColors = [...cpuColors];

  return (
    <Stack className="version-control-content">
      <Box className="content-wrapper">
        <Box
          className="machine-id-box"
          style={{ display: "flex", justifyContent: "center", width: "100%" }}
        >
          {selectedMachineName ? (
            <Indicator
              color={storedMachines[selectedMachineId]?.isOnline ? "green" : "red"}
              label={storedMachines[selectedMachineId]?.isOnline ? "Online" : "Offline"}
              size={16}
            >
              <Text fw={700} style={{ textAlign: "center" }} size="lg">
                {selectedMachineName}
              </Text>
              <Text fw={700} style={{ textAlign: "center" }} size="md">
                ID: {selectedMachineId}
              </Text>
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
          <SimpleGrid cols={isMobile ? 2 : 4}>
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
    </Stack>
  );
};
