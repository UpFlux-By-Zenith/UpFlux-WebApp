import logo from "./assets/logos/logo-dark.png";
import './App.css';
import { BrowserRouter as Router } from 'react-router-dom'; 
import Navbar from './components/Navbar'; 
import HeroImage from './components/Header'; 
import About from './components/About';
import ContactUs from './components/ContactUs';


export const App = () => {

  return (
    <Router>
    <Navbar />
    <HeroImage />
    <About/>
    <ContactUs/>
  </Router>

  );
}

