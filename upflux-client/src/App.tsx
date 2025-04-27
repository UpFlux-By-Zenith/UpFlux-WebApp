import { MantineProvider } from '@mantine/core';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import './App.css';
import { HomeRoute } from './HomeRoute';
import Layout from './Layout';
import { AuthProvider, ROLES } from './common/authProvider/AuthProvider';
import { PrivateRoutes } from './common/authProvider/PrivateRoutes';
import { AdminLogin } from './features/adminLogin/AdminLogin';
import { ClusterManagement } from './features/clusterManagement/ClusterManagement';
import { ForgotPassword } from './features/forgotPassword/ForgotPassword';
import { LoginComponent } from './features/login/Login';

// Import your SessionTimeout component
import { Provider } from 'react-redux';
import { CommonDashboard } from './features/commonDashboard/CommonDashboard';
import store from './features/reduxSubscription/store';
import SessionTimeout from './features/sessionTimeout/SessionTimeout';
import '@mantine/charts/styles.css';
import { MachineDetails } from './features/guestPage/MachineDetails';
import MachineDetailsRoute from './features/guestPage/MachineDetailsRoute';
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
              <Route path="/machine-details" element={<MachineDetailsRoute />} />

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
    <Route element={<PrivateRoutes role={ROLES.COMMON} />}>
      <Route path="/dashboard" element={<Layout> <CommonDashboard /></Layout>}></Route>
    </Route>
    <Route
      path="/cluster-management"
      element={
        <Layout>
          <ClusterManagement />
        </Layout>
      }
    />
    {/* Admin Protected Routes */}
    {/* <Route element={<PrivateRoutes role={ROLES.ADMIN} />}>
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
    </Route> */}

    {/* <Route
      path="/update-management"
      element={
        <Layout>
          <UpdateManagement />
        </Layout>
      }
    /> */}

    {/* Engineer Protected Routes */}
    <Route element={<PrivateRoutes role={ROLES.ENGINEER} />}>

      {/* <Route
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
      /> */}
    </Route>
  </Routes>
);
