import { Container, Title, Text, SimpleGrid, Stack, Image } from "@mantine/core";
import "./ourSolution.css";
import machineImage from "../../assets/images/machines_graph.png";
import metricImage from "../../assets/images/metric_graphs.jpg";
import clusterImage from "../../assets/images/cluster_graph.png";

export const OurSolution = () => {
  return (
    <Container className="our-solution">
      <SimpleGrid cols={2} spacing="xl">
        
        {/* Left side - Placeholder images */}
        <Stack align="center">
          <Image src={machineImage} alt="Placeholder 1" className="solution-image-left" />
          <Image src={metricImage} alt="Placeholder 2" className="solution-image-right" />
          <Image src={clusterImage} alt="Placeholder 3" className="solution-image-left" id="image-bottom" />
        </Stack>

        {/* Right side - Text Content */}
        <Stack>
          <Title order={2} className="solution-title">Our Solution</Title>
          <Text size="md" className="solution-text">
            Take Full Control over Your Production Machines: UpFlux is the perfect tool for remotely updating software and managing systems with ease. Our platform guarantees smooth updates, machine status, and performance optimization throughout the network.
          </Text>
          <Text size="md" className="solution-text">
            UpFlux provides real-time monitoring, smart clustering, and version control while maintaining security over your updates using trusted authentication mechanisms. Deploy faster and manage performance with more reliability through UpFlux.
          </Text>
        </Stack>
      </SimpleGrid>
    </Container>
  );
};
