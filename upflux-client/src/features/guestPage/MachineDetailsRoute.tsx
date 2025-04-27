import React from 'react';
import { Box } from '@mantine/core';
import './machineDetails.css';
import { BlankNavbar } from '../navbar/BlankNavbar';
import { MachineDetails } from './MachineDetails';
import { Footer } from '../footer/Footer';

const MachineDetailsRoute: React.FC = () => {
    return (
        <>
            <BlankNavbar />
            <Box className="details-section">
                <MachineDetails />
            </Box>
            <Footer />
        </>
    );
};

export default MachineDetailsRoute;