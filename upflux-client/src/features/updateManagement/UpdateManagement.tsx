import React, { useState, useEffect } from "react";
import { Box, Button, Group, Stack, Table, Text, Badge, Modal, Select, Title, Tabs } from "@mantine/core";
import { DonutChart } from "@mantine/charts";
import { Link, useNavigate } from "react-router-dom";
import { getAccessibleMachines } from "../../api/accessMachinesRequest";
import "./update-management.css";
import view from "../../assets/images/view.png";
import updateIcon from "../../assets/images/updateIcon.jpg";
import { useSubscription } from "../reduxSubscription/useSubscription";
import { useAuth } from "../../common/authProvider/AuthProvider";
import { deployPackage, doRollback, getRunningMachinesApplications } from "../../api/applicationsRequest";
import { notifications } from "@mantine/notifications";
import { IApplications } from "../reduxSubscription/applicationVersions";
import { useSelector } from "react-redux";
import { RootState } from "../reduxSubscription/store";
import { ConfigureUpdate } from "./ConfigureUpdate";

export const UpdateManagement = () => {
  const [modalOpened, setModalOpened] = useState(false);
  const [machines, setMachines] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedMachine, setSelectedMachine] = useState<string | null>(null);
  const [selectedVersion, setSelectedVersion] = useState<string | null>(null);
  const navigate = useNavigate();
  const [updateModal, setUpdateModal] = useState<boolean>(false)
  const applications: Record<string, IApplications> = useSelector((state: RootState) => state.applications.messages)
  const { authToken } = useAuth();


  useSubscription(authToken);

    // Hardcoded machine data
    // const hardcodedMachines = [
    //   {
    //     machineId: "c3589340-db6b-11ef-8615-2ccf677985c6",
    //     machineName: "M01",
    //     ipAddress: "192.168.1.1",
    //     status: "Alive",
    //     applications: [
    //       { name: "UpFlux-Monitoring-Service", versions: ["1.0.0", "1.1.0"] }
    //     ]
    //   },
    //   {
    //     machineId: "cfb393ec-db6b-11ef-9067-2ccf677985c6",
    //     machineName: "M02",
    //     ipAddress: "192.168.1.2",
    //     status: "Alive",
    //     applications: [
    //       { name: "UpFlux-Monitoring-Service", versions: ["1.0.0", "1.1.0"] }
    //     ]
    //   },
    //   {
    //     machineId: "d3b0580e-db6b-11ef-bd3e-2ccf677985c6",
    //     machineName: "M03",
    //     ipAddress: "192.168.1.3",
    //     status: "Alive",
    //     applications: [
    //       { name: "UpFlux-Monitoring-Service", versions: ["1.0.0", "1.1.0"] }
    //     ]
    //   }
    // ];

  //Fetch accessible machines on component load
  useEffect(() => {
    const fetchMachines = async () => {
      const result = await getAccessibleMachines();
      if (typeof result === "object" && result?.accessibleMachines?.result) {
        setMachines(result.accessibleMachines.result);
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

    getRunningMachines()
    fetchMachines();
  }, []);

  // Handle Deploy Button Click
  const handleRollback = () => {

    doRollback(selectedVersion, selectedMachine)

  };

  // Chart data for multiple measures
  const chartData = [
    { name: "Alive", value: machines.length, color: "#40C057" },
    { name: "Shutdown", value: 0, color: "#FA5252" },
    { name: "Unknown", value: 0, color: "#6c757d" },
  ];
  const [selectedApp, setSelectedApp] = useState<string | null>(null);
  const [availableApps, setAvailableApps] = useState<any[]>([]);
  const [availableVersions, setAvailableVersions] = useState<any[]>([]);

  // Simulate an API call to fetch the application list and versions
  const fetchPackages = async () => {
    try {
      const response = await fetch('http://localhost:5000/api/PackageManagement/packages', {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${sessionStorage.getItem('authToken')}`, // Get the token from localStorage (or sessionStorage)
        },
      });
      const data = await response.json();
      setAvailableApps(data); // Set available apps and versions in state
    } catch (error) {
      console.error('Error fetching packages:', error);
      setAvailableApps([]); // Clear available apps on error
    }
  };

  //Fetch data from the API
  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch(
          "http://localhost:5000/api/DataRequest/engineer/engineer-applications", {
          method: 'GET',
          headers: {
            'Authorization': `Bearer ${sessionStorage.getItem('authToken')}`, // Get the token from localStorage (or sessionStorage)
          },
        }
        );
        const data = await response.json();
        setMachines(data); // Set the machines data
      } catch (error) {
        console.error("Error fetching data:", error);
      }
    };

    // fetchData();
  }, []);

  useEffect(() => {
    // Fetch available applications and their versions on modal open
    if (modalOpened) {
      fetchPackages();
    }
  }, [modalOpened]);

  // Fetch data from the API
  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch(
          "http://localhost:5000/api/DataRequest/engineer/engineer-applications"
        );
        const data = await response.json();
        setMachines(data); // Set the machines data
      } catch (error) {
        console.error("Error fetching data:", error);
      }
    };

    fetchData();
  }, []);

  // Handle machine selection
  const handleMachineChange = (value: string | null) => {
    setSelectedMachine(value);
    if (value) {
      // Find the selected machine and set its applications
      const selectedMachineData = machines?.find(
        (machine) => machine.machineId === value
      );
      if (selectedMachineData) {
        setAvailableApps(selectedMachineData.applications);
      } else {
        setAvailableApps([]);
      }
    } else {
      setAvailableApps([]);
    }
    setSelectedApp(null); // Reset selected app
    setAvailableVersions([]); // Reset available versions
  };

  const machineIds = machines.map(m => m.machineId)

  // Handle application selection
  const handleAppChange = (value: string | null) => {
    setSelectedApp(value);
    if (value) {
      // Find the selected application and set its versions
      const selectedAppData = true
      if (selectedAppData) {
        setAvailableVersions(applications[selectedMachine].VersionNames);
      } else {
        setAvailableVersions([]);
      }
    } else {
      setAvailableVersions([]);
    }
  };

  return (

    <Stack className="update-management-content">
        {/* Tabs Section */}
        <Tabs defaultValue="dashboard" className="custom-tabs">
        <Tabs.List>
          <Tabs.Tab value="dashboard" className="custom-tab">
            Dashboard
          </Tabs.Tab>
          <Tabs.Tab value="applications" className="custom-tab" onClick={() => navigate("/version-control")}>
            Applications
          </Tabs.Tab>
        </Tabs.List>
      </Tabs>

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
              <Text size="sm">{3} Alive</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle red"></Box>
              <Text size="sm">{0} Shutdown</Text>
            </Group>
            <Group className="legend-item">
              <Box className="circle gray"></Box>
              <Text size="sm">{0} Unknown</Text>
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
              onClick={() => setModalOpened(true)}
            >
              Configure Rollback
            </Button>
            <Button
              color="rgba(0, 3, 255, 1)"
              className="smart-button"
              onClick={() => navigate("/clustering")}
            >
              Smart Update
            </Button>
          </Stack>
        </Group>

        <Box>
          {loading ? (
            <Text>Loading machines...</Text>
          ) : (
            <Table className="machine-table" highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Machine Name</Table.Th>
                  <Table.Th>Machine ID</Table.Th>
                  <Table.Th>IP Address</Table.Th>
                  <Table.Th>Current Version</Table.Th>
                  <Table.Th>Last Update</Table.Th>
                  <Table.Th>Updated By</Table.Th>
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
                    <Table.Td>{machine.applications[0].versions[0]}</Table.Td>
                    <Table.Td>{"02/08/2024"}</Table.Td>
                    <Table.Td>{"John Doe"}</Table.Td>
                    <Table.Td>
                      <Badge color="green">{machine.status || "Alive"}</Badge>
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          )}
        </Box>
      </Box>
      <ConfigureUpdate setModalOpened={setUpdateModal} modalOpened={updateModal} machineIds={machineIds} />
      <Modal
        opened={modalOpened}
        onClose={() => setModalOpened(false)}
        title="Configure Rollback"
        centered
      >
        <Box>
          <Text>Select Machines*</Text>
          <Select
            data={Object.keys(applications).map((machineid) => ({
              value: machineid,
              label: machineid, // Use machineName if available, otherwise use machineId
            }))}
            placeholder="Select Machines"
            onChange={handleMachineChange}
          />

          {selectedMachine && (
            <>
              <Text mt="md">Select Application*</Text>
              <Select
                data={[{
                  value: "UpFlux-Monitoring-Service",
                  label: "UpFlux-Monitoring-Service",
                }]}
                placeholder="Select Application"
                onChange={handleAppChange}
              />
            </>
          )}

          {selectedApp && (
            <>
              <Text mt="md">Select Software Version*</Text>
              <Select
                data={availableVersions.map((version) => ({
                  value: version,
                  label: `${version} (on device)`,
                }))}
                placeholder="Select Version"
                onChange={(value) => setSelectedVersion(value || null)}
              />
            </>
          )}

          <Button
            color="rgba(0, 3, 255, 1)"
            mt="md"
            fullWidth
            onClick={handleRollback}
          >
            Deploy
          </Button>
        </Box>
      </Modal>
    </Stack>
  );
};
