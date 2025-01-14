import React, { useState, useEffect } from "react";
import { Box, Text, Group, Select, Switch } from "@mantine/core";
import { LineChart } from "@mantine/charts"; // Import LineChart
import "@mantine/core/styles.css";
import "@mantine/charts/styles.css";
import "@mantine/dates/styles.css";

import { Calendar } from "@mantine/dates";
import dayjs from "dayjs"; // Import dayjs to compare and format dates
import "./clustering.css";
import cluster1 from "../../assets/images/cluster1.png";
import cluster2 from "../../assets/images/cluster2.png";

export const Clustering: React.FC = () => {
  // Updated data with 24-hour time format and 'value' instead of 'Apples'
  const dataPoints = {
    "Dec 10, 2024": [
        { time: "00:00", value: 0 },
        { time: "03:00", value: 0 },
        { time: "06:00", value: 80 },
        { time: "09:00", value: null },
        { time: "12:00", value: null },
        { time: "15:00", value: 40 },
        { time: "18:00", value: 110 },
        { time: "21:00", value: null },
      ],
      "Dec 9, 2024": [
        { time: "00:00", value: 120 },
        { time: "03:00", value: 65 },
        { time: "06:00", value: 90 },
        { time: "09:00", value: 40 },
        { time: "12:00", value: 75 },
        { time: "15:00", value: 50 },
        { time: "18:00", value: 130 },
        { time: "21:00", value: null },
      ],
      "Dec 6, 2024": [
          { time: "00:00", value: 0 },
          { time: "03:00", value: 0 },
          { time: "06:00", value: 0 },
          { time: "09:00", value: 40 },
          { time: "12:00", value: 60 },
          { time: "15:00", value: 90 },
          { time: "18:00", value: 0 },
          { time: "21:00", value: null },
        ],
  };

  // State to store the selected date
  const [selectedDate, setSelectedDate] = useState<string | null>(null);

  // Function to format the date to match the format used in dataPoints (e.g., 'Mar 22')
  const formatDate = (date: Date) => {
    return date.toLocaleDateString("en-US", { year: "numeric", month: "short", day: "numeric" });
  };

  // Function to handle date selection and update chart data
  const handleSelect = (date: Date) => {
    const formattedDate = formatDate(date);
    setSelectedDate(formattedDate); // Update the selected date in state
    console.log("Selected date:", formattedDate); 
  };

  // Get data for the selected date or use default
  const chartData = selectedDate ? dataPoints[selectedDate] : [];

  useEffect(() => {
    // Set the calendar to the current date initially
    const today = new Date();
    const formattedDate = formatDate(today);
    setSelectedDate(formattedDate);
  }, []);

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
            highlightToday
            getDayProps={(date) => ({
              selected: dayjs(date).isSame(selectedDate, 'date'), // Compare selected date using dayjs
              onClick: () => handleSelect(date), // Capture the date selection
            })}
          />
        </Box>
      </Group>

      {/* Network Upload Section */}
      <Box className="network-upload">
        <Text size="sm">Network Upload</Text>
        <Text size="sm" className="data-label">
          Data
        </Text>
        <LineChart
          h={180}
          data={chartData} // Use filtered data based on selected date
          dataKey="time"
          series={[{ name: "value", color: "indigo.6" }]}
          curveType="linear"
          connectNulls
        />
        {/* Axis Labels */}
        <Group align="apart">
          <Text size="sm" className="time-label">
            Time
          </Text>
        </Group>
      </Box>
    </Box>
  );
};
