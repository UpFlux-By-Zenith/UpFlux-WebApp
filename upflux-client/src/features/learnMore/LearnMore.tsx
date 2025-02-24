import { Container, Text, Title, Stack } from '@mantine/core';
import './learnMore.css';

export const LearnMore = () => {
  return (
    <Container className="learn-more-container">
      <Stack className="text-section">
        <Title order={2}>Efficient Software Update Management</Title>
        <Title order={1} className="highlighted-title">
          All in one remote Access
        </Title>
        <Text mt="md" size="md">
          UpFlux brings powerful, centralized software update management right at your fingertips. In our all-in-one platform, you can remotely monitor, manage, and deploy updates across an entire system to guarantee security, efficiency, and minimum downtime.
        </Text>
      </Stack>
      <Stack className="video-section">
        <video className="video-player" autoPlay muted loop>
          <source src="/globe.mp4" type="video/mp4" />
          Your browser does not support the video tag.
        </video>
      </Stack>
    </Container>
  );
};
