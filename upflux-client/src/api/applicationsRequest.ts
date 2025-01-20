import { DATA_REQUEST_API } from './apiConsts';

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
