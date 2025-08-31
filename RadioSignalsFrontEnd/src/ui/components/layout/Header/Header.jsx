import React from 'react';
import {Link} from "react-router";
import {AppBar, Box, Button, IconButton, Toolbar, Typography} from "@mui/material";
import MenuIcon from '@mui/icons-material/Menu';
import "./Header.css";
import AuthenticationToggle from "../../auth/AuthenticationToggle/AuthenticationToggle.jsx";

const pages = [
    {"path": "/", "name": "home"},
    {"path": "/workspaces", "name": "workspaces"},
];

const Header = () => {
    return (
        <Box>
            <AppBar position="static" color="error">
                <Toolbar sx={{ color: 'black' }}>
                    {/*<IconButton*/}
                    {/*    size="large"*/}
                    {/*    edge="start"*/}
                    {/*    color="inherit"*/}
                    {/*    aria-label="menu"*/}
                    {/*    sx={{mr: 2}}*/}
                    {/*>*/}
                    {/*    <MenuIcon/>*/}
                    {/*</IconButton>*/}
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