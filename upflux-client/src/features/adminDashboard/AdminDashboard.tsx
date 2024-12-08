import { Footer } from "../footer/Footer";
import { Navbar } from "../navbar/Navbar";
import { GetEngineerToken } from "./GetEngineerToken";
import { PackageFileInput } from "./PackageFileInput";
import "./admin-dashboard.css";
import { Stack, Text } from "@mantine/core"; // Ensure this import matches your grid library

export const AdminDashboard = () => {
  // ! mantine Grid not working
  return (
    <>
      <Navbar onHomePage={false} />
      <div className="admin-parent">
        <div className="div1">
          <GetEngineerToken />
        </div>
        <div className="div2">
          <PackageFileInput />
        </div>
        <div className="div3">
          <Stack align="center" className="form-stack">
            <Text className="form-title">Manage Engineers</Text>
          </Stack>
        </div>
        <div className="div4">
          <Stack align="center" className="form-stack">
            <Text className="form-title">Manage Machines</Text>
          </Stack>
        </div>
        <div className="div5">
          <Stack align="center" className="form-stack">
            <Text className="form-title">View Logs</Text>
          </Stack>
        </div>
      </div>
      <Footer />
    </>
  );
};
