import { useState } from "react";
import { Container, Box, Image, Button, TextInput, Text } from '@mantine/core';
import logo from "../../assets/logos/logo-light-large.png";
import { ROLES, useAuth } from "../../common/authProvider/AuthProvider";
import { useNavigate } from "react-router-dom";
import { adminLogin } from "../../api/adminApiActions";

interface AdminLoginFormState {
  email: string;
  password: string;
}

export const AdminLogin = () => {
  const [formState, setFormState] = useState<AdminLoginFormState>({ email: '', password: '' });
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const { login } = useAuth();
  const navigate = useNavigate()

  
  const handleInputChange = (field: keyof AdminLoginFormState) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setFormState({ ...formState, [field]: event.target.value });
  };

  const handleSubmit = async () => {
    // Reset error message
    setErrorMessage(null);

    // Basic validation
    if (!formState.email || !formState.password) {
      setErrorMessage("Both email and password are required.");
      return;
    }

    // Send login request to API
    const response = await adminLogin({
      email: formState.email,
      password: formState.password,
    });

    // Save the token to local storage
    localStorage.setItem('authToken', response);
    login(ROLES.ADMIN, response);
    // Redirect or handle successful login
    navigate("/admin-dashboard")
  }

  return (
    <Container className="login-container">
      <Box className="main-card">
        <Image src={logo} alt="UpFlux Logo" className="upflux-logo" />
        {errorMessage && <Text className="error-message">{errorMessage}</Text>}
        <Box className="input-field-box">
          <TextInput
            placeholder="E-mail"
            value={formState.email}
            onChange={handleInputChange('email')}
            className="input-card"
          />
          <TextInput
            placeholder="Password"
            type="password"
            value={formState.password}
            onChange={handleInputChange('password')}
            className="input-card"
          />
        </Box>
        <Button className="login-button" onClick={handleSubmit}>Log in</Button>
        <Box className="forgot-password">
          <a href="/password-settings" className="forgot-password-link">Forgotten your Password?</a>
        </Box>
      </Box>
    </Container>
  );
};
