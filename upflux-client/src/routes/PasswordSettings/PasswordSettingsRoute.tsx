import React from 'react';
import { Navbar } from '../../features/Navbar/Navbar';
import { PasswordSettingsContent } from '../../features/PasswordSettings/PasswordSettings';
import { Footer } from '../../features/Footer/Footer';

export const PasswordSettingsRoute: React.FC = () => {
  return (
    <>
      <Navbar onHomePage={false} />
      <PasswordSettingsContent />
      <Footer />
    </>
  );
};
