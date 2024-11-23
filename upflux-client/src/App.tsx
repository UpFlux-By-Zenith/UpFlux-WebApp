import logo from "./assets/logos/logo-dark.png";
import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Navbar } from './features/Navbar/Navbar';
import { Header } from './features/Header/Header';
import { About } from './features/About/About';
import { ContactUs } from './features/ContactUs/ContactUs';
import { Footer } from './features/Footer/Footer';
import { LoginComponent } from './features/Login/Login';
import { MantineProvider } from "@mantine/core";
import { PasswordSettingsContent } from "./features/PasswordSettings/PasswordSettings";

export const App = () => {
  return (
    <MantineProvider>
      <Router>
        <Routes>
          {/* Home route */}
          <Route
            path="/"
            element={
              <>
                <Navbar home={true} /> {/* Pass home=true */}
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

          {/* Login route */}
          <Route path="/login" element={<LoginComponent />} />

          {/* Password settings route */}
          <Route
            path="/password-settings"
            element={
              <>
                <Navbar home={false} /> {/* Pass home=false */}
                <PasswordSettingsContent />
                <Footer />
              </>
            }
          />
        </Routes>
      </Router>
    </MantineProvider>
  );
};
