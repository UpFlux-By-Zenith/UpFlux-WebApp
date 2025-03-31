import { useState } from "react";
import { Container, Box, Image, Button, TextInput, Text, Notification, Loader } from '@mantine/core';
import logo from "../../assets/logos/logo-light-large.png";
import { ROLES, useAuth } from "../../common/authProvider/AuthProvider";
import { useNavigate } from "react-router-dom";
import { adminLogin } from "../../api/adminApiActions";
import { LoginResponse } from "../../api/apiTypes";
import "./adminLogin.css";

interface AdminLoginFormState {
  email: string;
  password: string;
}

export const AdminLogin = () => {
  const [formState, setFormState] = useState<AdminLoginFormState>({ email: '', password: '' });
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  if (isAuthenticated) {
    navigate('/dashboard');
  }

  const handleInputChange = (field: keyof AdminLoginFormState) => (event: React.ChangeEvent<HTMLInputElement>) => {
    setFormState({ ...formState, [field]: event.target.value });
  };

  const validateForm = (): boolean => {
    if (!formState.email || !formState.password) {
      setErrorMessage("Both email and password are required.");
      return false;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formState.email)) {
      setErrorMessage("Please enter a valid email address.");
      return false;
    }

    if (formState.password.length < 6) {
      setErrorMessage("Password must be at least 6 characters long.");
      return false;
    }

    return true;
  };

  const handleSubmit = async () => {
    if (!validateForm()) return;

    setLoading(true);
    setErrorMessage(null);

    try {
      const response: LoginResponse = await adminLogin({
        email: formState.email,
        password: formState.password,
      });

      if (response.error) {
        setErrorMessage(response.error);
        return;
      } else {

      login(ROLES.ADMIN, response.token);
      navigate("/admin-dashboard");

      }

    } catch (error: any) {
      if (error.response) {
        setErrorMessage(error.response.data?.message || "Login failed. Please check your credentials.");
      } else if (error.request) {
        setErrorMessage("Network error. Please check your connection and try again.");
      } else {
        setErrorMessage("An unexpected error occurred.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <Box className="error-message-container" style={{ color: "red", fontWeight: "bold" }}>
        {errorMessage && (
          <Text className={`error-message ${errorMessage ? "active" : ""}`}>
            {errorMessage}
          </Text>
        )}
      </Box>

      <TextInput
        placeholder="E-mail"
        value={formState.email}
        onChange={handleInputChange('email')}
        className="input-card"
        disabled={loading}
      />
      <TextInput
        placeholder="Password"
        type="password"
        value={formState.password}
        onChange={handleInputChange('password')}
        className="input-card"
        disabled={loading}
      />

      <Button 
        className="login-button" 
        style={{ backgroundColor: '#2F3BFF', color: '#fff' }} 
        onClick={handleSubmit}
        disabled={loading}
      >
        {loading ? <Loader size="sm" color="white" /> : "Log in"}
      </Button>

      <Box className="forgot-password">
        <a href="/forgot-password" className="forgot-password-link">Forgotten your Password?</a>
      </Box>
    </>
  );
};
