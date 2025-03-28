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

interface IPlotData {
  color: string;
  name: string;
  machineId: string[];
  data: {
    x: number;
    y: number;
  }[];
}

export const Clustering: React.FC = () => {
  const storedMachines = useSelector((root: RootState) => root.machines.messages);
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

    Object.entries(groupedMachines).forEach(([clusterId, machines], index) => {
      const validMachines = machines.filter(m => m.x !== undefined && m.y !== undefined);

      if (validMachines.length > 0) {
        plotData.push({
          name: clusterId,
          data: validMachines.map(machine => ({
            x: machine.x!,
            y: machine.y!
          })),
          machineId: validMachines.map(m => m.machineId),
          color: PLOT_COLORS[index % PLOT_COLORS.length]
        });
      }
    });

    setMappedClusterPlotData(plotData);
  }, [storedMachines]);

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
      });

      if (response.ok) {
        const result = await response.json();
        console.log("Deployment scheduled successfully:", result);

        setSelectedCluster("")
        setSelectionVersion("")
        setSelectedDateTime(null)

      } else {
        const errorData = await response.json();
        console.error("Error scheduling update:", errorData);
      }
    } catch (error) {
      console.error("Error during deploy request:", error);
    }
  };


  const ChartTooltip = ({ payload }: ChartTooltipProps) => {
    if (!payload) return null;
    return (
      <Paper px="md" py="sm" withBorder shadow="md" radius="md">
        <Text fz="sm">
          {payload["0"]?.payload.name}
          {payload["0"]?.payload.machineId}
        </Text>
      </Paper>
    );
  };

  return (
    <Box className="clustering-container">
      <Group align="flex-start" className="main-content">
        <ScatterChart
          w={800}
          h={600}
          data={mappedClusterPlotData}
          tooltipProps={{
            content: ({ payload }) => <ChartTooltip payload={payload} />,
          }}
          dataKey={{ x: 'x', y: 'y' }}
          withLegend
        />
        <Box className="calendar-wrapper" title="Schedule Update" mt="md" style={{ display: 'flex', flexDirection: 'column', gap: "20px" }}>
          <Text className="form-title">Update Scheduling </Text>

          <Select
            label="Select Cluster*"
            placeholder="Choose a cluster"
            data={mappedClusterPlotData.map((plot) => ({ value: plot.name, label: plot.name }))}
            value={selectedCluster}
            onChange={(value) => setSelectedCluster(value)}

          />
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
