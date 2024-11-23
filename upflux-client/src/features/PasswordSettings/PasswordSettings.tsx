import React from 'react';
import { TextInput, Button, Text, Stack, Box, Group } from '@mantine/core';
import './passwordSettings.css';

export const PasswordSettingsContent: React.FC = () => {
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
      />

      <TextInput
        label={
          <>
            New password <span className="label-required">*</span>
          </>
        }
        placeholder="Please enter your new password"
        className="input-field wide-input"
      />
      <TextInput
        label={
          <>
            Confirm new password <span className="label-required">*</span>
          </>
        }
        placeholder="Please confirm your new password"
        className="input-field wide-input"
      />

      {/* Action Buttons */}
      <Box className="button-group">
        <Button variant="default" className="cancel-button">
          Cancel
        </Button>
        <Button className="update-button">Update Password</Button>
      </Box>
    </Stack>
  );
};
