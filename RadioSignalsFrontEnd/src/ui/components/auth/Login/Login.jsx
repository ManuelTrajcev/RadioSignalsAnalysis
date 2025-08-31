import React, { useState } from 'react';
import {
    Box, TextField, Button, Typography, Container, Paper, Snackbar, Alert
} from '@mui/material';
import { useNavigate } from 'react-router';
import userRepository from '../../../../repository/userRepository.js';
import useAuth from '../../../../hooks/useAuth.js';

const initialFormData = { username: '', password: '' };

export default function Login() {
    const navigate          = useNavigate();
    const { login }         = useAuth();
    const [formData, setFD] = useState(initialFormData);

    const [errorMsg, setError]     = useState('');
    const [openSnack, setOpen]     = useState(false);

    const handleChange = e => {
        const { name, value } = e.target;
        setFD(fd => ({ ...fd, [name]: value }));
    };

    const handleSubmit = () => {
        userRepository
            .login(formData)
            .then(res => {
                login(res.data.token);
                navigate('/');
            })
            .catch(err => {
                const msg =
                    err.response?.data
                    ?? err.response?.message
                    ?? 'Something went wrong.';

                setError(msg);
                setOpen(true);
            });
    };

    return (
        <Container maxWidth="sm">
            <Paper elevation={3} sx={{ p: 4, mt: 8 }}>
                <Typography variant="h5" align="center" gutterBottom>
                    Login
                </Typography>

                <Box component="form" autoComplete="off" onSubmit={e => { e.preventDefault(); handleSubmit(); }}>
                    <TextField
                        fullWidth label="Username" name="username" required margin="normal"
                        value={formData.username} onChange={handleChange}
                    />
                    <TextField
                        fullWidth label="Password" name="password" type="password" required margin="normal"
                        value={formData.password} onChange={handleChange}
                    />
                    <Button fullWidth variant="contained" sx={{ mt: 2 }} type="submit">
                        Login
                    </Button>
                    <Button fullWidth variant="outlined" sx={{ mt: 2 }} onClick={() => navigate('/register')}>
                        Register
                    </Button>
                </Box>
            </Paper>

            <Snackbar
                open={openSnack}
                autoHideDuration={4000}
                onClose={() => setOpen(false)}
                anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
            >
                <Alert severity="error" variant="filled" onClose={() => setOpen(false)}>
                    {errorMsg}
                </Alert>
            </Snackbar>
        </Container>
    );
}
