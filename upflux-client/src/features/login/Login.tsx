import React, { useState } from 'react';
import { Container, Box, Image, Button, Text, Radio, TextInput, Loader } from '@mantine/core';
import logo from "../../assets/logos/logo-light-large.png";
import './login.css';
import { engineerLoginSubmit } from '../../api/loginRequests';
import { ROLES, useAuth } from '../../common/authProvider/AuthProvider';
import { useNavigate } from 'react-router-dom';
import { AdminLogin } from '../adminLogin/AdminLogin';

interface LoginFormState {
  tokenFile: File | null | string;
}

export const LoginComponent: React.FC = () => {
  const navigate = useNavigate();
  const [formState, setFormState] = useState<LoginFormState>({ tokenFile: null });
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(false);

  const { login, isAuthenticated } = useAuth();

  const [isLoginEngineer, setIsLoginEngineer] = useState(true)


  if (isAuthenticated) {
    navigate('/update-management');
  }

  const handleInputChange = (field: keyof LoginFormState) => (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    if (field === 'tokenFile') {
      const file = event.target.files?.[0] || null;
      if (file) {
        if (file.type !== 'application/json') {
          setErrorMessage('Please upload a valid JSON file.');
          setFormState({ tokenFile: null });
        } else if (file.size > 2 * 1024 * 1024) { 
          setErrorMessage('Please upload a file smaller than 2MB.');
          setFormState({ tokenFile: null });
        } else {
          setFormState({ tokenFile: file });
          setErrorMessage(null);
        }
      }
    }
  };

  const validateForm = (): boolean => {
    const { tokenFile } = formState;
    if (!tokenFile) {
      setErrorMessage('Token file not recognized.');
      return false;
    }
    return true;
  };

  const handleSubmit = async (): Promise<void> => {
    if (validateForm() && formState.tokenFile) {
      const reader = new FileReader();

      setLoading(true);
      reader.onload = async () => {
        try {
          // Parse the JSON content from the file
          const tokenContent = JSON.parse(reader.result as string);

          // Ensure the JSON contains the "engineerToken" property
          if (!tokenContent.engineerToken) {
            setErrorMessage('Invalid token file. "engineerToken" property is missing.');
            return;
          }

          // Prepare the payload, only send the token
          const payload = { engineerToken: tokenContent.engineerToken };

          // Call the login function with the payload
          try {
            const result = await engineerLoginSubmit(payload);

            // Redirect or handle successful login
            setIsLoggedIn(true);
            login(ROLES.ENGINEER, result.token);

            if (result) {
              console.log('Login successful!');
              navigate('/update-management');
            } else {
              setErrorMessage('Login failed.');
            }
          } catch (error) {
            console.error('Error during login:', error);
            setErrorMessage('An unexpected error occurred. Please try again.');
          }
        } catch (error) {
          console.error('Invalid JSON file:', error);
          setErrorMessage('The uploaded file is not a valid JSON file.');
        }
        finally{
          setLoading(false);
        }
      };

      // Read the uploaded file as text
      reader.readAsText(formState.tokenFile as File);
    }
  };

  return (
    <>
      <Container className="login-container">
        {isLoggedIn && <h4>You have been logged in successfully</h4>}
        <Box className="main-card">
          <Image src={logo} alt="UpFlux Logo" className="upflux-logo" />
          <Box
            className="error-message-container"
            style={{ color: "red", fontWeight: "bold" }}
          >
            {errorMessage && (
              <Text className={`error-message ${errorMessage ? "active" : ""}`}>
                {errorMessage}
              </Text>
            )}
          </Box>
          <Box className="input-field-box">
            <Radio label="Engineer Login" onChange={(event) => setIsLoginEngineer(event.currentTarget.checked)} checked={isLoginEngineer} />
            {isLoginEngineer && <Box className="file-input-box">
              <label htmlFor="tokenFile" className="file-label">
                Token File
              </label>
              <input
                type="file"
                id="tokenFile"
                accept=".json"
                onChange={handleInputChange("tokenFile")}
                className="file-input"
              />
            </Box>
            }
            <Radio label="Admin Login" onChange={(event) => setIsLoginEngineer(!event.currentTarget.checked)} checked={!isLoginEngineer} />
            {!isLoginEngineer &&
              <AdminLogin />
            }
          </Box>



          {isLoginEngineer && <Button className="login-button" style={{ backgroundColor: '#2F3BFF', color: '#fff' }} 
          onClick={handleSubmit}
          disabled={loading}>
           {loading ? <Loader size="sm" color="white" /> : "Log in"}
           </Button>}

        </Box>
      </Container>
    </>
  );
};
