import React, { useState } from 'react';
import { Container, Box, Image, Button, TextInput, Text } from '@mantine/core';
import logo from "../../assets/logos/logo-light-large.png";
import './login.css';

interface LoginFormState {
  userName: string;
  password: string;
}

export const LoginComponent: React.FC = () => {
  const [formState, setFormState] = useState<LoginFormState>({
    userName: '',
    password: '',
  });

  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleInputChange = (field: keyof LoginFormState) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setFormState({
      ...formState,
      [field]: event.target.value,
    });
    // Clear error when user types
    setErrorMessage(null); 
  };

  const validateForm = (): boolean => {
    const { userName, password } = formState;

    if (!userName.trim() || !password || password.length < 6) {
      setErrorMessage('User Name or Password not recognised.');
      return false;
    }

    return true;
  };

  const handleSubmit = (): void => {
    if (validateForm()) {
      console.log('Form submitted:', formState);
      // Login Logic Here
    }
  };

  return (
    <Container className="login-container">
      <Box className="main-card">
        <Image src={logo} alt="UpFlux Logo" className="logo" />

        {/* Error Message */}
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

          <TextInput
            placeholder="Password"
            type="password"
            value={formState.password}
            onChange={handleInputChange('password')}
            className="input-card"
            style={{ marginTop: '15px' }}
          />
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
