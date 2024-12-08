import React, { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { ROLES, useAuth } from "./common/authProvider/AuthProvider";
import { About } from "./features/about/About";
import { ContactUs } from "./features/contactUs/ContactUs";
import { Footer } from "./features/footer/Footer";
import { Header } from "./features/header/Header";
import { Navbar } from "./features/navbar/Navbar";

export const HomeRoute: React.FC = () => {
  const { userRole } = useAuth();
  const navigate = useNavigate();
  useEffect(() => {
    if (userRole === ROLES.ADMIN) {
      navigate("/admin-dashboard");
    }
  }, [userRole]);

  return (
    <>
      <Navbar onHomePage={true} />
      <Header />
      <section id="about">
        <About />
      </section>
      <section id="contact">
        <ContactUs />
      </section>
      <Footer />
    </>
  );
};
