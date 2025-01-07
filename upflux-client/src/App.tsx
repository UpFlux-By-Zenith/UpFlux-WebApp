import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { MantineProvider } from '@mantine/core';
import { HomeRoute } from './HomeRoute';
import { LoginComponent } from './features/login/Login';
import { AdminLogin } from './features/adminLogin/AdminLogin';
import { PrivateRoutes } from './common/authProvider/PrivateRoutes';
import { AuthProvider, ROLES } from './common/authProvider/AuthProvider';
import { AdminDashboard } from './features/adminDashboard/AdminDashboard';
import { Clustering } from './features/clustering/Clustering';
import { PasswordSettingsContent } from './features/passwordSettings/PasswordSettings';
import Layout from './Layout';
import { UpdateManagementRoute } from './features/updateManagement/UpdateManagementRoute';


export const App = () => {
  return (
    <MantineProvider>
      <AuthProvider>
        <Router>
          <Routes>
            {/* Home route */}
            <Route path="/" element={<HomeRoute />} />

            {/* Login route */}
            <Route path="/login" element={<LoginComponent />} />

            {/* Admin login route */}
            <Route path="/admin-login" element={<AdminLogin />} />

            {/* Admin protected routes */}
            <Route element={<PrivateRoutes role={ROLES.ADMIN} />}>
              <Route
                path="/password-settings"
                element={
                  <Layout>
                    <PasswordSettingsContent />
                  </Layout>
                }
              />
              <Route
                path="/admin-dashboard"
                element={
                  <Layout>
                    <AdminDashboard />
                  </Layout>
                }
              />
            </Route>

            {/* Get Engineer Token route */}
            <Route path="/admin-dashboard" element={<AdminDashboard />} />
          </Route>
          <Route element={<PrivateRoutes role={ROLES.ENGINEER} />} >

          {/* Update Management route */}
          <Route path="/update-management" element={<UpdateManagementRoute />} />

               <Route
                path="/clustering"
                element={
                  <Layout>
                    <Clustering />
                  </Layout>
                }
              />
              
          </Route>
        </Routes>
      </Router>
      </AuthProvider>
    </MantineProvider>
  );
};
