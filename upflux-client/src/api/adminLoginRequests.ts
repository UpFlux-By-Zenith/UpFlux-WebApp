import { AUTH_API } from './apiConsts';

interface AdminLoginPayload {
  email: string;
  password: string;
}

export const adminLogin = async (payload: AdminLoginPayload): Promise<string | null> => {
  try {
    const response = await fetch(AUTH_API.ADMIN_LOGIN, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      const data = await response.json();
      console.log('Admin login successful:', data);
      return null; 
    } else {
      const errorData = await response.json();
      console.error('Admin login error:', errorData);
      return 'Admin login failed. Please check your credentials.';
    }
  } catch (error) {
    console.error('Error during admin login request:', error);
    return 'An error occurred while submitting the admin login request.';
  }
};
