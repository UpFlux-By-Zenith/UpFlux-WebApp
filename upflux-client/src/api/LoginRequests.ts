import { AUTH_API } from './apiConsts.js';

interface LoginPayload {
  email: string;
  engineerToken: string;
}

export const submitLogin = async (payload: LoginPayload): Promise<string | null> => {

  try {
    const response = await fetch(AUTH_API.LOGIN, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      const data = await response.json();
      console.log('Login successful:', data);
      return null; 
    }
    
    else {
      const errorData = await response.json();
      console.error('Login error:', errorData);
      return 'Login failed. Please check your credentials and token.';
    }

  } 
  
  catch (error) {
    console.error('Error during login request:', error);
    return 'An error occurred while submitting the login request.';
  }
};
