import React from 'react';
import { Container, Image } from '@mantine/core';
import { Link } from 'react-router-dom';
import notifBell from "../../assets/images/notif_bell.png";
import logo from "../../assets/logos/logo-no-name.png";
import './navbar.css';

// Accept a prop `home` to conditionally render the navigation items
interface NavbarProps {
  home: boolean;
}

export const Navbar: React.FC<NavbarProps> = ({ home }) => {
  return (
    <Container fluid className="navbar">
      <div className="navbar-logo">
        <Image src={logo} alt="Logo" className="logo" />
      </div>
      <ul className="navbar-links">
        {home ? (
          <>
            <li><Link to="/">Home</Link></li>
            <li><a href="#about">About</a></li>
            <li><a href="#contact">Contact</a></li>
            <li><Link to="login">Login</Link></li>
          </>
        ) : (
          <>
            <li className="notification-icon">
              <Image src={notifBell} alt="Notifications" className="notif-bell" />
            </li>
            <li className="profile"><Link to="profile">Profile</Link></li>  
          </>
        )}
      </ul>
    </Container>
  );
};
