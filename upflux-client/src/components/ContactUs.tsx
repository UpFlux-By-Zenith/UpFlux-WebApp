import React, { useState } from 'react'; 
import '../features/ContactUs/contactUs.css';
import phoneIcon from '../assets/images/phone.png'; 
import emailIcon from '../assets/images/email.png'; 

const ContactUs = () => {
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    message: '',
  });

  const handleChange = (e: { target: { name: any; value: any; }; }) => {
    const { name, value } = e.target;
    setFormData((prevData) => ({
      ...prevData,
      [name]: value,
    }));
  };

  const handleSubmit = (e: { preventDefault: () => void; }) => {
    e.preventDefault();
    console.log('Form submitted:', formData);
    setFormData({ name: '', email: '', message: '' });
  };

  return (
    <section id="contact" className="contact section light-background">
      <div className="contactContainer">
      <div className="contact-header">
          <h2>Contact Us</h2>
        </div>
        <div className="contact-content">
          <div className="info-boxes">
            <div className="info-item">
              <img src={phoneIcon} alt="Call Us" className="info-icon" />
              <h3>Call Us</h3>
              <p>+353 5589 55488 55</p>
              <p>+353 6678 254445 41</p>
            </div>

            <div className="info-item">
              <img src={emailIcon} alt="Email Us" className="info-icon" />
              <h3>Email Us</h3>
              <p>support@upflux.com</p>
              <p>contact@upflux.com</p>
            </div>
          </div>

          <div className="contact-form">
            <form onSubmit={handleSubmit}>
              <div className="form-group name-email-group">
                <div className="name-group">
                  <label htmlFor="name">Name</label>
                  <input
                    type="text"
                    id="name"
                    name="name"
                    value={formData.name}
                    onChange={handleChange}
                    required
                  />
                </div>
                <div className="email-group">
                  <label htmlFor="email">Email</label>
                  <input
                    type="email"
                    id="email"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                    required
                  />
                </div>
              </div>
              <div className="form-group message-group">
                <label htmlFor="message">Message</label>
                <textarea
                  id="message"
                  name="message"
                  value={formData.message}
                  onChange={handleChange}
                  required
                ></textarea>
              </div>
              <button type="submit" className="submit-button">
                Send Message
              </button>
            </form>
          </div>
        </div>
      </div>
    </section>
  );
};

export default ContactUs;
