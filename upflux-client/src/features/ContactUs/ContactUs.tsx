import React, { ChangeEvent, FormEvent, useState } from 'react';
import { Container, TextInput, Textarea, Button, Image, Title, Text } from '@mantine/core';
import './contactUs.css';
import phoneIcon from '../../assets/images/phone.png';
import emailIcon from '../../assets/images/email.png';

// Define an interface for the contact form data
interface ContactForm {
  name: string;
  email: string;
  message: string;
}

export const ContactUs = () => {
  const [formData, setFormData] = useState<ContactForm>({
    name: '',
    email: '',
    message: '',
  });

  const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prevData) => ({
      ...prevData,
      [name]: value,
    }));
  };

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    console.log('Form submitted:', formData);
    setFormData({ name: '', email: '', message: '' });
  };

  return (
    <section id="contact" className="contact section light-background">
      <Container className="contactContainer">
        <div className="contact-header">
          <Title order={2}>Contact Us</Title>
        </div>
        <div className="contact-content">
          <div className="info-boxes">
            <div className="info-item">
              <Image src={phoneIcon} alt="Call Us" className="info-icon" />
              <Title order={3}>Call Us</Title>
              <Text>+353 5589 55488 55</Text>
              <Text>+353 6678 254445 41</Text>
            </div>

            <div className="info-item">
              <Image src={emailIcon} alt="Email Us" className="info-icon" />
              <Title order={3}>Email Us</Title>
              <Text>support@upflux.com</Text>
              <Text>contact@upflux.com</Text>
            </div>
          </div>

          <div className="contact-form">
            <form onSubmit={handleSubmit}>
              <div className="form-group name-email-group">
                <TextInput
                  label="Name"
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleChange}
                  required
                  className="name-group"
                />
                <TextInput
                  label="Email"
                  id="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                  className="email-group"
                />
              </div>
              <div className="form-group message-group">
                <Textarea
                  label="Message"
                  id="message"
                  name="message"
                  value={formData.message}
                  onChange={handleChange}
                  required
                  autosize
                  minRows={4}
                  className="message-group"
                />
              </div>
              <Button type="submit" className="submit-button">
                Send Message
              </Button>
            </form>
          </div>
        </div>
      </Container>
    </section>
  );
};
