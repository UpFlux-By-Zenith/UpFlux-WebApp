// src/features/EngineerToken/GetEngineerToken.tsx
import React, { useState, useEffect } from 'react';
import { TextInput, Button, Stack, Box, Text } from '@mantine/core';
import API_BASE_URL, { AUTH_API } from '../../api/apiConsts';
import './getEngineerToken.css';

export const GetEngineerToken: React.FC = () => {
  // State for form fields
  const [engineerEmail, setEngineerEmail] = useState('');
  const [engineerName, setEngineerName] = useState('');
  const [machineIds, setMachineIds] = useState('');
  const [token, setToken] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [authorized, setAuthorized] = useState<boolean | null>(null); // To track if the user is authorized

  useEffect(() => {
    // Verify the admin token from sessionStorage on page load
    const adminToken = sessionStorage.getItem('adminToken');
    
    if (!adminToken) {
      setAuthorized(false);
      return;
    }

    const verifyAdminToken = async () => {
      try {
        const response = await fetch(AUTH_API.VERIFY_TOKEN, {
          method: 'POST',
          headers: {
            'Content-Type': 'text/plain', // Sending a plain string
          },
          body: adminToken, // Send the token directly as a string
        });

        if (response.ok) {
          setAuthorized(true); // Admin token is valid
        } else {
          setAuthorized(false); // Admin token is invalid
        }
      } catch (error) {
        console.error('Error verifying token:', error);
        setAuthorized(false); // Error during verification
      }
    };

    verifyAdminToken();
  }, []);

  if (authorized === null) {
    // Show a loading indicator while checking authorization
    return <Text>Verifying token...</Text>;
  }

  if (!authorized) {
    // If not authorized, display an error message and nothing else
    return <Text>You are not authorized to access this page. Please log in first.</Text>;
  }

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

    try {
      const result = await getEngineerToken(payload);

      if (result) {
        setToken(result); // Token retrieved successfully
        setErrorMessage(null);
        downloadToken(result); // Trigger the file download
      } else {
        setToken(null);
        setErrorMessage('Failed to fetch the engineer token. Please check your inputs.');
      }
    } catch (error) {
      console.error('Error during token creation:', error);
      setErrorMessage('An unexpected error occurred. Please try again.');
    }
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

async function getEngineerToken(payload: { engineerEmail: string; engineerName: string; machineIds: string[] }) {
  const response = await fetch(`${API_BASE_URL}/api/Auth/admin/create-engineer-token`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json', // Sending JSON data
    },
    body: JSON.stringify(payload), // Sending the payload as JSON
  });

  if (!response.ok) {
    throw new Error('Failed to create engineer token');
  }

  const data = await response.json();
  return data.token; // Return the token from the response
}
