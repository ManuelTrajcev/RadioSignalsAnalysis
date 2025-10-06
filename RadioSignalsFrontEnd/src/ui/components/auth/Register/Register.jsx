import React, {useState} from 'react';
import {
    Box, TextField, Button, Typography, Container, Paper, InputLabel, Select, MenuItem, FormControl
} from '@mui/material';
import userRepository from "../../../../repository/userRepository.js";
import {useNavigate} from "react-router-dom";

const initialFormData = {
    "name": "",
    "surname": "",
    "username": "",
    "password": "",
    "email": "",
    "repeatPassword": "",
    "role": "",
};

const Register = () => {
    const navigate = useNavigate();

    const [formData, setFormData] = useState(initialFormData);

    const handleChange = (event) => {
        const {name, value} = event.target;
        setFormData({...formData, [name]: value});
    };

    const handleSubmit = () => {
        userRepository
            .register(formData)
            .then(() => {
                console.log("The user is successfully registered.");
                setFormData(initialFormData);
                navigate("/login");
            })
            .catch((error) => console.log(error));
    };

    return (
        <Container maxWidth="sm">
            <Paper elevation={3} sx={{padding: 4, mt: 4}}>
                <Typography variant="h5" align="center" gutterBottom>Регистрација</Typography>
                <Box>
                    <TextField
                        fullWidth label="Име"
                        name="name"
                        margin="normal"
                        required
                        value={formData.name}
                        onChange={handleChange}
                    />
                    <TextField
                        fullWidth label="Презиме"
                        name="surname"
                        margin="normal"
                        required
                        value={formData.surname}
                        onChange={handleChange}
                    />
                    <TextField
                        fullWidth label="Корисничко име"
                        name="username"
                        margin="normal"
                        required
                        value={formData.username}
                        onChange={handleChange}
                    />
                    <TextField
                        fullWidth label="Е-пошта"
                        name="email"
                        margin="normal"
                        required
                        value={formData.email}
                        onChange={handleChange}
                    />
                    <TextField
                        fullWidth label="Лозинка"
                        name="password"
                        type="password"
                        margin="normal"
                        required
                        value={formData.password}
                        onChange={handleChange}
                    />
                    <TextField
                        fullWidth label="Повтори лозинка"
                        name="repeatPassword"
                        type="password"
                        margin="normal"
                        required
                        value={formData.repeatPassword}
                        onChange={handleChange}
                    />
                    <FormControl fullWidth margin="dense" required>
                        <InputLabel>Улога</InputLabel>
                        <Select
                            name="role"
                            label="Улога"
                            variant="outlined"
                            value={formData.role}
                            onChange={handleChange}
                        >
                            <MenuItem key="user" value="0">Корисник</MenuItem>
                            <MenuItem key="admin" value="1">Администратор</MenuItem>
                        </Select>
                    </FormControl>
                    <Button fullWidth variant="contained" type="submit" sx={{mt: 2}} onClick={handleSubmit}>
                        Регистрирај се
                    </Button>
                </Box>
            </Paper>
        </Container>
    );
};

export default Register;