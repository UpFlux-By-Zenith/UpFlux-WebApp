import React, { useState } from "react";
import { Navbar } from "./features/navbar/Navbar"; 
import { Footer } from "./features/footer/Footer"; 

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  const [notifications, setNotifications] = useState<any[]>([]);

  const addNotification = (newNotification: any) => {
    setNotifications((prevNotifications) => [...prevNotifications, newNotification]);
  };

  return (
    <div>
      <Navbar onHomePage={false} notifications={notifications} />  
      <main>
        {/* Pass addNotification to child components that need it */}
        {React.Children.map(children, (child) => {
          if (React.isValidElement(child) && child.props) {
            return React.cloneElement(child as React.ReactElement<any>, { addNotification });
          }
          return child;
        })}
      </main>
      <Footer /> 
    </div>
  );
};

export default Layout;
