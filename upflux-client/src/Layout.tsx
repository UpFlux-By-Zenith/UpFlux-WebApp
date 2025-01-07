// src/components/Layout.tsx
import React from "react";
import { Navbar } from "./features/navbar/Navbar"; 
import { Footer } from "./features/footer/Footer"; 

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  return (
    <div>
      <Navbar onHomePage={false} />  
      <main>
        {/*Component Content will go here*/}
        {children}  
      </main>
      <Footer /> 
    </div>
  );
};

export default Layout;
