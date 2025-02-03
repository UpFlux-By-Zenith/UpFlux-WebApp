import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { createTheme, MantineProvider } from '@mantine/core';
import { HomeRoute } from './HomeRoute';
import { LoginComponent } from './features/login/Login';
import { AdminLogin } from './features/adminLogin/AdminLogin';
import { PrivateRoutes } from './common/authProvider/PrivateRoutes';
import { AuthProvider, ROLES } from './common/authProvider/AuthProvider';
import { AdminDashboard } from './features/adminDashboard/AdminDashboard';
import { Clustering } from './features/clustering/Clustering';
import { PasswordSettingsContent } from './features/passwordSettings/PasswordSettings';
import Layout from './Layout';
import { UpdateManagement } from './features/updateManagement/UpdateManagement';
import { VersionControl } from './features/versionControl/VersionControl';
import { ClusterManagement } from './features/clusterManagement/ClusterManagement';
import { useState } from 'react';
import { AccountSettings } from './features/accountSettings/AccountSettings';
import { ForgotPassword } from './features/forgotPassword/ForgotPassword';

// Import your SessionTimeout component
import SessionTimeout from './features/sessionTimeout/SessionTimeout';
import { Provider } from 'react-redux';
import store from './features/reduxSubscription/store';

export const App = () => {

  // This function is called when the session times out.
  const handleLogout = () => {
    sessionStorage.removeItem('authToken');
    window.location.href = '/login';
  };


  return (
    <Provider store={store}>

      <MantineProvider >
        <AuthProvider>
          <Router>
            <Routes>
              {/* Public Routes - No session timeout here */}
              <Route path="/" element={<HomeRoute />} />
              <Route path="/login" element={<LoginComponent />} />
              <Route path="/admin-login" element={<AdminLogin />} />
              <Route path="/forgot-password" element={<ForgotPassword />} />

              {/* Protected Routes - Wrap with SessionTimeout */}
              <Route
                path="/*"
                element={
                  <SessionTimeout onLogout={handleLogout}>
                    <ProtectedRoutes />
                  </SessionTimeout>
                }
              />
            </Routes>
          </Router>
        </AuthProvider>
      </MantineProvider>
    </Provider>
  );
};

// Protected routes component
const ProtectedRoutes = () => (
  <Routes>
    {/* Admin Protected Routes */}
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

    <Route
      path="/update-management"
      element={
        <Layout>
          <UpdateManagement />
        </Layout>
      }
    />

    {/* Engineer Protected Routes */}
    <Route element={<PrivateRoutes role={ROLES.ENGINEER} />}>

      <Route
        path="/clustering"
        element={
          <Layout>
            <Clustering />
          </Layout>
        }
      />
      <Route
        path="/version-control"
        element={
          <Layout>
            <VersionControl />
          </Layout>
        }
      />
      <Route
        path="/cluster-management"
        element={
          <Layout>
            <ClusterManagement />
          </Layout>
        }
      />
      <Route
        path="/account-settings"
        element={
          <Layout>
            <AccountSettings />
          </Layout>
        }
      />
    </Route>
  </Routes>
);
