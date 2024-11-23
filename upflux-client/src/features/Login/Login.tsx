import React, { useState } from 'react';
import { Container, Box, Image, Button, TextInput, Text } from '@mantine/core';
import logo from "../../assets/logos/logo-light-large.png";
import './login.css';

interface LoginFormState {
  userName: string;
  tokenFile: File | null;
}

export const LoginComponent: React.FC = () => {
  const [formState, setFormState] = useState<LoginFormState>({
    userName: '',
    tokenFile: null,
  });

  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleInputChange = (field: keyof LoginFormState) => (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    if (field === 'tokenFile') {
      // Handle file input
      const file = event.target.files?.[0] || null;
      if (file && file.type !== 'application/json') {
        setErrorMessage('Please upload a valid JSON file.');
      } else {
        setFormState({
          ...formState,
          [field]: file,
        });
        setErrorMessage(null); // Clear any existing errors
      }
    } else {
      // Handle text input
      setFormState({
        ...formState,
        [field]: event.target.value,
      });
      setErrorMessage(null); // Clear error when user types
    }
  };  

  const validateForm = (): boolean => {
    const { userName, tokenFile } = formState;

    if (!userName.trim() || !tokenFile) {
      setErrorMessage('User Name or Token not recognised.');
      return false;
    }

    return true;
  };

  const handleSubmit = (): void => {
    if (validateForm()) {
      console.log('Form submitted:', formState);

      if (formState.tokenFile) {
        // Read the file content
        const reader = new FileReader();
        reader.onload = () => {
          console.log('Token file content:', reader.result);

          // Process the string content here

        };
        reader.readAsText(formState.tokenFile);
      }
    }
  };

  return (
    <Container className="login-container">
      <Box className="main-card">
        <Image src={logo} alt="UpFlux Logo" className="logo" />

        {errorMessage && (
          <Text className="error-message">
            {errorMessage}
          </Text>
        )}

        <Box className="input-field-box">
          <TextInput
            placeholder="User Name"
            value={formState.userName}
            onChange={handleInputChange('userName')}
            className="input-card"
          />

          <Box className="file-input-box">
            <label htmlFor="tokenFile" className="file-label">
              Token File
            </label>
            <input
              type="file"
              id="tokenFile"
              accept=".json"
              onChange={handleInputChange('tokenFile')}
              className="file-input"
            />
          </Box>
        </Box>

        <Button className="login-button" onClick={handleSubmit}>
          Log in
        </Button>

        <Box className="forgot-password">
          <a href="#" className="forgot-password-link">Forgotten your Password?</a>
        </Box>
      </Box>
    </Container>
  );
};
