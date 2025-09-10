import React from 'react';
import {Link} from "react-router-dom";
import {AppBar, Box, Button, Toolbar, Typography} from "@mui/material";
import "./Header.css";
import AuthenticationToggle from "../../auth/AuthenticationToggle/AuthenticationToggle.jsx";
import useAuth from "../../../../hooks/useAuth.js";

const Header = () => {
    const { isLoggedIn } = useAuth();

    const pages = [
        {"path": "/", "name": "Home"},
        // Only show app pages when logged in (navigation-safe; routes are still protected)
        ...(isLoggedIn ? [
            {"path": "/data-entry", "name": "Data Entry"},
            {"path": "/measurements", "name": "Measurements"},
            {"path": "/workspaces", "name": "Workspaces"},
        ] : []),
    ];

    return (
        <Box>
            <AppBar position="static" color="error">
                <Toolbar sx={{ color: 'black' }}>
                    <Typography variant="h6" component="div" sx={{mr: 3}}>
                        Radio signals analysis
                    </Typography>
                    <Box sx={{flexGrow: 1, display: {xs: "none", md: "flex"}}}>
                        {pages.map((page) => (
                            <Link key={page.name} to={page.path}>
                                <Button
                                    sx={{my: 2, color: "black", display: "block", textDecoration: "none"}}
                                >
                                    {page.name}
                                </Button>
                            </Link>
                        ))}
                    </Box>
                    <AuthenticationToggle/>
                </Toolbar>
            </AppBar>
        </Box>
    );
};

export default Header;
