import React from 'react';
import { Container, Button, Text } from '@mantine/core';
import heroImage from '../../assets/images/hero-image.png'; 
import './header.css';

export const Header = () => {
  return (
    <div className="hero-image">
      <img src={heroImage} alt="Hero" className="hero-img" />
      <div className="overlay">
        <Container className="text-container" style={{ position: 'relative', zIndex: 2 }}>
          <Text component="h1" className="hero-title" size="xl">
            Welcome to UpFlux!
          </Text>
          <Text className="hero-subtitle" size="lg">
            The leading update management tool in paper mills
          </Text>
          <Button 
            component="a" 
            href="#learnMore" 
            className="learn-more-btn"
            variant="filled" 
            color="blue"
            size="md"
          >
            Learn More
          </Button>
        </Container>
      </div>
    </div>
  );
};
