import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.jsx'
import AuthProvider from "./providers/authProvider.jsx";
import {ThemeProvider} from "@mui/material";
import theme from './theme';


createRoot(document.getElementById('root')).render(
  <StrictMode>
    <AuthProvider>
        <ThemeProvider theme={theme}>
        <App />
        </ThemeProvider>
    </AuthProvider>
  </StrictMode>,
);
