import React from 'react';
import { Container, Image, Menu, Text, Group, Avatar } from '@mantine/core';
import { Link } from 'react-router-dom';
import notifBell from "../../assets/images/notif_bell.png";
import logo from "../../assets/logos/logo-no-name.png";
import './navbar.css';

interface NavbarProps {
  onHomePage: boolean;
}

export const Navbar: React.FC<NavbarProps> = (props: NavbarProps) => {
  const { onHomePage } = props;

  // Hardcoded notification data
  const notifications = [
    {
      id: 1,
      message: "New update available for Cluster 001",
      image: "https://via.placeholder.com/32",
      timestamp: "2h ago",
    },
    {
      id: 2,
      message: "Cluster 002 needs attention",
      image: "https://via.placeholder.com/32",
      timestamp: "5h ago",
    },
    {
      id: 3,
      message: "System maintenance scheduled for Jan 20th",
      image: "https://via.placeholder.com/32",
      timestamp: "1d ago",
    },
  ];

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
            <li><Link to="login">Login</Link></li>
          </>
        ) : (
          <>
            <li className="notification-icon">
              {/* Notifications Menu */}
              <Menu width={450} shadow="md" position="bottom-end" trigger="hover">
                <Menu.Target>
                  <Image src={notifBell} alt="Notifications" className="notif-bell" />
                </Menu.Target>
                <Menu.Dropdown>
                  {notifications.map((notification) => (
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
                  ))}
                  {notifications.length === 0 && (
                    <Menu.Item disabled>No notifications</Menu.Item>
                  )}
                </Menu.Dropdown>
              </Menu>
            </li>
            <li className="profile">
              {/* Profile Menu */}
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
