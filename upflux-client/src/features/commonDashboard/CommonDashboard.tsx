import { Tabs } from '@mantine/core';
import "./common-dashboard.css";
import { Clustering } from '../clustering/Clustering';
import { UpdateManagement } from '../updateManagement/UpdateManagement';

export const CommonDashboard = () => {
    return <>
        <Tabs color="#2f3bff" className='common-tabs' defaultValue="main-management" >

            <Tabs.List>
                <Tabs.Tab value="main-management">Dashboard</Tabs.Tab>
                <Tabs.Tab value="ai">AI Clustering</Tabs.Tab>
                <Tabs.Tab value="gallery">Gallery</Tabs.Tab>
                <Tabs.Tab value="account">Account</Tabs.Tab>
            </Tabs.List>
            <Tabs.Panel value="main-management" pb="xs"><UpdateManagement /></Tabs.Panel>
            <Tabs.Panel value="ai" pb="xs"><Clustering /></Tabs.Panel>
            <Tabs.Panel value="gallery" pb="xs">Gallery panel</Tabs.Panel>
            <Tabs.Panel value="account" pb="xs">Account panel</Tabs.Panel>

        </Tabs>
    </>
}



