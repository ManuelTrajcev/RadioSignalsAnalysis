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
            localStorage.setItem("token", jwtToken);
            setState({
                "user": payload,
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
                setState({
                    "user": payload,
                    "loading": false,
                });
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