import { useState } from "react";
import { Container, Box, Button, TextInput, Text, Image } from '@mantine/core';
import { useNavigate } from "react-router-dom";
import "./forgotPassword.css";
import logo from "../../assets/logos/logo-light-large.png";

// Function to send email notification to user
const sendEmailNotification = async (email: string) => {
  const response = await fetch('/api/send-password-update-email', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ userEmail: email }),
  });

  if (response.ok) {
    console.log('Email sent successfully');
  } else {
    console.log('Failed to send email');
  }
};

export const ForgotPassword = () => {
  const [email, setEmail] = useState('');
  const [successMessage, setSuccessMessage] = useState<string | null>(null); 
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  
  // Use navigate hook to redirect to login page
  const navigate = useNavigate();

  // Handle email change
  const handleEmailChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setEmail(e.target.value);
  };

  // Handle form submission
  const handleSubmit = async () => {
    // Basic validation for email input
    if (!email) {
      setErrorMessage("Email is required.");
      setSuccessMessage(null); // Reset success message
      return;
    }
    setErrorMessage(null); // Clear error if any

    // Send email notification
    try {
      await sendEmailNotification(email);
      // On success, show success message
      setSuccessMessage("An email has been sent to reset your password.");
      
      // Redirect to login page after success
      setTimeout(() => {
        navigate('/admin-login'); 
      });

    } catch (err) {
      setErrorMessage("Failed to send email.");
      setSuccessMessage(null); 
    }
  };

  return (
    <Container className="login-container">
      <Box className="main-card">
        <Image src={logo} alt="UpFlux Logo" className="upflux-logo" />
        
        {/* Error message display */}
        <Box className="request-error-message-container" style={{ color: 'red', fontWeight: 'bold' }}>
          {errorMessage && <Text className={`request-error-message ${errorMessage ? 'active' : ''}`}>{errorMessage}</Text>}
        </Box>

        <Box className="input-field-box">
          <Text className="input-label">Please enter your e-mail address</Text>
          <TextInput
            placeholder="Enter your email"
            className="input-card"
            value={email}
            onChange={handleEmailChange}
            error={!!errorMessage}  // Show error state in UI
            required
          />
        </Box>
        
        <Button className="submit-request" onClick={handleSubmit}>Submit Request</Button>
      </Box>
    </Container>
  );
};
