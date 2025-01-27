import React, { useState, useEffect } from 'react';
import { TextInput, Button, Text, Stack, Group, Box } from '@mantine/core';
import { Link } from 'react-router-dom';
import { postAuthToken } from '../../api/parseTokenRequest';
import './accountSettings.css';

export const AccountSettings: React.FC = () => {
  // State for user role
  const [role, setRole] = useState<string | null>(null);

  // Fetch and verify the token on component mount
  useEffect(() => {
    const fetchUserRole = async () => {
      const result = await postAuthToken();
      if (typeof result === 'object' && result) {
        setRole(result['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']);
      } else {
        console.error('Failed to fetch role:', result);
      }
    };

    fetchUserRole();
  }, []);

  return (
    <Stack className="password-settings-content">
      {/* Account Settings and Password Buttons */}
      <Group className="setting-selector" align="center">
        <Button className="selector-button active" variant="subtle">
          Account Settings
        </Button>
        {/* Conditionally render the Password button */}
        {role === 'ADMIN' && (
          <Link to="/password-settings">
            <Button className="selector-button" variant="subtle">
              Password
            </Button>
          </Link>
        )}
      </Group>

      {/* Instructions and Input Fields */}
      <Text className="instructions">View your account details below.</Text>
      <Box className="grid-container">
        <TextInput
          label="Name"
          value="Adam Smith"
          readOnly
          className="input-field"
        />
        <TextInput
          label="Email"
          value="adam@upflux.com"
          readOnly
          className="input-field"
        />
        <TextInput
          label="Role"
          value="Engineer"
          readOnly
          className="input-field"
        />
        <TextInput
          label="Accessible Machines"
          value="Machine 001, Machine 002"
          readOnly
          className="input-field"
        />
      </Box>
    </Stack>
  );
};
