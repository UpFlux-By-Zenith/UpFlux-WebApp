import React, { useEffect, useState } from "react";
import { Container, Image, Menu, Text, Group, Avatar } from "@mantine/core";
import { Link } from "react-router-dom";
import logo from "../../assets/logos/logo-light-large.png";
import "./navbar.css";
import { useAuth } from "../../common/authProvider/AuthProvider";

export const BlankNavbar: React.FC = () => {

  return (
    <Container fluid className="navbar">
      <div className="navbar-logo">
        {(
          <>
            <Image src={logo} alt="Logo" className="logo" />
          </>
        )}
      </div>
    </Container>
  );
};
