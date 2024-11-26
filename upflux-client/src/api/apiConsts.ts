const API_BASE_URL =
  process.env.REACT_APP_ENV === 'production'
    ? process.env.REACT_APP_PROD_API_BASE
    : process.env.REACT_APP_DEV_API_BASE;

export default API_BASE_URL;

// Auth-related endpoints
export const AUTH_API = {
  LOGIN: `${API_BASE_URL}/api/Auth/engineer/login`,
  CHANGE_PASSWORD: `${API_BASE_URL}/api/Auth/admin/change-password`,
  GET_ENGINEER_TOKEN: `${API_BASE_URL}/api/Auth/admin/create-engineer-token`,
  ADMIN_LOGIN: `${API_BASE_URL}/api/Auth/admin/login`,
  VERIFY_TOKEN: `${API_BASE_URL}/api/Auth/parse-token`,
};


