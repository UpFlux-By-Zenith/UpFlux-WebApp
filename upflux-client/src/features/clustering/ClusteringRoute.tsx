import React from 'react';
import { Navbar } from '../navbar/Navbar';
import { Clustering } from './Clustering';
import { Footer } from '../footer/Footer';

export const ClusteringRoute: React.FC = () => {
  return (
    <>
      <Navbar onHomePage={false} />
      <Clustering />
      <Footer />
    </>
  );
};
