import React from 'react';
import { Divider, Text, Container, Anchor, Group } from '@mantine/core';
import './footer.css';

export const Footer = () => {
  return (
    <Container className="footer" fluid>
      <Text className="contact-us">Contact Us</Text>

      <Divider className="footer-line" />

      <div className="footer-bottom">
        <Text size="sm" className="copyright">
          Copyright © 2024 UpFlux all rights reserved
        </Text>

        <Group gap="lg" className="footer-links">
          <Anchor href="/privacy-policy" className="footer-link">
            Privacy Policy
          </Anchor>
          <Anchor href="/terms-conditions" className="footer-link">
            Terms and Conditions
          </Anchor>
        </Group>
      </div>
    </Container>
  );
};
