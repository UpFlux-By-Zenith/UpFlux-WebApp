import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { ROLES, useAuth } from "./common/authProvider/AuthProvider";
import { About } from "./features/about/About";
import { ContactUs } from "./features/contactUs/ContactUs";
import { Footer } from "./features/footer/Footer";
import { Header } from "./features/header/Header";
import { Navbar } from "./features/navbar/Navbar";
import { LearnMore } from "./features/learnMore/LearnMore";
import { OurSolution } from "./features/ourSolution/OurSolution";

export const HomeRoute: React.FC = () => {
  const { userRole } = useAuth();
  const navigate = useNavigate();
  useEffect(() => {
    if (userRole === ROLES.ADMIN) {
      navigate("/admin-dashboard");
    }
  }, [userRole]);
  const [notifications, setNotifications] = useState<any[]>([]);

  return (
    <>
      <Navbar onHomePage={true} notifications={notifications}/>
      <Header />
      <section id="learnMore">
        <LearnMore />
      </section>
      <section id="ourSolution">
        <OurSolution />
      </section>
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
