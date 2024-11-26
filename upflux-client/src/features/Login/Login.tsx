import React, { useState } from 'react';
import { Container, Box, Image, Button, Text } from '@mantine/core';
import logo from "../../assets/logos/logo-light-large.png";
import './login.css';
import { submitLogin } from '../../api/loginRequests';

interface LoginFormState {
  tokenFile: File | null;
}

export const LoginComponent: React.FC = () => {
  const [formState, setFormState] = useState<LoginFormState>({ tokenFile: null });
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleInputChange = (field: keyof LoginFormState) => (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const { files } = event.target;
    setFormState((prevState) => ({
      ...prevState,
      [field]: files?.[0] || null,
    }));
    setError(null);
  };

  const validateForm = (): boolean => {
    if (!formState.tokenFile) {
      setError('Token file is required');
      return false;
    } 
    
    if (formState.tokenFile.type !== 'application/json') {
      setError('Please upload a valid JSON file');
      return false;
    }

    return true;
  };

  const handleSubmit = async (): Promise<void> => {
    if (validateForm() && formState.tokenFile) {
      setIsLoading(true);
      const reader = new FileReader();
      reader.onload = async () => {
        const tokenContent = reader.result as string;
        const payload = {email:"engineer@upflux.com", engineerToken: tokenContent };

        try {
          const authToken = await submitLogin(payload);
          if (authToken) {
            sessionStorage.setItem('engineerToken', authToken);
            console.log('Login successful!');
          } else {
            setError('An unexpected error occurred. Please try again.');
          }
        } catch (error) {
          console.error('Error during login:', error);
          setError('An unexpected error occurred. Please try again.');
        } finally {
          setIsLoading(false);
        }
      };
      reader.readAsText(formState.tokenFile);
    }
  };

  return (
    <Container className="login-container">
      <Box className="main-card">
        <Image src={logo} alt="UpFlux Logo" className="logo" />
        {error && <Text className="error-message">{error}</Text>}
        <Box className="input-field-box">
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
        <Button
          className="login-button"
          onClick={handleSubmit}
          loading={isLoading}
          disabled={isLoading}
        >
          Log in
        </Button>
        <Box className="forgot-password">
          <a href="/password-settings" className="forgot-password-link">Forgotten your Password?</a>
        </Box>
      </Box>
    </Container>
  );
};