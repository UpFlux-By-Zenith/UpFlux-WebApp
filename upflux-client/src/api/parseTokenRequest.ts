import { AUTH_API } from './apiConsts';

interface ParseResponse {
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": string;
    "MachineIds": string;
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": string;
    exp: string;
    iss: string;
    aud: string;
}

export const postAuthToken = async (): Promise<ParseResponse | string | null> => {
  // Retrieve the token from local storage
  const authToken = sessionStorage.getItem('authToken');
  console.log('authToken:', authToken);

  if (!authToken) {
    console.error('No authentication token found in local storage.');
    return null;
  }

  try {
    const response = await fetch(AUTH_API.VERIFY_TOKEN, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Add the Bearer token
      },
      body: JSON.stringify({ token: authToken }), // Send the token in the body
    });

    if (response.ok) {
      const data: ParseResponse = await response.json();
      console.log('Token verified successfully:', data);
      return data;
    } else {
      const errorData = await response.json();
      console.error('Error verifying token:', errorData);
      return errorData.message || 'Failed to verify token.';
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    return 'An error occurred while verifying the token.';
  }
};