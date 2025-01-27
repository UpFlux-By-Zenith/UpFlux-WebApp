import React, { useEffect, useState } from "react";
import { Container, Image, Menu, Text, Group, Avatar } from "@mantine/core";
import { Link } from "react-router-dom";
import notifBell from "../../assets/images/notif_bell.png";
import logo from "../../assets/logos/logo-no-name.png";
import "./navbar.css";

interface NavbarProps {
  onHomePage: boolean;
  notifications: any[];
}

interface Notification {
  id: number;
  message: string;
  image: string;
  timestamp: string;
}

export const Navbar: React.FC<NavbarProps> = ({ onHomePage, notifications }) => {
  // Check if there are unread notifications
  const hasUnreadNotifications = notifications.length > 0;

  return (
    <Container fluid className="navbar">
      <div className="navbar-logo">
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
            <li><Link to="/login">Login</Link></li>
          </>
        ) : (
          <>
            <li className="notification-icon">
              {/* Notifications Menu */}
              <Menu width={500} shadow="md" position="bottom-end" trigger="hover">
                <Menu.Target>
                  <div className="notif-bell-wrapper">
                    <Image src={notifBell} alt="Notifications" className="notif-bell" />
                    {/* Blue Circle Indicator */}
                    {hasUnreadNotifications && <div className="notif-indicator"></div>}
                  </div>
                </Menu.Target>
                <Menu.Dropdown>
                  {notifications.length > 0 ? (
                    notifications.map((notification) => (
                      <Menu.Item key={notification.id}>
                        <Group align="center">
                          <Group>
                            <Avatar src={notification.image} size={32} radius="xl" />
                            <Text>{notification.message}</Text>
                          </Group>
                          <Text size="xs" color="dimmed">
                            {notification.timestamp}
                          </Text>
                        </Group>
                      </Menu.Item>
                    ))
                  ) : (
                    <Menu.Item disabled>No notifications</Menu.Item>
                  )}
                </Menu.Dropdown>
              </Menu>
            </li>
            <li className="profile">
              {/* Profile Menu */}
              <Menu width={80} trigger="hover">
                <Menu.Target>
                  <Link to="/account-settings">Profile</Link>
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
