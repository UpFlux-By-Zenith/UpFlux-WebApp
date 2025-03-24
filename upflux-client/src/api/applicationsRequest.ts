import { IMachineLicense } from '../features/adminDashboard/ManageMachines';
import { IEngineer } from '../features/adminDashboard/ViewEngineers';
import { IMachineStatus } from '../features/reduxSubscription/subscriptionConsts';
import { ADMIN_REQUEST_API, DATA_REQUEST_API, LICENCE_APIS, PACKAGE_DEPOYMENT, ROLLBACK } from './apiConsts';

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

export interface IStoredVersionsResponse {
  id: number,
  machineId: string;
  versionName: string;
  installedAt: string;
}

export const getMachineStatus = async () => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(DATA_REQUEST_API.GET_MACHINES_STATUS, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });

    if (response.ok) {
      const data: IMachineStatus[] = await response.json();
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
}

export const getMachineStoredVersions = async () => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(DATA_REQUEST_API.GET_MACHINE_STORED_VERSIONS, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });

    if (response.ok) {
      const data: IStoredVersionsResponse[] = await response.json();
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
}

export const getMachineDetails = async (): Promise<MachineDetailsResponse | string | null> => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(DATA_REQUEST_API.GET_AVAILABLE_APPLICATIONS, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });

    if (response.ok) {
      const data: MachineDetailsResponse = await response.json();
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
      body: JSON.stringify({ groupid: groupId }),
    });

    const data = await response.json();
    return data;

  } catch (error) {
    return { "error": error };
  }
};

export const getAllMachineDetails = async () => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

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

export const deployPackage = async (appName: string, ver: string, targetedMachines: string[]) => {
  try {

    // Retrieve the token from session storage
    const authToken = sessionStorage.getItem('authToken');

    if (!authToken) {
      console.error('No authentication token found in session storage.');
      return null;
    }

    const response = await fetch("http://localhost:5000/api/PackageManagement/packages/upload", {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`,
      },
      body: JSON.stringify({
        name: appName,
        version: ver,
        targetDevices: targetedMachines
      }),
    });

    const data = await response.json();
    return data;

  } catch (error) {
    console.error('Error during deploy request:', error);
    return { "error": error };
  }
}


export const generateMachineId = async () => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(LICENCE_APIS.GENERATE_MACHINE_ID, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });

    if (response.ok) {
      const data: { machineId: string } = await response.json();
      return data.machineId;
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

export const getMachinesWithLicense = async () => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(ADMIN_REQUEST_API.GET_MACHINES_WITH_LICENSE, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });
    if (response.ok) {
      const data: IMachineLicense[] = await response.json();
      return data;
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    return 'An error occurred while fetching machine details.';
  }
};

export const getAllEngineers = async () => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(ADMIN_REQUEST_API.GET_ALL_ENGINEERS, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });
    if (response.ok) {
      const data: IEngineer[] = await response.json();
      return data;
    }
  } catch (error) {
    return 'An error occurred while fetching machine details.';
  }
}

export const getAllMachinesWithApps = async () => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(ADMIN_REQUEST_API.GET_ALL_MACHINES_WITH_APPS, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });
    if (response.ok) {
      const data = await response.json();
      return data;
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    return 'An error occurred while fetching machine details.';
  }
}

export const getRunningMachinesApplications = async () => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(DATA_REQUEST_API.GET_RUNNING_APPS, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });
    if (response.ok) {
      const data = await response.json();
      return data;
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    return 'An error occurred while fetching machine details.';
  }
}



export const getAvailablePackages = async () => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(PACKAGE_DEPOYMENT.GET_AVAILABLE_PACKAGES, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });
    if (response.ok) {
      const data: IPackagesOnCloud[] = await response.json();
      return data;
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    return 'An error occurred while fetching machine details.';
  }
}

export interface IPackagesOnCloud {
  name: string,
  versions: string[]
}

export const doRollback = async (versionId: string, deviceId: string) => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(`${ROLLBACK}?version=${versionId}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
      body: JSON.stringify([deviceId])
    });
    if (response.ok) {
      const data: IPackagesOnCloud[] = await response.json();
      return data;
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    return 'An error occurred while fetching machine details.';
  }
}