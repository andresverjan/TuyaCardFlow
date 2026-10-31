import { createTheme, darken, lighten } from '@mui/material/styles';

export const APP_COLORS = {
  primary: '#25272d',
  secondary: '#1347c0',
  lightSecondary: '#edf2ff',
};

export const theme = createTheme({
  palette: {
    primary: { main: APP_COLORS.primary, dark: darken(APP_COLORS.primary, 0.2), light: lighten(APP_COLORS.primary, 0.93) },
    secondary: { main: APP_COLORS.secondary, light: APP_COLORS.lightSecondary },
    text: { primary: '#25272d', secondary: '#73757f' },
    background: { default: '#fff', paper: '#fff' },
    success: { main: '#23775a' }, divider: '#ececf0',
  },
  typography: {
    fontFamily: '"Inter Variable", Inter, system-ui, sans-serif',
    h1: { fontSize: '2.5rem', fontWeight: 650, letterSpacing: '-.055em', lineHeight: 1.17 },
    h2: { fontSize: '1.9rem', fontWeight: 650, letterSpacing: '-.045em', lineHeight: 1.22 },
    h3: { fontSize: '1.15rem', fontWeight: 600, letterSpacing: '-.025em' },
    body1: { fontSize: '.94rem', lineHeight: 1.75 },
    body2: { fontSize: '.84rem', lineHeight: 1.7 },
    button: { textTransform: 'none', fontWeight: 600, letterSpacing: '-.015em' },
  },
  shape: { borderRadius: 12 },
  components: {
    MuiButton: { defaultProps: { disableElevation: true }, styleOverrides: { root: { minHeight: 48, borderRadius: 10 }, sizeLarge: { minHeight: 52, fontSize: '.91rem' } } },
    MuiOutlinedInput: { styleOverrides: { root: { borderRadius: 10, background: '#fff', fontSize: '.91rem', '& fieldset': { borderColor: '#dedee4' } }, input: { padding: '14px 15px' } } },
    MuiInputLabel: { styleOverrides: { root: { fontSize: '.9rem' } } },
    MuiAlert: { styleOverrides: { root: { fontSize: '.85rem', borderRadius: 10 } } },
    MuiLink: { defaultProps: { underline: 'hover' }, styleOverrides: { root: { fontWeight: 600 } } },
  },
});
