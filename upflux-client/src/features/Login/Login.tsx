import React, { useState } from 'react';
import { Container, Box, Image, Button, TextInput, Text } from '@mantine/core';
import logo from "../../assets/logos/logo-light-large.png";
import './login.css';

interface LoginFormState {
  email: string;
  tokenFile: File | null;
}

export const LoginComponent: React.FC = () => {
  const [formState, setFormState] = useState<LoginFormState>({ email: '', tokenFile: null });
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleInputChange = (field: keyof LoginFormState) => (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    if (field === 'tokenFile') {
      const file = event.target.files?.[0] || null;
      if (file && file.type !== 'application/json') {
        setErrorMessage('Please upload a valid JSON file.');
      } else {
        setFormState({ ...formState, [field]: file });
        setErrorMessage(null);
      }
    } else {
      setFormState({ ...formState, [field]: event.target.value });
      setErrorMessage(null);
    }
  };

  const validateForm = (): boolean => {
    const { email, tokenFile } = formState;
    if (!email.trim() || !tokenFile) {
      setErrorMessage('E-mail or Token not recognised.');
      return false;
    }
    return true;
  };

  const handleSubmit = async (): Promise<void> => {
    if (validateForm() && formState.tokenFile) {
      const reader = new FileReader();
      reader.onload = async () => {
        const tokenContent = reader.result as string;
        const payload = { email: formState.email, engineerToken: tokenContent };
      };
      reader.readAsText(formState.tokenFile);
    }
  };

  return (
    <Container className="login-container">
      <Box className="main-card">
        <Image src={logo} alt="UpFlux Logo" className="logo" />
        {errorMessage && <Text className="error-message">{errorMessage}</Text>}
        <Box className="input-field-box">
          <TextInput
            placeholder="E-mail"
            value={formState.email}
            onChange={handleInputChange('email')}
            className="input-card"
          />
          <Box className="file-input-box">
            <label htmlFor="tokenFile" className="file-label">Token File</label>
            <input
              type="file"
              id="tokenFile"
              accept=".json"
              onChange={handleInputChange('tokenFile')}
              className="file-input"
            />
          </Box>
        </Box>
        <Button className="login-button" onClick={handleSubmit}>Log in</Button>
        <Box className="forgot-password">
          <a href="/password-settings" className="forgot-password-link">Forgotten your Password?</a>
        </Box>
      </Box>
    </Container>
  );
};
