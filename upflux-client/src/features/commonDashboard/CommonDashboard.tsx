import { Tabs } from '@mantine/core';
import "./common-dashboard.css";

export const CommonDashboard = () => {
    return <>
        <Tabs defaultValue="chat" className="custom-tabs">

            <Tabs.List>
                <Tabs.Tab className="custom-tab" value="chat">Chat</Tabs.Tab>
                <Tabs.Tab className="custom-tab" value="gallery">Gallery</Tabs.Tab>
                <Tabs.Tab className="custom-tab" value="account">Account</Tabs.Tab>
            </Tabs.List>
            <Tabs.Panel className='tab-content' value="chat" pb="xs">Chat panel</Tabs.Panel>
            <Tabs.Panel className='tab-content' value="gallery" pb="xs">Gallery panel</Tabs.Panel>
            <Tabs.Panel className='tab-content' value="account" pb="xs">Account panel</Tabs.Panel>

        </Tabs>
    </>
}



