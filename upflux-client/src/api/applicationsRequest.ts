import { ADMIN_REQUEST_API, DATA_REQUEST_API } from './apiConsts';

interface MachineDetailsResponse {
  applications: {
    appId: number;
    machineId: string;
    appName: string;
    addedBy: string;
    currentVersion: string;
    versions: {
      versionId: number;
      appId: number;
      versionName: string;
      updatedBy: string;
      date: string;
    }[];
  }[];
}

export const getMachineDetails = async (): Promise<MachineDetailsResponse | string | null> => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');
  console.log('authToken:', authToken);

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(DATA_REQUEST_API.GET_MACHINE_DETAILS, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });

    if (response.ok) {
      const data: MachineDetailsResponse = await response.json();
      console.log('Machine details fetched successfully:', data);
      return data;
    } else {
      const errorData = await response.json();
      console.error('Error fetching machine details:', errorData);
      return errorData.message || 'Failed to fetch machine details.';
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    return 'An error occurred while fetching machine details.';
  }
};


export const CreateSubscription = async (groupId: string) => {
  try {

      // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');
  console.log('authToken:', authToken);

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

    const response = await fetch("http://localhost:5000/api/Notification/create-group", {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`,
      },
      body: JSON.stringify({groupid : groupId}),
    });

    const data = await response.json() ;
    return data; 
    
  } catch (error) {
    console.error('Error during admin login request:', error);
    return { "error" : error};
  }
};

export const getALLMachineDetails = async () => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');
  console.log('authToken:', authToken);

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(ADMIN_REQUEST_API.GET_ALL_MACHINES, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });

    if (response.ok) {
      const data = await response.json();
      console.log('Machine details fetched successfully:', data);
      return data.accessibleMachines.result;
    } else {
      const errorData = await response.json();
      console.error('Error fetching machine details:', errorData);
      return errorData.message || 'Failed to fetch machine details.';
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    return 'An error occurred while fetching machine details.';
  }
};