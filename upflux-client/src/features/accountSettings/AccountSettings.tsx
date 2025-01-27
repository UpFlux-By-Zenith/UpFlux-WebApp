import React, { useState, useEffect } from 'react';
import Layout from '../../Layout';
import { TextInput, Button, Text, Stack, Box, Group } from '@mantine/core';
import { Link } from 'react-router-dom';
import { postAuthToken } from '../../api/parseTokenRequest';
import './accountSettings.css';

export const AccountSettings: React.FC = () => {
  // State for form values and errors
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [errors, setErrors] = useState({
    newPassword: '',
    confirmPassword: '',
  });
  const [generalError, setGeneralError] = useState<string | null>(null); // Error for API responses

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
      <Text className="instructions">
        View your account details below.
      </Text>
      <TextInput
        label={
          <>
            Name
          </>
        }
        placeholder="{PLACEHOLDER_CURRENT_PASSWORD}"
        className="input-field wide-input"
        type="password"
        value={currentPassword}
        onChange={(e) => setCurrentPassword(e.target.value)}
      />

      <TextInput
        label={
          <>
            Email
          </>
        }
        placeholder="{PLACEHOLDER_NEW_PASSWORD}"
        className="input-field wide-input"
        type="password"
        value={newPassword}
        onChange={(e) => setNewPassword(e.target.value)}
        error={errors.newPassword}
      />
      <TextInput
        label={
          <>
            Role
          </>
        }
        placeholder="{PLACEHOLDER_CONFIRM_PASSWORD}"
        className="input-field wide-input"
        type="password"
        value={confirmPassword}
        onChange={(e) => setConfirmPassword(e.target.value)}
        error={errors.confirmPassword}
      />

      {/* Display general API error if it exists */}
      {generalError && (
        <Text color="red" className="error-text">
          {generalError}
        </Text>
      )}

    </Stack>
  );
};
