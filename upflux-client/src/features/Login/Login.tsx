import { Container, Box, Image, Button, TextInput } from '@mantine/core';
import logo from "../../assets/logos/logo-light-large.png";
import './login.css';

export const LoginComponent = () => {
  return (
    <Container className="login-container">
      <Box className="main-card">
        
        <Image
          src={logo}
          alt="UpFlux Logo"
          className="logo"
        />

        <Box className="input-field-box">
          <TextInput
            placeholder="User Name"
            className="input-card"
          />

          <TextInput
            placeholder="Password"
            type="password"
            className="input-card"
            style={{ marginTop: '15px' }}
          />
        </Box>

        <Button className="login-button">Log in</Button>

        <Box className="forgot-password">
          <a href="#" className="forgot-password-link">Forgotten your Password?</a>
        </Box>
      </Box>
    </Container>
  );
};
