// src/features/EngineerToken/GetEngineerToken.tsx
import React, { useState} from 'react';
import { TextInput, Button, Stack, Box, Text } from '@mantine/core';
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
