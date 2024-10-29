import React from 'react';
import './footer.css';

const Footer = () => {
  return (
    <div className="footer">
      <p className="contact-us">Contact Us</p>
      <hr className="footer-line" />
      <div className="footer-bottom">
        <p className="copyright">Copyright © 2024 UpFlux all rights reserved</p>
        <div className="footer-links">
          <a href="/privacy-policy" className="footer-link">Privacy Policy</a>
          <a href="/terms-conditions" className="footer-link">Terms and Conditions</a>
        </div>
      </div>
    </div>
  );
};

export default Footer;
