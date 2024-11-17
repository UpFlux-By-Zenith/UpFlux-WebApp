import logo from "./assets/logos/logo-dark.png";
import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'; 
import { Navbar } from './features/Navbar/Navbar'; 
import { Header } from './features/Header/Header'; 
import { About } from './features/About/About';
import { ContactUs } from './features/ContactUs/ContactUs';
import { Footer } from './features/Footer/Footer';
import { LoginComponent } from './features/Login/Login'; // Import the LoginComponent
import { MantineProvider } from "@mantine/core";

export const App = () => {
  return (
    <MantineProvider>
      <Router> 
        <Routes>
          <Route 
            path="/" 
            element={
              <>
                <Navbar />
                <Header />
                <section id="about">
                  <About />
                </section>
                <section id="contact">
                  <ContactUs />
                </section>
                <Footer />
              </>
            } 
          />

          <Route path="/login" element={<LoginComponent />} />
        </Routes>
      </Router>
    </MantineProvider>
  );
};
