import React, { useEffect, useState } from "react";
import { Container, Image, Menu, Text, Group, Avatar } from "@mantine/core";
import { Link } from "react-router-dom";
import logo from "../../assets/logos/logo-no-name.png";
import "./navbar.css";
import { useAuth } from "../../common/authProvider/AuthProvider";

interface NavbarProps {
  onHomePage: boolean;
  notifications: any[];
}

export const Navbar: React.FC<NavbarProps> = ({ onHomePage }) => {

  const { logout } = useAuth()

  return (
    <Container fluid className="navbar">
      <div className="navbar-logo">
        {onHomePage ? (
          <Image src={logo} alt="Logo" className="logo" />
        ) : (
          <Link to="/update-management">
            <Image src={logo} alt="Logo" className="nav-logo" />
          </Link>
        )}
      </div>
      <ul className="navbar-links">
        {onHomePage ? (
          <>
            <li><Link to="/">Home</Link></li>
            <li><a href="#about">About</a></li>
            <li><a href="#contact">Contact</a></li>
            <li><Link to="/login">Login</Link></li>
          </>
        ) : (
          <>
            <li className="profile">
              {/* Profile Menu */}
              <Menu width={80} trigger="hover">
                <Menu.Target>
                  <Link to="/account-settings">Profile</Link>
                </Menu.Target>
                <Menu.Dropdown>
                  <Menu.Item component={Link} onClick={logout} to="/">Logout</Menu.Item>
                </Menu.Dropdown>
              </Menu>
            </li>
          </>
        )}
      </ul>
    </Container>
  );
};
