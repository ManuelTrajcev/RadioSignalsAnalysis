import React, {useState} from 'react';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControl,
    InputLabel, MenuItem, Select,
    TextField
} from "@mui/material";


const AccessWorkspaceDialog = ({open, onClose, workspace}) => {

    return (
        <Dialog open={open} onClose={onClose}>
            <DialogTitle>Workspace Info</DialogTitle>
            <DialogContent>
                <TextField
                    margin="dense"
                    label="Name"
                    name="name"
                    value={workspace.name}
                    // disabled={true}
                    fullWidth
                />
                <TextField
                    margin="dense"
                    label="Description"
                    name="description"
                    value={workspace.description}
                    //disabled={true}
                    fullWidth
                />

            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>Cancel</Button>
            </DialogActions>
        </Dialog>
    );
};

export default AccessWorkspaceDialog;