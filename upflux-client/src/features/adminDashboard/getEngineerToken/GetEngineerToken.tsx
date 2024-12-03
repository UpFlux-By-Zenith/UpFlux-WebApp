// src/features/EngineerToken/GetEngineerToken.tsx
import React, { useState, useEffect } from 'react';
import { TextInput, Button, Stack, Box, Text } from '@mantine/core';
import API_BASE_URL, { AUTH_API } from '../../../api/apiConsts';
import './getEngineerToken.css';
import { useAuth } from '../../../common/authProvider/AuthProvider';
import { getEngineerToken } from '../../../api/adminApiActions';

export const GetEngineerToken: React.FC = () => {
  // State for form fields
  const [engineerEmail, setEngineerEmail] = useState('');
  const [engineerName, setEngineerName] = useState('');
  const [machineIds, setMachineIds] = useState('');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [token,setToken] = useState("")

  const { authToken } = useAuth();



  // Function to download the engineer token as a file
  const downloadToken = (token: string) => {
    const blob = new Blob([token], { type: 'text/plain' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = 'engineer-token.txt'; // The name of the file
    link.click(); // Trigger the download
  };

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Split machineIds by commas and trim any extra spaces
    const machineIdsArray = machineIds.split(',').map((id) => id.trim());

    const payload = {
      engineerEmail,
      engineerName,
      machineIds: machineIdsArray,
    };

    await getEngineerToken(payload,authToken);
  
  };

  return (
    <Box className="get-engineer-token-container">
      <Stack align="center" className="form-stack">
        <Text className="form-title">Create Engineer Token</Text>

        <TextInput
          label="Engineer Email"
          placeholder="Enter engineer email"
          value={engineerEmail}
          onChange={(e) => setEngineerEmail(e.target.value)}
          className="input-field"
        />

        <TextInput
          label="Engineer Name"
          placeholder="Enter engineer name"
          value={engineerName}
          onChange={(e) => setEngineerName(e.target.value)}
          className="input-field"
        />

        <TextInput
          label="Machine IDs (comma-separated)"
          placeholder="e.g., Machine1, Machine2"
          value={machineIds}
          onChange={(e) => setMachineIds(e.target.value)}
          className="input-field"
        />

        <Button onClick={handleSubmit} className="submit-button">
          Create Token
        </Button>

        {token && (
          <Text className="token-display">
            <strong>Token:</strong> {token}
          </Text>
        )}

        {errorMessage && (
          <Text className="error-message">
            {errorMessage}
          </Text>
        )}
      </Stack>
    </Box>
  );
};
