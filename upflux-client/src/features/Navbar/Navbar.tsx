import { Container, Image } from '@mantine/core';
import { Link } from 'react-router-dom'; 
import logo from "../../assets/logos/logo-light-large.png";
import './navbar.css';

export const Navbar = () => {
  return (
    <Container fluid className="navbar">
      <div className="navbar-logo">
        <Image src={logo} alt="Logo" className="logo" />
      </div>
      <ul className="navbar-links">
        <li><Link to="/">Home</Link></li>        
        <li><Link to="/#about">About</Link></li>     
        <li><Link to="/#contact">Contact</Link></li> 
        <li><Link to="/login">Login</Link></li>      
      </ul>
    </Container>
  );
};
