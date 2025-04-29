import { ChartTooltipProps, ScatterChart } from "@mantine/charts";
import { Paper, Text } from "@mantine/core";
import { useState, useEffect } from "react";
import { useSelector } from "react-redux";
import { IMachine } from "../../api/reponseTypes";
import { RootState } from "../reduxSubscription/store";
import { PLOT_COLORS } from "./clusteringConsts";

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


export const ClusteringChart = () => {
    const storedMachines = useSelector((root: RootState) => root.machines.messages);
    const syntheticMachines = useSelector((root: RootState) => root.machines.syntheticMachines)
    const [mappedClusterPlotData, setMappedClusterPlotData] = useState<IPlotData[]>([]);
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

    return <div>
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
}