import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { MantineProvider } from '@mantine/core';
import { HomeRoute } from './HomeRoute';
import { LoginComponent } from './features/login/Login';
import { PasswordSettingsRoute } from './features/passwordSettings/PasswordSettingsRoute';
import { GetEngineerTokenRoute } from './features/getEngineerToken/GetEngineerTokenRoute';
import { AdminLogin } from './features/adminLogin/AdminLogin';

export const App = () => {
  return (
    <MantineProvider>
      <Router>
        <Routes>
          {/* Home route */}
          <Route path="/" element={<HomeRoute />} />

          {/* Login route */}
          <Route path="/login" element={<LoginComponent />} />

          {/* Password settings route */}
          <Route path="/password-settings" element={<PasswordSettingsRoute />} />

          {/* Get Engineer Token route */}
          <Route path="/admin-login" element={<AdminLogin />} />

          {/* Get Engineer Token route */}
          <Route path="/get-engineer-token" element={<GetEngineerTokenRoute />} />

        </Routes>
      </Router>
    </MantineProvider>
  );
};
