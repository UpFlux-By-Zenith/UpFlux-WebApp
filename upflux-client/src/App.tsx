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
import { UpdateManagement } from './features/updateManagement/UpdateManagement';
import { VersionControl } from './features/versionControl/VersionControl';
import { ClusterManagement } from './features/clusterManagement/ClusterManagement';
import { useState } from 'react';
import { Provider } from "react-redux";
import store from "./features/reduxSubscription/store";

export const App = () => {
  return (
    <Provider store={store}>
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

              {/* Engineer protected routes */}
              <Route element={<PrivateRoutes role={ROLES.ENGINEER} />}>
                <Route
                  path="/update-management"
                  element={
                    <Layout>
                      <UpdateManagement />
                    </Layout>
                  }
                />
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
              </Route>
            </Routes>
          </Router>
        </AuthProvider>
      </MantineProvider>
    </Provider>
  );
};
