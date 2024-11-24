import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { MantineProvider } from '@mantine/core';
import { HomeRoute } from './routes/HomeRoute';
import { LoginRoute } from './routes/LoginRoute';
import { PasswordSettingsRoute } from './routes/PasswordSettingsRoute';

export const App = () => {
  return (
    <MantineProvider>
      <Router>
        <Routes>
          {/* Home route */}
          <Route path="/" element={<HomeRoute />} />

          {/* Login route */}
          <Route path="/login" element={<LoginRoute />} />

          {/* Password settings route */}
          <Route path="/password-settings" element={<PasswordSettingsRoute />} />
        </Routes>
      </Router>
    </MantineProvider>
  );
};
