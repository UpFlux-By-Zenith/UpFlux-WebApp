import { useState } from "react";
import { Button, FileInput, Modal, Stack, Text } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import "./admin-dashboard.css";

export const PackageFileInput = ({ opened, close }) => {
  const [file, setFile] = useState<File | null>(null);

  // Handle file selection
  const handleFileChange = (selectedFile: File | null) => {
    setFile(selectedFile);
  };

  // Trigger the upload request when the file is selected
  const handleSubmit = async () => {
    if (file) {
      try {
        const formData = new FormData();
        formData.append('file', file, file.name);

        // Retrieve the token from session storage
        const authToken = sessionStorage.getItem('authToken');
        if (!authToken) {
          console.error('No authentication token found in session storage.');
          return;
        }

        const response = await fetch("http://52.215.12.174:5000/api/PackageManagement/sign", {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${authToken}`,
          },
          body: formData,
        });

        if (response.ok) {
          setFile(null);
          notifications.show({
            title: "Cloud Repository",
            position: "top-right",
            autoClose: 10000,
            message: "Package has been signed & stored"
          })
        }
        const data = await response.json();
      } catch (error) {
        console.error("Error during package upload:", error);
      }
    } else {
      console.error("No file selected");
    }
  };

  return (
    <Modal opened={opened} onClose={close} centered>
      <Stack align="center" className="form-stack">
        <Text className="form-title">Get Signed Update Package</Text>
        <FileInput
          style={{ width: "200px" }}
          className={"file-input"}
          clearable
          value={file}
          label="Upload files"
          placeholder="Upload files"
          onChange={handleFileChange}
        />
        <Button color="rgba(0, 3, 255, 1)" onClick={handleSubmit}>Upload Package</Button>
      </Stack>
    </Modal>
  );
};
