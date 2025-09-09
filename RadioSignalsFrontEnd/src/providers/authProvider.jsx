import React, {useEffect, useState} from 'react';
import AuthContext from "../contexts/authContext.js";

const decode = (jwtToken) => {
    try {
        return JSON.parse(atob(jwtToken.split(".")[1]));
    } catch (error) {
        console.log(error);
        return null;
    }
};

const AuthProvider = ({children}) => {
    const [state, setState] = useState({
        "user": null,
        "loading": true
    });

    const login = (jwtToken) => {
    const payload = decode(jwtToken);
    if (payload) {
        // Ensure roles are always an array
        const roles = Array.isArray(payload.role) ? payload.role : [payload.role];
        const user = {
            ...payload,
            roles: roles.filter(Boolean) // Filter out null/undefined roles
        };
        localStorage.setItem("token", jwtToken);
        setState({
            "user": user,
            "loading": false,
        });
    }
};

    const logout = () => {
        const jwtToken = localStorage.getItem("token");
        if (jwtToken) {
            localStorage.removeItem("token");
            setState({
                "user": null,
                "loading": false,
            });
        }
    };

    useEffect(() => {
        const jwtToken = localStorage.getItem("token");
        if (jwtToken) {
            const payload = decode(jwtToken);
            if (payload) {
                const roles = Array.isArray(payload.role) ? payload.role : [payload.role];
                const user = {
                ...payload,
                roles: roles.filter(Boolean)
                };
                setState({ "user": user, "loading": false });
            } else {
                setState({
                    "user": null,
                    "loading": false,
                });
            }
        } else {
            setState({
                "user": null,
                "loading": false,
            });
        }
    }, []);

    return (
        <AuthContext.Provider value={{login, logout, ...state, isLoggedIn: !!state.user}}>
            {children}
        </AuthContext.Provider>
    );
};

export default AuthProvider;