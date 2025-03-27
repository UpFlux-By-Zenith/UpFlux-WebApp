import React, { useState, useEffect } from "react";
import { Box, Button, Group, Stack, Table, Text, Badge, Modal, Select, Title, Tabs } from "@mantine/core";
import { DonutChart } from "@mantine/charts";
import { useNavigate } from "react-router-dom";
import { getAccessibleMachines } from "../../api/accessMachinesRequest";
import "./update-management.css";
import { useSubscription } from "../reduxSubscription/useSubscription";
import { useAuth } from "../../common/authProvider/AuthProvider";
import { ConfigureUpdate } from "./ConfigureUpdate";
import { IMachine } from "../../api/reponseTypes";
import { ConfigureRollback } from "./ConfigureRollback";
import { useDispatch, useSelector } from "react-redux";
import { updateMachine, updateMachineStatus } from "../reduxSubscription/machinesSlice";
import { RootState } from "../reduxSubscription/store";
import { getMachineStatus, getMachineStoredVersions } from "../../api/applicationsRequest";
import { IMachineStatus } from "../reduxSubscription/subscriptionConsts";

export const UpdateManagement = () => {
  const [rollbackModalOpened, setRollbackModalOpened] = useState(false);
  const [machines, setMachines] = useState<IMachine[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const [updateModal, setUpdateModal] = useState<boolean>(false)
  const { authToken } = useAuth();

  //Machine list from redux 
  const storedMachines = useSelector((root: RootState) => root.machines.messages)

  const machineIds = machines.map(m => m.machineId)
  const dispatch = useDispatch();

  useSubscription(authToken);

  //Fetch accessible machines on component load
  useEffect(() => {

    const fetchMachines = async () => {
      const result = await getAccessibleMachines();
      const res = await getMachineStoredVersions();

      if (typeof result === "object" && result?.accessibleMachines?.result) {
        setMachines(result.accessibleMachines.result as IMachine[]);

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

    console.log(res)
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

    <Stack className="update-management-content">

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

          <Stack className="button-group">
            <Button
              color="rgba(0, 3, 255, 1)"
              className="configure-button"
              onClick={() => setUpdateModal(true)}
            >
              Configure Update
            </Button>
            <Button
              color="rgba(0, 3, 255, 1)"
              className="configure-button"
              onClick={() => setRollbackModalOpened(true)}
            >
              Configure Rollback
            </Button>
          </Stack>
        </Group>

        <Box>
          {loading ? (
            <Text>Loading QC machines...</Text>
          ) : (
            <Table className="machine-table" highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>QC Machine Name</Table.Th>
                  <Table.Th>QC Machine ID</Table.Th>
                  {/* <Table.Th>IP Address</Table.Th> */}
                  <Table.Th>Application Name</Table.Th>
                  <Table.Th>Application Version</Table.Th>
                  <Table.Th>Machine Added On</Table.Th>
                  <Table.Th>Current Status</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {Object.values(storedMachines)?.map((machine) => (
                  <Table.Tr
                    key={machine.machineId}
                    // onClick={() => navigate("/version-control", { state: { machineId: machine.machineId } })}
                    style={{ cursor: "pointer" }}
                  >
                    <Table.Td>{machine.machineName}</Table.Td>
                    <Table.Td>{machine.machineId}</Table.Td>
                    {/* <Table.Td>{machine.ipAddress || "N/A"}</Table.Td> */}
                    <Table.Td>{machine.appName}</Table.Td>
                    <Table.Td>{machine.currentVersion}</Table.Td>
                    <Table.Td>{machine.dateAddedOn}</Table.Td>
                    <Table.Td>{getStatusBadge(machine.isOnline)}</Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          )}
        </Box>
      </Box>
      <ConfigureUpdate setModalOpened={setUpdateModal} modalOpened={updateModal} machineIds={machineIds} />
      <ConfigureRollback setRollbackModalOpened={setRollbackModalOpened} rollbackModalOpened={rollbackModalOpened} machines={machines} />

    </Stack>
  );
};
