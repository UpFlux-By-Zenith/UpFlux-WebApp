import { DATA_REQUEST_API } from './apiConsts';
import { IMachine } from './reponseTypes';

interface MachineAccessResponse {
  engineerEmail: string;
  accessibleMachines: IMachine[]
}

export const getAccessibleMachines = async (): Promise<MachineAccessResponse | string | null> => {
  // Retrieve the token from local storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in local storage.');
    return null;
  }

  try {
    const response = await fetch(DATA_REQUEST_API.GET_ACCESS_MACHINES, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Add the Bearer token
      },
    });

    if (response.ok) {
      const data: MachineAccessResponse = await response.json();
      return data;
    } else {
      const errorData = await response.json();
      console.error('Error fetching accessible machines:', errorData);
      return errorData.message || 'Failed to fetch accessible machines.';
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    return 'An error occurred while fetching the accessible machines.';
  }
};
