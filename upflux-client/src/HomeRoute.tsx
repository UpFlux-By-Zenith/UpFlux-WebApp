import React from 'react';
import { Navbar } from './features/navbar/Navbar';
import { Header } from './features/header/Header';
import { About } from './features/about/About';
import { ContactUs } from './features/contactUs/ContactUs';
import { Footer } from './features/footer/Footer';

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
