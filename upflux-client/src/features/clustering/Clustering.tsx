import React, { useState, useEffect, useMemo } from "react";
import { Box, Text, Group, Select, Paper, Button, ActionIcon } from "@mantine/core";
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
import { IconArrowBigDownLinesFilled } from "@tabler/icons-react";

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


  const handleDeploy = () => {
    console.log("Deploying cluster:", selectedCluster, "at time:", selectedDateTime);
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

        <Box className="calendar-wrapper" title="Schedule Update" mt="md" style={{ display: 'flex', flexDirection: 'column' }}>
          <Select
            label="Select Cluster"
            placeholder="Choose a cluster"
            data={mappedClusterPlotData.map((plot) => ({ value: plot.name, label: plot.name }))}
            value={selectedCluster}
            onChange={(value) => setSelectedCluster(value)}

          />

          {recommendedTime && (
            <Group mt="xs">
              <Text c="blue">
                Suggested Update Time: {recommendedTime.toLocaleString()}
              </Text>

              <ActionIcon color="rgba(0, 3, 255, 1)" aria-label="Settings" onClick={() => setSelectedDateTime(recommendedTime)}>
                <IconArrowBigDownLinesFilled style={{ width: "70%", height: "70%" }} stroke={1.5} />
              </ActionIcon>
            </Group>
          )}

          <DateTimePicker
            label="Select Date & Time"
            value={selectedDateTime}
            onChange={setSelectedDateTime}
          />

          <Button color="rgba(0, 3, 255, 1)" onClick={handleDeploy}>Deploy</Button>
        </Box>
      </Group>
    </Box>
  );
};
