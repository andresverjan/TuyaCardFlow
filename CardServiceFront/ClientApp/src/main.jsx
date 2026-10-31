import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { CssBaseline, GlobalStyles, ThemeProvider } from '@mui/material';
import '@fontsource-variable/inter/wght.css';
import { APP_COLORS, theme } from './app/theme';
import { App } from './app/App';
import { AuthProvider } from './modules/auth/context/AuthContext';
import './shared/styles/global.css';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <ThemeProvider theme={theme}>
      <GlobalStyles styles={{ ':root': {
        '--color-primary': APP_COLORS.primary,
        '--color-secondary': APP_COLORS.secondary,
        '--color-light-secondary': APP_COLORS.lightSecondary,
      } }} />
      <CssBaseline />
      <BrowserRouter>
        <AuthProvider><App /></AuthProvider>
      </BrowserRouter>
    </ThemeProvider>
  </React.StrictMode>,
);
