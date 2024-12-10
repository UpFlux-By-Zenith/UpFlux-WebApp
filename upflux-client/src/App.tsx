import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { MantineProvider } from '@mantine/core';
import { HomeRoute } from './HomeRoute';
import { LoginComponent } from './features/login/Login';
import { PasswordSettingsRoute } from './features/passwordSettings/PasswordSettingsRoute';
import { AdminLogin } from './features/adminLogin/AdminLogin';
import { PrivateRoutes } from './common/authProvider/PrivateRoutes';
import { AuthProvider, ROLES } from './common/authProvider/AuthProvider';
import { AdminDashboard } from './features/adminDashboard/AdminDashboard';
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

          {/* Get Engineer Token route */}
          <Route path="/admin-login" element={<AdminLogin />} />
          
          <Route element={<PrivateRoutes role={ROLES.ADMIN} />} >
            {/* Password settings route */}
            <Route path="/password-settings" element={<PasswordSettingsRoute />} />

            {/* Get Engineer Token route */}
            <Route path="/admin-dashboard" element={<AdminDashboard />} />
          </Route>
          <Route element={<PrivateRoutes role={ROLES.ENGINEER} />} >

          {/* Update Management route */}
          <Route path="/update-management" element={<UpdateManagementRoute />} />

          </Route>
        </Routes>
      </Router>
      </AuthProvider>
    </MantineProvider>
  );
};
