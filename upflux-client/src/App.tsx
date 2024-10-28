import logo from "./assets/logos/logo-dark.png";
import './App.css';
import { BrowserRouter as Router } from 'react-router-dom'; 
import Navbar from './components/Navbar'; 
import HeroImage from './components/Header'; 
import About from './components/About';


export const App = () => {

  return (
    <Router>
    <Navbar />
    <HeroImage />
    <About/>
    <div style={{ padding: '20px' }}>
      <h1>Welcome to My Website</h1>
      <p>This is the main content of the page.</p>
    </div>
  </Router>

  );
}

