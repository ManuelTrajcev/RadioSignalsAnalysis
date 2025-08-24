import React, {useState} from 'react';
import {
    Box,
    Button,
    CircularProgress,
    Tabs,
    Tab,
    Typography,
} from "@mui/material";
import useHome from "../../../hooks/useHome.js";
// import WorkspacesGrid from "../../components/workspaces/WorkspaceGrid/WorkspacesGrid.jsx";

const HomePage = () => {
    const {homePage, loading} = useHome();
    const [selectedTab, setSelectedTab] = useState(0); // 0 = All, 1 = Mine

    const handleTabChange = (event, newValue) => {
        setSelectedTab(newValue);
    };

    // const workspacesToShow = selectedTab === 0 ? allWorkspaces : myWorkspaces;

    return (
        <Box className="products-box">
            <Tabs value={selectedTab} onChange={handleTabChange} sx={{ mb: 2 }}>
                <Tab label="All Workspaces" />
                <Tab label="My Workspaces" />
            </Tabs>

            {loading ? (
                <Box className="progress-box">
                    <CircularProgress />
                </Box>
            ) : (
                <h1>{homePage}</h1>
            )}
        </Box>
    );
};

export default HomePage;
