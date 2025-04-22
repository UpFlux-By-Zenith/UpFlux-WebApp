import { useEffect, useMemo, useState } from "react";
import { ScatterChart, type ScatterChartSeries } from "@mantine/charts";
import {
  Box,
  Group,
  Loader,
  Title,
  Tooltip,
  useMantineTheme,
} from "@mantine/core";

type PlotPoint = {
  deviceUuid: string;
  x: number;
  y: number;
  clusterId: string;
  isSynthetic: boolean;
};

export default function ScatterRealtime() {
  const [points, setPoints] = useState<PlotPoint[]>([]);
  const theme = useMantineTheme();

  useEffect(() => {
    const ws = new WebSocket(`ws://localhost:5003/ws/ai`);
    ws.onmessage = (e) => {
      try {
        const { plotData } = JSON.parse(e.data);
        console.log("plotData", plotData);
        const normalised: PlotPoint[] = plotData.map((p: any) => ({
          deviceUuid: p.deviceUuid ?? p.DeviceUuid,
          x: p.x ?? p.X,
          y: p.y ?? p.Y,
          clusterId: p.clusterId ?? p.ClusterId,
          isSynthetic: p.isSynthetic ?? p.IsSynthetic ?? false,
        }));
        setPoints(normalised);
      } catch {
        console.error("bad JSON from /ws/ai");
      }
    };
    return () => ws.close();
  }, []);

  const series = useMemo<ScatterChartSeries[]>(() => {
    const buckets = new Map<string, PlotPoint[]>();
    points.forEach((p) => {
      if (!buckets.has(p.clusterId)) buckets.set(p.clusterId, []);
      buckets.get(p.clusterId)!.push(p);
    });

    // colors for clusters
    const baseColors = [
      theme.colors.red[7],
      theme.colors.orange[7],
      theme.colors.green[7],
      theme.colors.blue[7],
      theme.colors.violet[7],
      theme.colors.teal[7],
    ];

    // lighter version for synthetic - adding opacity
    const syntheticColors = [
      theme.colors.red[4],
      theme.colors.orange[4],
      theme.colors.green[4],
      theme.colors.blue[4],
      theme.colors.violet[4],
      theme.colors.teal[4],
    ];

    // preserve the clusterId order
    const clusterIds = Array.from(buckets.keys());

    return clusterIds.map((cid, i) => {
      const pts = buckets.get(cid)!;
      // pick by index, wrap around if more clusters than colours
      const idx = i % baseColors.length;
      const realColour = baseColors[idx];
      const synthColour = syntheticColors[idx];
      return {
        name: `Cluster${cid}`,
        color: realColour,
        data: pts.map((p) => ({
          x: p.x,
          y: p.y,
          uid: p.deviceUuid,
          syn: p.isSynthetic ? 1 : 0,
          cid,
          realColour,
          synthColour,
        })) as unknown as Record<string, number>[],
      };
    });
  }, [points, theme]);

  if (!points.length) return <Loader />;

  return (
    <Group p="md">
      <Title order={3}>UpFlux AI clusters (live)</Title>
      <Box style={{ width: "73%" }}>
        <ScatterChart
          h={520}
          withLegend
          dataKey={{ x: "x", y: "y" }}
          data={series}
          xAxisLabel="PC 1"
          yAxisLabel="PC 2"
          scatterProps={{
            shape: (props: any) => {
              const { cx, cy, payload } = props;
              const isSynthetic = payload.syn === 1;
              const r = isSynthetic ? 4 : 8;
              // pick the correct fill from payload
              const fill = isSynthetic
                ? (payload.synthColour as string)
                : (payload.realColour as string);

              return (
                <Tooltip
                  label={
                    <div style={{ fontSize: 12, lineHeight: 1.3 }}>
                      <strong>{payload.uid}</strong>
                      <br />
                      cluster {payload.cid}
                      <br />
                      {isSynthetic ? "synthetic" : "real"}
                      <br />({payload.x.toFixed(2)}, {payload.y.toFixed(2)})
                    </div>
                  }
                  withArrow
                  offset={6}
                  transitionProps={{ duration: 30 }}
                >
                  <circle
                    cx={cx}
                    cy={cy}
                    r={r}
                    fill={fill}
                    fillOpacity={isSynthetic ? 0.7 : 1}
                    stroke={isSynthetic ? fill : undefined}
                    strokeOpacity={isSynthetic ? 0.9 : 0}
                    strokeWidth={isSynthetic ? 1 : 0}
                  />
                </Tooltip>
              );
            },
          }}
        />
      </Box>
    </Group>
  );
}
