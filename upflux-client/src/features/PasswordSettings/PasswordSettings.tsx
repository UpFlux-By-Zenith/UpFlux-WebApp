import React, { useState } from 'react';
import { TextInput, Button, Text, Stack, Box, Group } from '@mantine/core';
import './passwordSettings.css';

export const PasswordSettingsContent: React.FC = () => {
  // State for form values and errors
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  
  const [errors, setErrors] = useState({
    newPassword: '',
    confirmPassword: '',
  });

  // Validation function for the New Password
  const validateNewPassword = (password: string) => {
    const minLength = 12;
    const hasNumber = /\d/;
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/;

    if (password.length < minLength) {
      return 'Password must be at least 12 characters long.';
    }
    if (!hasNumber.test(password)) {
      return 'Password must contain at least one number.';
    }
    if (!hasSpecialChar.test(password)) {
      return 'Password must contain at least one special character.';
    }
    return ''; // No error
  };

  // Validation function for Confirm Password
  const validateConfirmPassword = (confirmPassword: string, newPassword: string) => {
    if (confirmPassword !== newPassword) {
      return 'Passwords do not match.';
    }
    return ''; // No error
  };

  // Handle form submission
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    // Validate fields
    const newPasswordError = validateNewPassword(newPassword);
    const confirmPasswordError = validateConfirmPassword(confirmPassword, newPassword);

    // Set errors if any
    setErrors({
      newPassword: newPasswordError,
      confirmPassword: confirmPasswordError,
    });

    // If no errors, proceed with form submission (e.g., updating password)
    if (!newPasswordError && !confirmPasswordError) {
      console.log('Password updated successfully!');
      // Perform further actions (e.g., make API call)
    }
  };

  return (
    <Stack className="password-settings-content">
      {/* Account Settings and Password Buttons */}
      <Group className="setting-selector" align="center">
        <Button className="selector-button" variant="subtle" disabled>
          Account Settings
        </Button>
        <Button className="selector-button active" variant="subtle">
          Password
        </Button>
      </Group>

      {/* Instructions and Input Fields */}
      <Text className="instructions">
        Please enter your current password and create a new one.
      </Text>
      <TextInput
        label={
          <>
            Current password <span className="label-required">*</span>
          </>
        }
        placeholder="Please enter your current password"
        className="input-field wide-input"
        value={currentPassword}
        onChange={(e) => setCurrentPassword(e.target.value)}
      />

      <TextInput
        label={
          <>
            New password <span className="label-required">*</span>
          </>
        }
        placeholder="Please enter your new password"
        className="input-field wide-input"
        type="password"
        value={newPassword}
        onChange={(e) => setNewPassword(e.target.value)}
        error={errors.newPassword}
      />
      <TextInput
        label={
          <>
            Confirm new password <span className="label-required">*</span>
          </>
        }
        placeholder="Please confirm your new password"
        className="input-field wide-input"
        type="password"
        value={confirmPassword}
        onChange={(e) => setConfirmPassword(e.target.value)}
        error={errors.confirmPassword}
      />

      {/* Action Buttons */}
      <Box className="button-group">
        <Button variant="default" className="cancel-button">
          Cancel
        </Button>
        <Button className="update-button" onClick={handleSubmit}>
          Update Password
        </Button>
      </Box>
    </Stack>
  );
};
