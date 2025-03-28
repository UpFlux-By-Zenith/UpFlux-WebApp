import React, { useState, useEffect } from 'react';
import { TextInput, Stack, Box } from '@mantine/core';
import { postAuthToken } from '../../api/parseTokenRequest';
import './accountSettings.css';

export const AccountSettings: React.FC = () => {
  // State for user info
  const [email, setEmail] = useState<string>('');
  const [role, setRole] = useState<string>('');
  const [machines, setMachines] = useState<string>('');

  // Fetch and parse token
  useEffect(() => {
    const fetchUserInfo = async () => {
      const result = await postAuthToken();
      if (typeof result === 'object' && result) {
        setEmail(result["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || '');
        setRole(result["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || '');
        setMachines(result["MachineIds"]?.split(',').join(', ') || '');
      } else {
        console.error('Failed to fetch user info:', result);
      }
    };

    fetchUserInfo();
  }, []);

  return (
    <Stack className="password-settings-content">

      <Box className="grid-container">
        <TextInput
          label="Name"
          value="Unknown" // If you later get a Name claim, you can replace this
          readOnly
          disabled
          className="input-field"
        />
        <TextInput
          label="Email"
          value={email}
          readOnly
          disabled
          className="input-field"
        />
        <TextInput
          label="Role"
          value={role}
          readOnly
          disabled
          className="input-field"
        />
        <TextInput
          label="Accessible Machines"
          value={machines}
          readOnly
          disabled
          className="input-field"
        />
      </Box>
    </Stack>
  );
};
