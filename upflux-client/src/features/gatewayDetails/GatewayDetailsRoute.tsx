import React from 'react';
import { Box } from '@mantine/core';
import './gatewayDetails.css';
import {BlankNavbar} from '../../features/navbar/BlankNavbar';
import {GatewayDetails} from './GatewayDetails';
import { Footer } from '../footer/Footer';

const MachineDetailsRoute: React.FC = () => {
    return (
        <>
            <BlankNavbar />
            <Box className="details-section">
            <GatewayDetails />
            </Box>
            <Footer />
        </>
    );
};

export default MachineDetailsRoute;