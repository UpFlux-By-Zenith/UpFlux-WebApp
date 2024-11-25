import { AUTH_API } from './apiConsts';

interface PasswordChangePayload {
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export const changePassword = async (payload: PasswordChangePayload): Promise<string | null> => {

  try {
    const response = await fetch(AUTH_API.CHANGE_PASSWORD, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      console.log('Password updated successfully!');
      window.location.href = '/login'; 
      return null; // No error
    } 
    
    else {
      const errorData = await response.json();
      console.error('Change-password error:', errorData);
      return errorData.message || 'Failed to change password.';
    }
    
  } catch (error) {
    console.error('Error during password change request:', error);
    return 'An error occurred while updating the password.';
  }
};
