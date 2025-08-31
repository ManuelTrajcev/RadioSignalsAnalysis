import React from 'react';
import {Button} from "@mui/material";
import useAuth from "../../../../hooks/useAuth.js";
import {useNavigate} from "react-router";

const AuthenticationToggle = () => {
    const {logout, isLoggedIn} = useAuth();

    const navigate = useNavigate();

    const handleLogin = () => {
        navigate("/login");
    };

    const handleLogout = () => {
        logout();
        navigate("/login");
    };

    return (
        <Button
            color="inherit"
            variant={!isLoggedIn ? "text" : "outlined"}
            onClick={!isLoggedIn ? handleLogin : handleLogout}
            sx={{borderRadius: 0, border: 2}}
        >
            {!isLoggedIn ? "Login" : "Logout"}
        </Button>
    );
};

export default AuthenticationToggle;