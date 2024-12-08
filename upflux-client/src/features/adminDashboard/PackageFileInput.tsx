import { FileInput, Stack, Text } from "@mantine/core";

export const PackageFileInput = () => {
  return (
    <>
      <Stack align="center" className="form-stack">
        <Text className="form-title">Get Signed Update Package</Text>
        <FileInput clearable label="Upload files" placeholder="Upload files" />
      </Stack>
    </>
  );
};
