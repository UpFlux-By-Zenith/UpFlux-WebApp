import React from 'react';
import { Navbar } from '../navbar/Navbar';
import { PasswordSettingsContent } from './PasswordSettings';
import { Footer } from '../footer/Footer';

export const PasswordSettingsRoute: React.FC = () => {
  return (
    <>
      <Navbar onHomePage={false} />
      <PasswordSettingsContent />
      <Footer />
    </>
  );
};
