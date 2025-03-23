import { AUTH_API, LICENCE_APIS } from './apiConsts';
import { AdminLoginPayload, EngineerTokenPayload, LoginResponse } from './apiTypes';



export const adminLogin = async (payload: AdminLoginPayload) => {
  try {
    const response = await fetch(AUTH_API.ADMIN_LOGIN, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    const data = await response.json() as LoginResponse;
    return data;

  } catch (error) {
    console.error('Error during admin login request:', error);
    return { "error": error } as LoginResponse;
  }
};



export const getEngineerToken = async (payload: EngineerTokenPayload, adminAuthToken: string): Promise<void> => {
  try {
    // Make the API request with the Bearer token
    const response = await fetch(AUTH_API.GET_ENGINEER_TOKEN, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${adminAuthToken}`, // Add the Bearer token
      },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      const data = await response.json();

      // Convert JSON response to a Blob
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });

      // Create a download link
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = `${payload.engineerName}AccessToken.json`; // Filename for the download
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);

    } else {
      const errorData = await response.json();
      console.error('Error fetching engineer token:', errorData);
    }
  } catch (error) {
    console.error('Error during engineer token request:', error);
  }
};

export const createMachineLicense = async (machineId: string) => {
  try {

    // Retrieve the token from session storage
    const authToken = sessionStorage.getItem('authToken');

    if (!authToken) {
      console.error('No authentication token found in session storage.');
      return null;
    }
    // Make the API request with the Bearer token
    const response = await fetch(LICENCE_APIS.REGISTRATION, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Add the Bearer token
      },
      body: JSON.stringify({ machineId }),
    });

    if (response.ok) {
      return response.json()
    } else {
      const errorData = await response.json();
      console.error('Error fetching engineer token:', errorData);
    }
  } catch (error) {
    console.error('Error during engineer token request:', error);
  }
}