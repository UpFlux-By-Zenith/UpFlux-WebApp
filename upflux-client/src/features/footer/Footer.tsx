import React from 'react';
import { Divider, Text, Container, Anchor, Group } from '@mantine/core';
import './footer.css';

export const Footer = () => {
  return (
    <Container className="footer" fluid>
      <Text className="contact-us">Contact Us</Text>

      <Divider className="footer-line" />

      <div className="footer-bottom">
        <Text size="sm" className="upflux-copyright">
          Copyright © 2024 UpFlux all rights reserved
        </Text>
      </div>
    </Container>
  );
};
