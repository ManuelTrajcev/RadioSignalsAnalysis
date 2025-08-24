// import React, {useState} from 'react';
// import {
//     Box, TextField, Button, Typography, Container, Paper, InputLabel, Select, MenuItem, FormControl
// } from '@mui/material';
// import userRepository from "../../../../repository/userRepository.js";
// import {useNavigate} from "react-router";
//
// const initialFormData = {
//     "name": "",
//     "surname": "",
//     "username": "",
//     "password": "",
//     "repeatPassword": "",
//     "role": "",
// };
//
// const Register = () => {
//     const navigate = useNavigate();
//
//     const [formData, setFormData] = useState(initialFormData);
//
//     const handleChange = (event) => {
//         const {name, value} = event.target;
//         setFormData({...formData, [name]: value});
//     };
//
//     const handleSubmit = () => {
//         userRepository
//             .register(formData)
//             .then(() => {
//                 console.log("The user is successfully registered.");
//                 setFormData(initialFormData);
//                 navigate("/login");
//             })
//             .catch((error) => console.log(error));
//     };
//
//     return (
//         <Container maxWidth="sm">
//             <Paper elevation={3} sx={{padding: 4, mt: 4}}>
//                 <Typography variant="h5" align="center" gutterBottom>Register</Typography>
//                 <Box>
//                     <TextField
//                         fullWidth label="Name"
//                         name="name"
//                         margin="normal"
//                         required
//                         value={formData.name}
//                         onChange={handleChange}
//                     />
//                     <TextField
//                         fullWidth label="Surname"
//                         name="surname"
//                         margin="normal"
//                         required
//                         value={formData.surname}
//                         onChange={handleChange}
//                     />
//                     <TextField
//                         fullWidth label="Username"
//                         name="username"
//                         margin="normal"
//                         required
//                         value={formData.username}
//                         onChange={handleChange}
//                     />
//                     <TextField
//                         fullWidth label="Password"
//                         name="password"
//                         type="password"
//                         margin="normal"
//                         required
//                         value={formData.password}
//                         onChange={handleChange}
//                     />
//                     <TextField
//                         fullWidth label="Repeat Password"
//                         name="repeatPassword"
//                         type="password"
//                         margin="normal"
//                         required
//                         value={formData.repeatPassword}
//                         onChange={handleChange}
//                     />
//                     <FormControl fullWidth margin="dense" required>
//                         <InputLabel>Role</InputLabel>
//                         <Select
//                             name="role"
//                             label="Role"
//                             variant="outlined"
//                             value={formData.role}
//                             onChange={handleChange}
//                         >
//                             <MenuItem key="user" value="ROLE_USER">User</MenuItem>
//                             <MenuItem key="admin" value="ROLE_ADMIN">Administrator</MenuItem>
//                         </Select>
//                     </FormControl>
//                     <Button fullWidth variant="contained" type="submit" sx={{mt: 2}} onClick={handleSubmit}>
//                         Register
//                     </Button>
//                 </Box>
//             </Paper>
//         </Container>
//     );
// };
//
// export default Register;