import React from 'react';
import {Link} from "react-router-dom";
import {AppBar, Box, Button, Toolbar, Typography} from "@mui/material";
import "./Header.css";
import AuthenticationToggle from "../../auth/AuthenticationToggle/AuthenticationToggle.jsx";
import useAuth from "../../../../hooks/useAuth.js";

const Header = () => {
    const { isLoggedIn } = useAuth();

    const pages = [
        {"path": "/", "name": "Почетна"},
        ...(isLoggedIn ? [
            {"path": "/data-entry", "name": "Внес на податоци"},
            {"path": "/measurements", "name": "Мерења"},
            {"path": "/map", "name": "Мапа"},
            {"path": "/settlement", "name": "Населено место"}
        ] : []),
    ];

    return (
        <Box>
            <AppBar position="static" color="error">
                <Toolbar sx={{ color: 'black' }}>
                    <Typography variant="h6" component="div" sx={{mr: 3}}>
                        Анализа на радио сигнали
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
