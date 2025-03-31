import { createContext, ReactNode, useContext, useState } from "react";

export enum ROLES {
    GUEST,
    ENGINEER,
    COMMON,
    ADMIN
}

interface IContext {
    isAuthenticated: boolean;
    login: (arg0: ROLES, arg1: string) => void;
    logout: () => void;
    userRole: ROLES;
    authToken: string;
}

interface AuthProviderProps {
    children: ReactNode;
}

const AuthContext = createContext<IContext>({
    isAuthenticated: false,
    userRole: ROLES.GUEST,
    login: () => {
        console.warn("login function is not implemented");
    },
    logout: () => {
        console.warn("logout function is not implemented");
    },
    authToken: ""
});

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
    const [userRole, setUserRole] = useState<ROLES>(() => {
        const storedRole = sessionStorage.getItem("userRole");
        return storedRole ? parseInt(storedRole) : ROLES.GUEST;
    });
    const [authToken, setAuthToken] = useState<string>(() => {
        return sessionStorage.getItem("authToken") || "";
    });

    const login = (role: ROLES, token: string) => {
        setAuthToken(token);
        setUserRole(role);
        // Save to sessionStorage
        sessionStorage.setItem("authToken", token);
        sessionStorage.setItem("userRole", role.toString());
    };

    const logout = () => {
        setAuthToken("");
        setUserRole(ROLES.GUEST);

        // Remove from sessionStorage
        sessionStorage.removeItem("authToken");
        sessionStorage.removeItem("userRole");

        // Refresh the page
        window.location.reload();
    };


    const isAuthenticated: boolean = !!authToken;

    return (
        <AuthContext.Provider
            value={{ isAuthenticated, login, logout, userRole, authToken }}
        >
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error("useAuth must be used within an AuthProvider");
    }
    return context;
};
