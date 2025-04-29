import React, { useState, useEffect, useMemo } from "react";
import { Box, Text, Group, Select, Paper, Button, ActionIcon, Tooltip } from "@mantine/core";
import { ChartTooltipProps, ScatterChart } from "@mantine/charts";
import "@mantine/core/styles.css";
import "@mantine/charts/styles.css";
import "@mantine/dates/styles.css";

import { DateTimePicker } from "@mantine/dates";
import "./clustering.css";
import { useSelector } from "react-redux";
import { RootState } from "../reduxSubscription/store";
import { IMachine } from "../../api/reponseTypes";
import { PLOT_COLORS } from "./clusteringConsts";
import { IconAi, IconArrowBigDownLinesFilled, IconBulbFilled } from "@tabler/icons-react";
import { IPackagesOnCloud, getAvailablePackages } from "../../api/applicationsRequest";
import { notifications } from "@mantine/notifications";

interface IPlotData {
  color: string;
  name: string;
  machineId: string[];
  data: {
    x: number;
    y: number;
    machineId: any;
  }[];
}

interface PlotRecommendation {
  x?: number;
  y?: number;
  clusterId?: string;
}


export const Clustering: React.FC = () => {
  const storedMachines = useSelector((root: RootState) => root.machines.messages);
  const syntheticMachines = useSelector((root: RootState) => root.machines.syntheticMachines)

  const clusterRecommendation = useSelector((root: RootState) => root.clusterRecommendation);

  const [mappedClusterPlotData, setMappedClusterPlotData] = useState<IPlotData[]>([]);
  const [selectedCluster, setSelectedCluster] = useState<string | null>(null);
  const [selectedDateTime, setSelectedDateTime] = useState<Date | null>(null);

  const [selectedVersion, setSelectionVersion] = useState<string>("")
  const [availableApps, setAvailableApps] = useState<IPackagesOnCloud[]>([])

  useEffect(() => {

    getAvailablePackages().then(res => {
      setAvailableApps(res as IPackagesOnCloud[])
    })

  }, [])

  useEffect(() => {
    const plotData: IPlotData[] = [];
    let clusterIndex = 0; // Track cluster color manually
    const clusterColorMap: Record<string, { normal: string; light: string }> = {};
    // --- Process real machines ---
    const groupedMachines: Record<string, IMachine[]> = Object.values(storedMachines).reduce(
      (acc, machine) => {
        if (machine && machine.clusterId) {
          if (!acc[machine.clusterId]) {
            acc[machine.clusterId] = [];
          }
          acc[machine.clusterId].push(machine);
        }
        return acc;
      },
      {} as Record<string, IMachine[]>
    );

    // --- Process real machines ---
    Object.entries(groupedMachines).forEach(([clusterId, machines]) => {
      const validMachines = machines.filter(m => m.x !== undefined && m.y !== undefined);

      if (validMachines.length > 0) {
        const color = PLOT_COLORS[clusterIndex % PLOT_COLORS.length];
        clusterColorMap[clusterId] = color; // Save mapping real clusterId -> color

        plotData.push({
          name: clusterId,
          data: validMachines.map(machine => ({
            x: machine.x!,
            y: machine.y!,
            machineId: machine.machineId
          })),
          machineId: validMachines.map(m => m.machineId),
          color: color.normal
        });

        clusterIndex++;
      }
    });

    // --- Process synthetic machines ---
    const syntheticEntries = Object.entries(syntheticMachines);
    if (syntheticEntries.length > 0) {
      const syntheticClustered = syntheticEntries.reduce((acc, [deviceId, machine]) => {
        if (machine && machine.clusterId) {
          if (!acc[machine.clusterId]) {
            acc[machine.clusterId] = [];
          }
          acc[machine.clusterId].push({ deviceId, ...machine });
        }
        return acc;
      }, {} as Record<string, (PlotRecommendation & { deviceId: string })[]>);

      // --- Process synthetic machines ---
      Object.entries(syntheticClustered).forEach(([clusterId, machines]) => {
        const validMachines = machines.filter(m => m.x !== undefined && m.y !== undefined);

        if (validMachines.length > 0) {
          const baseColor = clusterColorMap[clusterId] || PLOT_COLORS[clusterIndex % PLOT_COLORS.length];

          plotData.push({
            name: `Synthetic ${clusterId}`,
            data: validMachines.map(machine => ({
              x: machine.x!,
              y: machine.y!,
              machineId: machine.deviceId
            })),
            machineId: validMachines.map(m => m.deviceId),
            color: baseColor.light // use lighter color for synthetic
          });

          clusterIndex++;
        }
      });
    }
    setMappedClusterPlotData(plotData);
  }, [storedMachines, syntheticMachines]);



  const [recommendedTime, setRecommendedTime] = useState<Date | null>(null);

  useEffect(() => {
    if (!selectedCluster) {
      setRecommendedTime(null);
      return;
    }

    const recommendation = clusterRecommendation.find((rec) => rec.ClusterId === selectedCluster);

    if (!recommendation) {
      setRecommendedTime(null);
      return;
    }

    const { Seconds, Nanos } = recommendation.UpdateTime;
    setRecommendedTime(new Date(Seconds * 1000 + Math.floor(Nanos / 1e6))); // Convert to milliseconds
  }, [selectedCluster, clusterRecommendation]);


  const handleDeploy = async () => {

    const authToken = sessionStorage.getItem('authToken');


    // Check if required fields are selected
    if (!selectedCluster || !selectedDateTime || !selectedVersion) {
      console.error("Please select all required fields.");
      return;
    }

    const targetDevices = mappedClusterPlotData
      .find((data) => data.name === selectedCluster)
      ?.machineId;

    if (!targetDevices || targetDevices.length === 0) {
      console.error("No devices found for the selected cluster.");
      return;
    }

    const requestBody = {
      name: "upflux-monitoring-service", // Assuming this is static; if dynamic, replace accordingly
      version: selectedVersion,
      targetDevices: targetDevices,
      startTimeUtc: selectedDateTime.toISOString(), // Convert to ISO format
    };

    try {
      const response = await fetch("http://localhost:5000/api/PackageManagement/packages/schedule-update", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "Authorization": `Bearer ${authToken}`, // Ensure you add the authorization token
        },
        body: JSON.stringify(requestBody),
      }).then(() => {

        notifications.show({
          title: "Web Service",
          position: "top-right",
          autoClose: 10000,
          message: "Update has been scheduled"
        })
        setSelectedCluster(null)
        setSelectionVersion(null)
        setSelectedDateTime(null)

      })

    } catch (error) {
      console.error("Error during deploy request:", error);
    }
  };


  const ChartTooltip = ({ payload }: ChartTooltipProps) => {
    if (!payload || !payload.length) return null;

    const data = payload[0]?.payload;
    if (!data) return null;

    return (
      <Paper px="md" py="sm" withBorder shadow="md" radius="md">
        <Text fz="sm" fw={500}>
          {data.name}
        </Text>
        <Text fz="xs" c="dimmed">
          ID: {data.machineId}
        </Text>
        <Text fz="xs">
          ( {data.x.toFixed(2)}, {data.y.toFixed(2)} )
        </Text>
      </Paper>
    );
  };

  return (
    <Box className="clustering-container">
      <Group align="flex-start" className="main-content">
        <div>
          <ScatterChart
            w={800}
            h={600}
            data={mappedClusterPlotData}
            tooltipProps={{
              content: ({ payload }) => <ChartTooltip payload={payload} />,
            }}
            dataKey={{ x: 'x', y: 'y' }}
            withLegend
            xAxisLabel="PC 1"
            yAxisLabel="PC 2"
          />
          <Text
            size="xs"
            mt="sm"
            c="dimmed"
            ta="center"
          >
            * Some data points (labelled "Synthetic") have been added for demonstration purposes.
          </Text>
        </div>

        <Box className="calendar-wrapper" title="Schedule Update" mt="md" style={{ display: 'flex', flexDirection: 'column', gap: "20px" }}>
          <Text className="form-title">Update Scheduling </Text>

          <Select
            label="Select Cluster*"
            placeholder="Choose a cluster"
            data={mappedClusterPlotData
              .filter(plot => !plot.name.startsWith("Synthetic"))
              .map(plot => ({ value: plot.name, label: plot.name }))}
            value={selectedCluster}
            onChange={(value) => setSelectedCluster(value)}
          />

          {selectedCluster && <Text></Text>}
          <Select
            label="Select Application*"

            data={[{
              value: "upflux-monitoring-service",
              label: "UpFlux-Monitoring-Service",
            }]}
            placeholder="Select Application"
            value="upflux-monitoring-service"
            disabled
          />
          <Select
            label="Select Version*"
            data={availableApps[0]?.versions.map((version) => ({
              value: version,
              label: version,
            }))}
            placeholder="Select Version"
            onChange={(value) => setSelectionVersion(value)}
          />


          {recommendedTime && (

            <Group mt="xs" align="center">
              <Tooltip label="Suggested by AI" position="top" withArrow>
                <Paper withBorder p="xs" style={{ display: "flex", alignItems: "center", gap: "10px" }}>
                  {/* Lightbulb Icon */}
                  <IconBulbFilled size={20} />
                  <Text c="blue" fz="md" style={{ fontWeight: 500 }}>
                    Suggested Update Time: {recommendedTime.toLocaleString()}
                  </Text>

                  <ActionIcon
                    color="rgba(0, 3, 255, 1)"
                    aria-label="Settings"
                    onClick={() => setSelectedDateTime(recommendedTime)}
                    variant="light"
                  >
                    <IconArrowBigDownLinesFilled style={{ width: "70%", height: "70%" }} stroke={1.5} />
                  </ActionIcon>
                </Paper>
              </Tooltip>
            </Group>
          )}

          <DateTimePicker
            label="Select Date & Time*"
            value={selectedDateTime}
            onChange={setSelectedDateTime}
          />

          <Button disabled={!selectedVersion && !selectedDateTime && !selectedCluster} color="rgba(0, 3, 255, 1)" onClick={handleDeploy}>Schedule Update</Button>
        </Box>
      </Group>
    </Box>
  );
};
