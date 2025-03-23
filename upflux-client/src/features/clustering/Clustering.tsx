import React, { useState, useEffect } from "react";
import { Box, Text, Group, Select, Switch } from "@mantine/core";
import { LineChart, ScatterChart } from "@mantine/charts"; // Import LineChart
import "@mantine/core/styles.css";
import "@mantine/charts/styles.css";
import "@mantine/dates/styles.css";

import { Calendar } from "@mantine/dates";
import dayjs from "dayjs"; // Import dayjs to compare and format dates
import "./clustering.css";
import { useSelector } from "react-redux";
import { data } from "react-router-dom";
import { RootState } from "../reduxSubscription/store";
import { IMachine } from "../../api/reponseTypes";
import { PLOT_COLORS } from "./clusteringConsts";

interface IPlotData {
  color: string;
  name: string;
  data: {
    x: number;
    y: number
  }[]
}

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
  const data = [
    {
      color: 'blue.5',
      name: 'Group 1',
      data: [
        { age: 25, BMI: 20 },
        { age: 30, BMI: 22 },
        { age: 35, BMI: 18 },
        { age: 40, BMI: 25 },
        { age: 45, BMI: 30 },
        { age: 28, BMI: 15 },
        { age: 22, BMI: 12 },
        { age: 50, BMI: 28 },
        { age: 32, BMI: 19 },
        { age: 48, BMI: 31 },
        { age: 26, BMI: 24 },
        { age: 38, BMI: 27 },
        { age: 42, BMI: 29 },
        { age: 29, BMI: 16 },
        { age: 34, BMI: 23 },
        { age: 44, BMI: 33 },
        { age: 23, BMI: 14 },
        { age: 37, BMI: 26 },
        { age: 49, BMI: 34 },
        { age: 27, BMI: 17 },
        { age: 41, BMI: 32 },
        { age: 31, BMI: 21 },
        { age: 46, BMI: 35 },
        { age: 24, BMI: 13 },
        { age: 33, BMI: 22 },
        { age: 39, BMI: 28 },
        { age: 47, BMI: 30 },
        { age: 36, BMI: 25 },
        { age: 43, BMI: 29 },
        { age: 21, BMI: 11 },
      ],
    },
    {
      color: 'red.5',
      name: 'Group 2',
      data: [
        { age: 26, BMI: 21 },
        { age: 31, BMI: 24 },
        { age: 37, BMI: 19 },
        { age: 42, BMI: 27 },
        { age: 29, BMI: 32 },
        { age: 35, BMI: 18 },
        { age: 40, BMI: 23 },
        { age: 45, BMI: 30 },
        { age: 27, BMI: 15 },
        { age: 33, BMI: 20 },
        { age: 38, BMI: 25 },
        { age: 43, BMI: 29 },
        { age: 30, BMI: 16 },
        { age: 36, BMI: 22 },
        { age: 41, BMI: 28 },
        { age: 46, BMI: 33 },
        { age: 28, BMI: 17 },
        { age: 34, BMI: 22 },
        { age: 39, BMI: 26 },
        { age: 44, BMI: 31 },
        { age: 32, BMI: 18 },
        { age: 38, BMI: 23 },
        { age: 43, BMI: 28 },
        { age: 48, BMI: 35 },
        { age: 25, BMI: 14 },
        { age: 31, BMI: 20 },
        { age: 36, BMI: 25 },
        { age: 41, BMI: 30 },
        { age: 29, BMI: 16 },
      ],
    },
  ];

  //Machine list from redux 
  const storedMachines = useSelector((root: RootState) => root.machines.messages)

  const [mappedClusterPlotData, setMappedClusterPlotData] = useState<IPlotData[]>([])

  useEffect(() => {
    const plotData: IPlotData[] = [];

    // Group machines by ClusterId
    const groupedMachines: Record<string, IMachine[]> = Object.values(storedMachines).reduce(
      (acc, machine) => {
        if (!acc[machine.clusterId]) {
          acc[machine.clusterId] = [];
        }
        acc[machine.clusterId].push(machine);
        return acc;
      },
      {} as Record<string, IMachine[]>
    );

    // Map grouped machines into plotData format
    const clusterIds = Object.keys(groupedMachines);
    clusterIds.forEach((clusterId, index) => {
      if (groupedMachines.hasOwnProperty(clusterId)) {
        const machines = groupedMachines[clusterId];
        const data: { x: number, y: number }[] = machines.map(machine => ({
          x: machine.x,
          y: machine.y
        }));

        // Cycle through colors based on the cluster index
        const color = PLOT_COLORS[index % PLOT_COLORS.length]; // Cycle through colors if there are more clusters than colors


        plotData.push({
          name: clusterId, // You can use ClusterId as the name
          data: data,
          color: color
        });
      }
    })


    setMappedClusterPlotData(plotData)
  }, [storedMachines]);

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
  };


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
        <ScatterChart
          w={800}
          h={600}
          data={mappedClusterPlotData}
          dataKey={{ x: 'x', y: 'y' }}
          xAxisLabel="Age"
          yAxisLabel="BMI"
          withLegend
        />
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
    </Box>
  );
};
