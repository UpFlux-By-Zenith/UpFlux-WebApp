import React, { useState, useEffect } from "react";
import { Box, Button, Group, Stack, Table, Text, Badge, Modal, Select, Title, Tabs } from "@mantine/core";
import { DonutChart } from "@mantine/charts";
import { useNavigate } from "react-router-dom";
import { getAccessibleMachines } from "../../api/accessMachinesRequest";
import "./gatewayDetails.css";
import { useSubscription } from "../reduxSubscription/useSubscription";
import { ROLES, useAuth } from "../../common/authProvider/AuthProvider";
import { IMachine } from "../../api/reponseTypes";
import { useDispatch, useSelector } from "react-redux";
import { updateMachine, updateMachineStatus } from "../reduxSubscription/machinesSlice";
import { RootState } from "../reduxSubscription/store";
import { getMachineStatus, getMachineStoredVersions } from "../../api/applicationsRequest";
import { IMachineStatus } from "../reduxSubscription/subscriptionConsts";
import { adminLogin } from "../../api/adminApiActions";

export const GatewayDetails = () => {
  const [rollbackModalOpened, setRollbackModalOpened] = useState(false);
  const [machines, setMachines] = useState<IMachine[]>([]);
  const [loading, setLoading] = useState(true);
  const [updateModal, setUpdateModal] = useState<boolean>(false)
   const [authToken, setAuthToken] = useState<string | null>(null);
  
    const ADMIN_EMAIL = process.env.REACT_APP_ADMIN_EMAIL!;
    const ADMIN_PASSWORD = process.env.REACT_APP_ADMIN_PASSWORD!;
  
  
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
            sessionStorage.setItem("authToken", response.token);
            setAuthToken(response.token);
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
    
  //Machine list from redux 
  const storedMachines = useSelector((root: RootState) => root.machines.messages)

  const dispatch = useDispatch();

  useSubscription(authToken);

  //Fetch accessible machines on component load
  useEffect(() => {

    const fetchMachines = async () => {
      const result = await getAccessibleMachines();

      if (typeof result === "object" && result?.accessibleMachines) {
        setMachines(result.accessibleMachines as IMachine[]);
        console.log(machines)

      } else {
        console.error("Failed to fetch machines:", result);
        setMachines([]); // Clear machines in case of failure
      }
      setLoading(false);
    };
    fetchMachines();
  }, []);


  const fetchMachineStatus = async () => {
    const res: IMachineStatus[] = await getMachineStatus()

    if (!res || !Array.isArray(res)) {
      console.error("Machine status data is empty or invalid:", res);
      return; // exit early if data is invalid
    }

    res.forEach((m: any) => dispatch(updateMachineStatus({
      DeviceUuid: m.machineId,
      IsOnline: m.isOnline,
      LastSeen: {
        Seconds: 0,
        Nanos: 0
      }
    })))
  }

  useEffect(() => {

    machines.forEach(m => dispatch(updateMachine(m)))
    fetchMachineStatus()
  }, [machines])

  // Calculate machine statuses
  const machineValues = Object.values(storedMachines);

  const aliveCount = machineValues.filter((machine) => machine.isOnline).length;
  const shutdownCount = machineValues.filter((machine) => !machine.isOnline).length;

  const chartData = [
    { name: "Online", value: aliveCount, color: "#40C057" },
    { name: "Offline", value: shutdownCount, color: "#FA5252" },
    { name: "Unknown", value: machineValues.length === 0 ? 1 : 0, color: "#6c757d" },
  ];

  const getStatusBadge = (isOnline) => {
    if (isOnline) return <Badge color="green">Online</Badge>;
    return <Badge color="red">Offline</Badge>;
  };

  return (

      <Box className="content-wrapper">
        <Group className="overview-section">
          <Box className="chart">
            <DonutChart className="chart" size={180} thickness={14} withTooltip={false} data={chartData} />
            <Text className="chart-text">
              <br /> QC Machines <br /> {machines.length}
            </Text>
          </Box>

          <Stack className="legend">
            <Group className="legend-item">
              <Box className="circle green"></Box>
              <Text size="sm">{aliveCount} Online</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle red"></Box>
              <Text size="sm">{shutdownCount} Offline</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle gray"></Box>
              <Text size="sm">{machineValues.length === 0 ? 1 : 0} Unknown</Text>
            </Group>
          </Stack>
        </Group>
      </Box>
  );
};
