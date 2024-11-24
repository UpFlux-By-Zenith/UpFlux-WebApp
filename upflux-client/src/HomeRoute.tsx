import React from 'react';
import { Navbar } from './features/Navbar/Navbar';
import { Header } from './features/Header/Header';
import { About } from './features/About/About';
import { ContactUs } from './features/ContactUs/ContactUs';
import { Footer } from './features/Footer/Footer';

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
