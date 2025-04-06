import { ADMIN_REQUEST_API, AUTH_API, DATA_REQUEST_API, LICENCE_APIS } from './apiConsts';
import { AdminLoginPayload, EngineerTokenPayload, LoginResponse } from './apiTypes';

export const revokeEngineer = async (engineerId: string, reason: string) => {
  try {

    // Retrieve the token from session storage
    const authToken = sessionStorage.getItem('authToken');

    if (!authToken) {
      console.error('No authentication token found in session storage.');
      return null;
    }
    // Make the API request with the Bearer token
    const response = await fetch(ADMIN_REQUEST_API.REVOKE_ENGINEER, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Add the Bearer token
      },
      body: JSON.stringify({ engineerId, reason }),
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

export const getMachineLogs = async (deviceUuids: string[]) => {
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(ADMIN_REQUEST_API.GET_MACHINE_LOGS, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`,
      },
      body: JSON.stringify({ deviceUuids: deviceUuids }),
    });

    if (response.ok) {
      const contentDisposition = response.headers.get('content-disposition');
      const contentType = response.headers.get('content-type');

      if (contentDisposition && contentType && contentType === 'application/zip') {
        const fileName = contentDisposition
          .split(';')
          .find((value) => value.trim().startsWith('filename='))
          ?.split('=')[1]
          .replace(/['"]/g, '');

        if (!fileName) {
          console.error('Filename could not be determined from the response.');
          return;
        }

        const blob = await response.blob();

        // Optional: Log blob to confirm the file content
        console.log('Downloaded file blob:', blob);

        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = fileName;

        // Append link to body
        document.body.appendChild(link);

        // Trigger download and safely revoke URL after
        setTimeout(() => {
          link.click();
          document.body.removeChild(link);
          // Delay revoke to allow download to properly start
          setTimeout(() => {
            URL.revokeObjectURL(link.href);
          }, 100);
        }, 0);

      } else {
        const errorData = await response.json();
        console.error('Error fetching machine logs:', errorData);
        throw errorData.message || 'Failed to fetch machine logs.';
      }
    } else {
      const errorData = await response.json();
      console.error('Error fetching machine logs:', errorData);
      throw errorData.message || 'Failed to fetch machine logs.';
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    throw 'An error occurred while fetching machine logs.';
  }
};

export const getAllMachineLogs = async () => {
  // Retrieve the token from session storage
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(ADMIN_REQUEST_API.GET_MACHINE_LOGS + "-all", {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`, // Include Bearer token
      },
    });

    if (response.ok) {
      // Handle file download
      const contentDisposition = response.headers.get('content-disposition');
      const contentType = response.headers.get('content-type');

      if (contentDisposition && contentType && contentType === 'application/zip') {
        // Get the filename from the content-disposition header
        const fileName = contentDisposition
          .split(';')
          .find((value) => value.trim().startsWith('filename='))
          ?.split('=')[1]
          .replace(/['"]/g, ''); // Remove quotes if present

        if (!fileName) {
          console.error('Filename could not be determined from the response.');
          return;
        }

        // Convert the response to a blob
        const blob = await response.blob();

        // Create a link element
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = fileName; // Set the filename for download

        // Trigger the download
        link.click();

        // Clean up the object URL after the download
        URL.revokeObjectURL(link.href);
      } else {
        const errorData = await response.json();
        console.error('Error fetching machine logs:', errorData);
        return errorData.message || 'Failed to fetch machine logs.';
      }
    } else {
      const errorData = await response.json();
      console.error('Error fetching machine logs:', errorData);
      return errorData.message || 'Failed to fetch machine logs.';
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    return 'An error occurred while fetching machine logs.';
  }
}


export const getWebServiceLogs = async () => {
  const authToken = sessionStorage.getItem('authToken');

  if (!authToken) {
    console.error('No authentication token found in session storage.');
    return null;
  }

  try {
    const response = await fetch(ADMIN_REQUEST_API.GET_WEBSERVICE_LOG, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`,
      },
    });

    if (response.ok) {
      const contentDisposition = response.headers.get('content-disposition');
      const contentType = response.headers.get('content-type');

      // Check if content type is 'application/zip'
      if (contentType && contentType.includes('application/zip')) {
        // Extract file name from content-disposition
        const fileName = contentDisposition
          ? contentDisposition
            .split(';')
            .find((value) => value.trim().startsWith('filename='))
            ?.split('=')[1]
            .replace(/['"]/g, '') // Remove any quotes around the file name
          : 'logs.zip'; // Default filename if not specified

        if (!fileName) {
          console.error('Filename could not be determined from the response.');
          return;
        }

        const blob = await response.blob();

        // Optional: Log blob to confirm the file content
        console.log('Downloaded file blob:', blob);

        // Create a link element
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = fileName;

        // Append link to body and trigger click event to start download
        document.body.appendChild(link);
        link.click();

        // Clean up: Remove link and revoke the object URL
        document.body.removeChild(link);
        setTimeout(() => {
          URL.revokeObjectURL(link.href);
        }, 100);
      } else {
        const errorData = await response.json();
        console.error('Error fetching logs:', errorData);
        return errorData.message || 'Failed to fetch logs.';
      }
    } else {
      const errorData = await response.json();
      console.error('Error fetching logs:', errorData);
      return errorData.message || 'Failed to fetch logs.';
    }
  } catch (error) {
    console.error('Error during fetch request:', error);
    return 'An error occurred while fetching logs.';
  }
};
