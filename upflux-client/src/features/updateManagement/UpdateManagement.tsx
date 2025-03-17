import React, { useState, useEffect } from "react";
import { Box, Button, Group, Stack, Table, Text, Badge, Modal, Select, Title, Tabs } from "@mantine/core";
import { DonutChart } from "@mantine/charts";
import { useNavigate } from "react-router-dom";
import { getAccessibleMachines } from "../../api/accessMachinesRequest";
import "./update-management.css";
import { useSubscription } from "../reduxSubscription/useSubscription";
import { useAuth } from "../../common/authProvider/AuthProvider";
import { getRunningMachinesApplications } from "../../api/applicationsRequest";
import { ConfigureUpdate } from "./ConfigureUpdate";
import { IMachine } from "../../api/reponseTypes";
import { ConfigureRollback } from "./ConfigureRollback";

export const UpdateManagement = () => {
  const [rollbackModalOpened, setRollbackModalOpened] = useState(false);
  const [machines, setMachines] = useState<IMachine[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const [updateModal, setUpdateModal] = useState<boolean>(false)
  const { authToken } = useAuth();


  useSubscription(authToken);

  //Fetch accessible machines on component load
  useEffect(() => {
    const fetchMachines = async () => {
      const result = await getAccessibleMachines();
      if (typeof result === "object" && result?.accessibleMachines?.result) {
        setMachines(result.accessibleMachines.result as IMachine[]);
      } else {
        console.error("Failed to fetch machines:", result);
        setMachines([]); // Clear machines in case of failure
      }
      setLoading(false);
    };

    const getRunningMachines = async () => {
      await getRunningMachinesApplications().then(res => {
        console.log(res)
      })
    }

    // getRunningMachines()
    fetchMachines();
  }, []);


  // Chart data for multiple measures
  const chartData = [
    { name: "Alive", value: machines.length, color: "#40C057" },
    { name: "Shutdown", value: 0, color: "#FA5252" },
    { name: "Unknown", value: 0, color: "#6c757d" },
  ];


  const machineIds = machines.map(m => m.machineId)

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
              <Text size="sm">{0} Alive</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle red"></Box>
              <Text size="sm">{0} Shutdown</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle gray"></Box>
              <Text size="sm">{machines.length} Unknown</Text>
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
                  <Table.Th>IP Address</Table.Th>
                  <Table.Th>Application Name</Table.Th>
                  <Table.Th>Application Version</Table.Th>
                  <Table.Th>Machine Added On</Table.Th>
                  <Table.Th>Current Status</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {machines?.map((machine) => (
                  <Table.Tr
                    key={machine.machineId}
                    onClick={() => navigate("/version-control", { state: { machineId: machine.machineId } })}
                    style={{ cursor: "pointer" }}
                  >
                    <Table.Td>{machine.machineName}</Table.Td>
                    <Table.Td>{machine.machineId}</Table.Td>
                    <Table.Td>{machine.ipAddress || "N/A"}</Table.Td>
                    <Table.Td>{machine.appName}</Table.Td>
                    <Table.Td>{machine.currentVersion}</Table.Td>
                    <Table.Td>{machine.dateAddedOn}</Table.Td>
                    <Table.Td>
                      <Badge color="green">{"Alive"}</Badge>
                    </Table.Td>
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
