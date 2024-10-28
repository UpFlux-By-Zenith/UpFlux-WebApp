import logo from "./assets/logos/logo-dark.png";
import './App.css';
import { BrowserRouter as Router } from 'react-router-dom'; 
import Navbar from './components/Navbar'; 
import HeroImage from './components/Header'; 
import About from './components/About';
import ContactUs from './components/ContactUs';
import Footer from './components/Footer';


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

