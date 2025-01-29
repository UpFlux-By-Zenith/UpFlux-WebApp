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

export const ADMIN_REQUEST_API = {
  GET_ALL_MACHINES:`${API_BASE_URL}/api/auth/admin/get-all-machines`
}

//DataRequest-related endpoints
export const DATA_REQUEST_API = {
  GET_ACCESS_MACHINES: `${API_BASE_URL}/api/DataRequest/engineer/access-machines`,
  GET_MACHINE_DETAILS: `${API_BASE_URL}/api/DataRequest/applications`,
};


