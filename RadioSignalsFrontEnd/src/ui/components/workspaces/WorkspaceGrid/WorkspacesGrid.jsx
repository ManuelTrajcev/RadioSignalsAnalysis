import React from 'react';
import ProductCard from "../WorkspaceCard/WorkspaceCard.jsx";
import {Grid} from "@mui/material";
import WorkspaceCard from "../WorkspaceCard/WorkspaceCard.jsx";

const WorkspacesGrid = ({workspaces, onEdit, onAccess}) => {
    return (
        <Grid container spacing={{xs: 2, md: 3}}>
            {workspaces.map((workspace) => (
                <Grid key={workspace.id} size={{xs: 12, sm: 12, md: 12, lg: 12}}>
                    <WorkspaceCard workspace={workspace} onEdit={onEdit} onAccess={onAccess}/>
                </Grid>
            ))}
        </Grid>
    );
};

export default WorkspacesGrid;