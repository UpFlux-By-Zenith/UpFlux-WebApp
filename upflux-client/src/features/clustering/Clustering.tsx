import React from "react";
import { Box, Text, Group, Select, Switch } from "@mantine/core";
import { LineChart } from "@mantine/charts"; // Import LineChart
import "@mantine/core/styles.css";
import "@mantine/charts/styles.css";
import "@mantine/dates/styles.css";

import { Calendar } from "@mantine/dates";
import "./clustering.css";
import cluster1 from "../../assets/images/cluster1.png";
import cluster2 from "../../assets/images/cluster2.png";

export const Clustering: React.FC = () => {
  // Updated data with 24-hour time format and 'value' instead of 'Apples'
  const dataPoints = [
    { time: "00:00", value: 110 },
    { time: "03:00", value: 60 },
    { time: "06:00", value: 80 },
    { time: "09:00", value: null },
    { time: "12:00", value: null },
    { time: "15:00", value: 40 },
    { time: "18:00", value: 110 },
    { time: "21:00", value: null },
  ];

  return (
    <Box className="clustering-container">
      {/* Dropdown Section */}
      <Group align="center" className="dropdown-section">
        <Select
          data={["Cluster State", "Cluster Details"]}
          defaultValue="Cluster State"
          rightSection={null}
          className="dropdown"
        />
        <Select
          data={["Update State", "Update Logs"]}
          defaultValue="Update State"
          className="dropdown"
        />
        <Switch label="Real Time" size="md" className="real-time-switch" />
      </Group>

      <Group align="flex-start" className="main-content">
        {/* Cluster Visuals */}
        <Box className="cluster-visuals">
          {/* Small Cluster */}
          <Box className="cluster-circle small-circle">
            <img src={cluster1} alt="Small Cluster" className="cluster-icon" />
          </Box>
          {/* Large Cluster */}
          <Box className="cluster-circle large-circle">
            <img src={cluster2} alt="Large Cluster" className="cluster-icon" />
          </Box>
        </Box>

        {/* Calendar */}
        <Box className="calendar-wrapper">
      {/* Calendar with custom styles */}
      <Calendar
        className="calendar"
        styles={{
            calendarHeader: {
            marginLeft: '100px', // Adjust month margin
            },
          day: {
            width: '60px',  // Increase width of each day cell
            height: '60px', // Increase height of each day cell
          },
          weekday: {
            fontSize: '16px', // Adjust weekday font size
          },
        }}
      />
    </Box>
      </Group>

      {/* Network Upload Section */}
      <Box className="network-upload">
        <Text size="sm">Network Upload</Text>
        <Text size="sm" className="data-label"
    >
      Data
    </Text>
        <LineChart
      h={180}
      data={dataPoints}
      dataKey="time"
      series={[{ name: 'value', color: 'indigo.6' }]}
      curveType="linear"
      connectNulls
    />
        {/* Axis Labels */}
        <Group align="apart">
          <Text size="sm" className="time-label">Time</Text>
        </Group>
      </Box>
    </Box>
  );
};
