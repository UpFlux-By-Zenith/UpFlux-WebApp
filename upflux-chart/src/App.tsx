import "@mantine/core/styles.css";
import { MantineProvider } from "@mantine/core";
import ScatterRealtime from "./ScatterRealtime";

export default function App() {
  return (
    <MantineProvider>
      <ScatterRealtime />
    </MantineProvider>
  );
}
