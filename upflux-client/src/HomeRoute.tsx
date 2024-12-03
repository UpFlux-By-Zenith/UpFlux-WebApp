import React from 'react';
import { useAuth } from './common/authProvider/AuthProvider';
import { About } from './features/about/About';
import { ContactUs } from './features/contactUs/ContactUs';
import { Footer } from './features/footer/Footer';
import { Header } from './features/header/Header';
import { Navbar } from './features/navbar/Navbar';

export const HomeRoute: React.FC = () => {
  return (
    <>
      <Navbar onHomePage={true} />
      <Header />
      <section id="about">
        <About />
      </section>
      <section id="contact">
        <ContactUs />
      </section>
      <Footer />
    </>
  );
};
