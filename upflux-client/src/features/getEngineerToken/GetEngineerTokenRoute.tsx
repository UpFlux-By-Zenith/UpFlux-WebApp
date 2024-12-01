import React from 'react';
import { Navbar } from '../navbar/Navbar';
import { GetEngineerToken } from './GetEngineerToken'; // Adjust the import path as needed

export const GetEngineerTokenRoute: React.FC = () => {
  return (
    <>
      <Navbar onHomePage={false} />
      <GetEngineerToken />
    </>
  );
};
