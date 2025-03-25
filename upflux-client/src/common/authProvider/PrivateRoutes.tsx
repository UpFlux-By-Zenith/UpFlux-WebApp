

import { Navigate, Outlet } from "react-router-dom";
import { ROLES, useAuth } from "./AuthProvider";

interface IPrivateRoutes {
    role: ROLES
}

export const PrivateRoutes = (props: IPrivateRoutes) => {

    const { role } = props

    const { isAuthenticated, userRole } = useAuth();
    return isAuthenticated && (role === userRole || role === ROLES.COMMON) ? <Outlet /> : <Navigate to="/login" />
}