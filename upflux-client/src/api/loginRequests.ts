import { AUTH_API } from './apiConsts';

interface LoginPayload {
  engineerToken: string;
}

interface AdminLoginPayload {
  email: string;
  password: string;
}

export const engineerLoginSubmit = async (payload: LoginPayload) => {

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
      return data;
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

export const adminLoginSubmit = async (payload: AdminLoginPayload): Promise<string | null> => {

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
      return data;
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


