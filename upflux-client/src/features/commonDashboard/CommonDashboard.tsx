import { Tabs } from '@mantine/core';
import "./common-dashboard.css";
import { Clustering } from '../clustering/Clustering';
import { UpdateManagement } from '../updateManagement/UpdateManagement';
import { ManageMachines } from '../adminDashboard/ManageMachines';
import { ViewEngineers } from '../adminDashboard/ViewEngineers';
import { DownloadLogs } from '../adminDashboard/DownloadLogs';
import { UploadedApplications } from '../adminDashboard/UploadedApplications';
import { PackageFileInput } from '../adminDashboard/PackageFileInput';
import { GetEngineerToken } from '../adminDashboard/GetEngineerToken';
import { VersionControl } from '../versionControl/VersionControl';
import { AccountSettings } from '../accountSettings/AccountSettings';
import { ROLES, useAuth } from '../../common/authProvider/AuthProvider';

export const CommonDashboard = () => {
    const { userRole } = useAuth();

    return <>
        <Tabs color="#2f3bff" className='common-tabs' defaultValue="main-management" >
            <Tabs.List>
                <Tabs.Tab value="main-management">Dashboard</Tabs.Tab>
                <Tabs.Tab value="version">Version Control</Tabs.Tab>
                <Tabs.Tab value="ai">AI Scheduling</Tabs.Tab>
                {userRole === ROLES.ADMIN && <>
                    <Tabs.Tab value="license">License Management</Tabs.Tab>
                    <Tabs.Tab value="engineer">Engineer Management</Tabs.Tab>
                    <Tabs.Tab value="package">Package Management</Tabs.Tab>
                </>
                }
                <Tabs.Tab value="logs">View Logs</Tabs.Tab>
                <Tabs.Tab value="account">Account</Tabs.Tab>
            </Tabs.List>
            <Tabs.Panel value="main-management" pb="xs"><UpdateManagement /></Tabs.Panel>
            <Tabs.Panel value="version" pb="xs"><VersionControl /></Tabs.Panel>
            <Tabs.Panel value="ai" pb="xs"><Clustering /></Tabs.Panel>
            <Tabs.Panel value="package" pb="xs">
                <PackageFileInput />
                <UploadedApplications />
            </Tabs.Panel>
            <Tabs.Panel value="logs" pb="xs"><DownloadLogs /></Tabs.Panel>
            <Tabs.Panel value="account" pb="xs"><AccountSettings /></Tabs.Panel>
            <Tabs.Panel value="engineer" pb="xs"><GetEngineerToken /><ViewEngineers /></Tabs.Panel>
            <Tabs.Panel value="license" pb="xs"><ManageMachines /></Tabs.Panel>

        </Tabs>
    </>
}



