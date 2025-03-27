import { Grid } from "@mantine/core";
import { ViewEngineers } from './ViewEngineers';
import { ManageMachines } from './ManageMachines';
import { MachineApplicationDetails } from './MachineApplicationDetails';
import { PackageFileInput } from "./PackageFileInput";
import { DownloadLogs } from './DownloadLogs';
import { GetEngineerToken } from "./GetEngineerToken";
import { UploadedApplications } from './UploadedApplications';
import "./admin-dashboard.css";

export const AdminDashboard = () => {
  return (
    <Grid className="admin-dashboard" grow>
      {/* Left Column - Tables (70%) */}
      <Grid.Col span={7} className="left-column">
        <Grid>
          <Grid.Col span={12}>
            <MachineApplicationDetails />
          </Grid.Col>
          <Grid.Col span={12}>
            <ManageMachines />
          </Grid.Col>
          <Grid.Col span={12}>
            <ViewEngineers />
          </Grid.Col>
        </Grid>
      </Grid.Col>

      <Grid.Col span={3} className="right-column">
        <Grid>
          {/* Upload & Logs in a Row (Each takes 50%) */}
          <Grid.Col span={6}>
            <PackageFileInput />
          </Grid.Col>
          <Grid.Col span={6}>
            <DownloadLogs />
          </Grid.Col>

          {/* Other Components Below */}
          <Grid.Col span={12}>
          </Grid.Col>
          <Grid.Col span={12}>
            <UploadedApplications />
          </Grid.Col>
        </Grid>
      </Grid.Col>
    </Grid>
  );
};
