import { AUTH_API } from './apiConsts';

interface EngineerTokenPayload {
  engineerEmail: string;
  engineerName: string;
  machineIds: string[];
}

export const getEngineerToken = async (payload: EngineerTokenPayload): Promise<string | null> => {

  try {
    const response = await fetch(AUTH_API.GET_ENGINEER_TOKEN, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      const data = await response.json();
      console.log('Token retrieved:', data);
      return data.token;
    } 
    
    else {
      const errorData = await response.json();
      console.error('Error fetching engineer token:', errorData);
      return errorData.message || 'Failed to fetch the token.';
    }
    
  } catch (error) {
    console.error('Error during engineer token request:', error);
    return 'An error occurred while fetching the token.';
  }
};
