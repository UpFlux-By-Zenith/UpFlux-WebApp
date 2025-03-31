// src/features/EngineerToken/GetEngineerToken.tsx
import React, { useCallback, useEffect, useState } from "react";
import {
  TextInput,
  Button,
  Stack,
  Box,
  Text,
  MultiSelect,
  ComboboxData,
  Modal,
} from "@mantine/core";
import { useAuth } from "../../common/authProvider/AuthProvider";
import { getEngineerToken } from "../../api/adminApiActions";
import { useSelector } from "react-redux";
import { RootState } from "../reduxSubscription/store";

interface IMachineDetails {
  machineId: number;
  dateAddedOn: string;
  ipAddress: string;
}

export const GetEngineerToken = ({ opened, close }) => {
  // State for form fields
  const [engineerEmail, setEngineerEmail] = useState("");
  const [engineerName, setEngineerName] = useState("");
  const [machineIds, setMachineIds] = useState([]);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [token, setToken] = useState("");
  const [multiSelectOptions, setMultiSelectOptions] = useState([]);
  const { authToken } = useAuth();
  //Machine list from redux 
  const storedMachines = useSelector((root: RootState) => root.machines.messages)

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();


    const payload = {
      engineerEmail,
      engineerName,
      machineIds: machineIds,
    };

    await getEngineerToken(payload, authToken);
  };

  useEffect(() => {
    const multiSelectOptions = Object.keys(storedMachines).map((val) => {
      return { value: val, label: val };
    });
    setMultiSelectOptions(multiSelectOptions);
  }, [storedMachines]);

  const handleChange = useCallback((va) => {
    setMachineIds([...va]); // Ensure state is properly updated
  }, [setMachineIds]);

  return (
    <Modal opened={opened} onClose={close} centered>

      <Stack align="center" className="form-stack" >
        <Text className="form-title">Create Engineer Token</Text>

        <TextInput
          label="Engineer Email"
          placeholder="Enter engineer email"
          value={engineerEmail}
          onChange={(e) => setEngineerEmail(e.target.value)}
          className="input-field"
        />

        <TextInput
          label="Engineer Name"
          placeholder="Enter engineer name"
          value={engineerName}
          onChange={(e) => setEngineerName(e.target.value)}
          className="input-field"
        />
        <div className="input-field">

          <MultiSelect
            onChange={handleChange}
            data={multiSelectOptions}
            label="Machine IDs"
            placeholder="Select machines to give access to"
          />
        </div>

        <Button color="rgba(0, 3, 255, 1)" onClick={handleSubmit} className="submit-button">
          Create Token
        </Button>

        {token && (
          <Text className="token-display">
            <strong>Token:</strong> {token}
          </Text>
        )}

        {errorMessage && <Text className="error-message">{errorMessage}</Text>}
      </Stack>
    </Modal>
  );
};
