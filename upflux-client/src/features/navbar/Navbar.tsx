import React from 'react';
import { Container, Image, Menu, Text } from '@mantine/core';
import { Link } from 'react-router-dom';
import notifBell from "../../assets/images/notif_bell.png";
import logo from "../../assets/logos/logo-no-name.png";
import './navbar.css';

// Accept a prop `home` to conditionally render the navigation items
interface NavbarProps {
  onHomePage: boolean;
}

export const Navbar: React.FC<NavbarProps> = (props: NavbarProps) => {
  const { onHomePage } = props;

  return (
    <Container fluid className="navbar">
      <div className="navbar-logo">
        {/* Conditionally render the Link based on onHomePage */}
        {onHomePage ? (
          <Image src={logo} alt="Logo" className="logo" />
        ) : (
          <Link to="/update-management">
            <Image src={logo} alt="Logo" className="logo" />
          </Link>
        )}
      </div>
      
      <ul className="navbar-links">
        {onHomePage ? (
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
            <li className="profile">
              {/* Profile Menu with hover dropdown */}
              <Menu width={80} trigger="hover">
                <Menu.Target>
                  <Link to="profile">Profile</Link>
                </Menu.Target>
                <Menu.Dropdown>
                  <Menu.Item component={Link} to="/">Logout</Menu.Item>
                </Menu.Dropdown>
              </Menu>
            </li>  
          </>
        )}
      </ul>
    </Container>
  );
};
