import logo from "./assets/logos/logo-dark.png";
import './App.css';
import { BrowserRouter as Router } from 'react-router-dom'; 
import Navbar from './features/Navbar/Navbar'; 
import HeroImage from './features/Header/Header'; 
import About from './features/About/About';
import ContactUs from './features/ContactUs/ContactUs';
import Footer from './features/Footer/Footer';


export const App = () => {

  return (
    <Router>
      <section id="home">
    <Navbar />
    </section>
    <HeroImage />

    <section id="about">
    <About/>
    </section>

    <section id="contact">
    <ContactUs/>
    </section>
    <Footer/>
  </Router>

  );
}

