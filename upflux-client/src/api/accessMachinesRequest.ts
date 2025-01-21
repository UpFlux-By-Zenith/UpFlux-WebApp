import { DATA_REQUEST_API } from './apiConsts';

interface MachineAccessResponse {
  engineerEmail: string;
  accessibleMachines: {
    result: {
      machineId: string;
      dateAddedOn: string;
      ipAddress: string;
    }[];
    id: number;
    exception: string | null;
    status: number;
    isCanceled: boolean;
    isCompleted: boolean;
    isCompletedSuccessfully: boolean;
    creationOptions: number;
    asyncState: unknown;
    isFaulted: boolean;
  };
}

export const getAccessibleMachines = async (): Promise<MachineAccessResponse | string | null> => {
  // Retrieve the token from local storage
  const authToken = sessionStorage.getItem('authToken');
  console.log('authToken:', authToken);

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
      console.log('Machines fetched successfully:', data);
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
