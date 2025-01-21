import { Footer } from "../footer/Footer";
import { Navbar } from "../navbar/Navbar";
import { GetEngineerToken } from "./GetEngineerToken";
import { PackageFileInput } from "./PackageFileInput";
import "./admin-dashboard.css";
import { Stack, Text } from "@mantine/core"; // Ensure this import matches your grid library
import { Grid } from "@mantine/core";
import "@mantine/core/styles/Grid.css";
export const AdminDashboard = () => {
  return (
    <>
      <Grid grow className="admin-dashboard">
        <Grid.Col span={4}>
          <GetEngineerToken />
        </Grid.Col>
        <Grid.Col span={4}>
          <PackageFileInput />
        </Grid.Col>
        <Grid.Col span={4}>
          <Stack align="center" className="form-stack">
            <Text className="form-title">Manage Engineers</Text>
          </Stack>
        </Grid.Col>
        <Grid.Col span={4}>
          <Stack align="center" className="form-stack">
            <Text className="form-title">Manage Machines</Text>
          </Stack>
        </Grid.Col>
        <Grid.Col span={4}>
          <Stack align="center" className="form-stack">
            <Text className="form-title">View Logs</Text>
          </Stack>
        </Grid.Col>
      </Grid>
    </>
  );
};
