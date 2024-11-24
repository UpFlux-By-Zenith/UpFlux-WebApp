import React from 'react';
import { Navbar } from '../Navbar/Navbar';
import { PasswordSettingsContent } from './PasswordSettings';
import { Footer } from '../Footer/Footer';

export const PasswordSettingsRoute: React.FC = () => {
  return (
    <>
      <Navbar onHomePage={false} />
      <PasswordSettingsContent />
      <Footer />
    </>
  );
};
