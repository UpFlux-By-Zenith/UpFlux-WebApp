import { AUTH_API } from './apiConsts';
import { AdminLoginPayload, EngineerTokenPayload } from './apiTypes';



export const adminLogin = async (payload: AdminLoginPayload) => {
  try {
    const response = await fetch(AUTH_API.ADMIN_LOGIN, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    const data : {token :string } = await response.json();
    console.log('Admin login successful:', data);
    return data.token; 

  } catch (error) {
    console.error('Error during admin login request:', error);
    return null;
  }
};



export const getEngineerToken = async (payload: EngineerTokenPayload , adminAuthToken:string): Promise<void> => {
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
      console.log('Token retrieved:', data);

      // Convert JSON response to a Blob
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });

      // Create a download link
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = `${payload.engineerName}AccessToken.json`; // Filename for the download
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);

      console.log('JSON response downloaded as file.');
    } else {
      const errorData = await response.json();
      console.error('Error fetching engineer token:', errorData);
    }
  } catch (error) {
    console.error('Error during engineer token request:', error);
  }
};
