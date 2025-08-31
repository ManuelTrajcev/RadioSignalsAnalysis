import React, {useState} from 'react';
import {
    Box, TextField, Button, Typography, Container, Paper,
    InputLabel, Select, MenuItem, FormControl, Snackbar, Alert
} from '@mui/material';
import {useNavigate} from 'react-router';
import userRepository from '../../../../repository/userRepository.js';

const initialFormData = {
    name: '', surname: '', username: '',
    password: '', repeatPassword: '', email: '', role: ''
};

export default function Register() {
    const navigate = useNavigate();
    const [formData, setFD] = useState(initialFormData);

    const [errorMsg, setErr] = useState('');
    const [open, setOpen] = useState(false);

    const handleChange = e => {
        const {name, value} = e.target;
        setFD(fd => ({...fd, [name]: value}));
    };

    const handleSubmit = () => {
        userRepository
            .register(formData)
            .then(() => {
                setFD(initialFormData);
                navigate('/login');
            })
            .catch(err => {
                const msg =
                    err.response?.data ??
                    err.response?.statusText ??
                    'Registration failed.';
                setErr(msg);
                setOpen(true);
            });
    };

    return (
        <Container maxWidth="sm">
            <Paper elevation={3} sx={{p: 4, mt: 4}}>
                <Typography variant="h5" align="center" gutterBottom>
                    Register
                </Typography>

                <Box component="form" autoComplete="off" onSubmit={e => {
                    e.preventDefault();
                    handleSubmit();
                }}>
                    <TextField fullWidth label="Name" name="name" margin="normal" required
                               value={formData.name} onChange={handleChange}/>
                    <TextField fullWidth label="Surname" name="surname" margin="normal" required
                               value={formData.surname} onChange={handleChange}/>
                    <TextField fullWidth label="Username" name="username" margin="normal" required
                               value={formData.username} onChange={handleChange}/>
                    <TextField fullWidth label="Email" name="email" margin="normal" required
                               value={formData.email} onChange={handleChange}/>
                    <TextField fullWidth label="Password" name="password" type="password"
                               margin="normal" required value={formData.password}
                               onChange={handleChange}/>
                    <TextField fullWidth label="Repeat Password" name="repeatPassword" type="password"
                               margin="normal" required value={formData.repeatPassword}
                               onChange={handleChange}/>
                    <FormControl fullWidth margin="dense" required>
                        <InputLabel>Role</InputLabel>
                        <Select name="role" label="Role" value={formData.role} onChange={handleChange}>
                            <MenuItem value="0">User</MenuItem>
                            <MenuItem value="1">Administrator</MenuItem>
                        </Select>
                    </FormControl>
                    <Button fullWidth variant="contained" sx={{mt: 2}} type="submit">
                        Register
                    </Button>
                </Box>
            </Paper>

            <Snackbar
                open={open}
                autoHideDuration={4000}
                onClose={() => setOpen(false)}
                anchorOrigin={{vertical: 'bottom', horizontal: 'center'}}
            >
                <Alert severity="error" variant="filled" onClose={() => setOpen(false)}>
                    {errorMsg}
                </Alert>
            </Snackbar>
        </Container>
    );
}
