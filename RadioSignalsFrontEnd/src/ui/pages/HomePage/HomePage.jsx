import React from 'react';
import useHome from "../../../hooks/useHome.js";
import {
    Box,
    Button,
    CircularProgress,
    Tabs,
    Tab,
    Typography, Container,
} from "@mui/material";

const HomePage = () => {
    const {homePage, loading} = useHome();

    return (
        <Box sx={{m: 0, p: 0}}>
            <Container maxWidth="xl" sx={{mt: 3, py: 3}}>
                <Typography variant="h4" gutterBottom>
                    Radio signals analysis
                </Typography>
                {loading ? (
                    <Box className="progress-box">
                        <CircularProgress />
                    </Box>
                ) : (
                    <Typography></Typography>
                )}
            </Container>
        </Box>
    );
};

export default HomePage;