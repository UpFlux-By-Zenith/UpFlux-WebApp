import React, { useState } from 'react';
import { Container, Box, Image, Button, TextInput, Text } from '@mantine/core';
import logo from '../../assets/logos/logo-light-large.png';
import './adminLogin.css';
import { adminLogin } from '../../api/adminLoginRequests';
import { useNavigate } from 'react-router-dom';

interface AdminLoginFormState {
  email: string;
  password: string;
}

export const AdminLogin: React.FC = () => {
  const [formState, setFormState] = useState<AdminLoginFormState>({ email: '', password: '' });
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleInputChange = (field: keyof AdminLoginFormState) => (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    setFormState((prevState) => ({
      ...prevState,
      [field]: event.target.value,
    }));
    setError(null); // Clear error when user starts typing
  };

  const validateForm = (): boolean => {
    if (!formState.email.trim() || !formState.password.trim()) {
      setError('Email and password are required');
      return false;
    }
    return true;
  };

  const handleSubmit = async (): Promise<void> => {
    if (validateForm()) {
      setIsLoading(true);
      const payload = {
        email: formState.email,
        password: formState.password,
      };

      try {
        const adminToken = await adminLogin(payload);
        if (adminToken) {
          sessionStorage.setItem('adminToken', adminToken);
          console.log('Admin login successful!');
          setError(null);
          //Navigate to /get-engineer-token route
          navigate('/get-engineer-token'); // Navigate 
        } else {
          setError('Login failed. Please check your credentials.');
        }
      } catch (error) {
        console.error('Error during admin login:', error);
        setError('An unexpected error occurred. Please try again.');
      } finally {
        setIsLoading(false);
      }
    }
  };

  return (
    <Container className="login-container">
      <Box className="main-card">
        <Image src={logo} alt="Company Logo" className="logo" />
        {error && <Text className="error-message">{error}</Text>}
        <Box className="input-field-box">
        <label htmlFor="email" className="file-label">E-Mail</label>
          <TextInput
            placeholder="Enter your email"
            value={formState.email}
            onChange={handleInputChange('email')}
            className="input-field"
          />

          <label htmlFor="password" className="file-label">Password</label>
          <TextInput
            type="password"
            placeholder="Enter your password"
            value={formState.password}
            onChange={handleInputChange('password')}
            className="input-field"
          />
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
          <a href="/password-settings" className="forgot-password-link">
            Forgotten your Password?
          </a>
        </Box>
      </Box>
    </Container>
  );
};
