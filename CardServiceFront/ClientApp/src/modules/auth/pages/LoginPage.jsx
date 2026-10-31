import { useState } from 'react';
import { Link as RouterLink, useLocation, useNavigate } from 'react-router-dom';
import { Alert, Box, Button, CircularProgress, Link, Stack, TextField, Typography } from '@mui/material';
import ArrowForwardRounded from '@mui/icons-material/ArrowForwardRounded';
import LockOutlined from '@mui/icons-material/LockOutlined';
import { AccessLayout } from '../../../shared/layouts/AccessLayout';
import { PasswordField } from '../../../shared/components/PasswordField';
import { useAuth } from '../context/AuthContext';

export function LoginPage() {
  const location = useLocation();
  const [email, setEmail] = useState(location.state?.email || '');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState({});
  const [message, setMessage] = useState('');
  const [busy, setBusy] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();
  const submit = async event => {
    event.preventDefault();
    if (busy) return;
    const validation = {};
    if (!email.trim() || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) validation.email = 'Escribe un correo válido.';
    if (!password) validation.password = 'Escribe tu contraseña.';
    setErrors(validation); setMessage('');
    if (Object.keys(validation).length) return;
    setBusy(true);
    try { await login({ email: email.trim(), password }); setPassword(''); navigate('/mi-perfil', { replace: true }); }
    catch (error) { setMessage(error.status === 401 ? 'El correo o la contraseña no son correctos. Revisa tus datos.' : error.message); }
    finally { setBusy(false); }
  };
  return <AccessLayout mode="login">
    <div className="step-line"><span className="welcome-icon"><LockOutlined sx={{ fontSize: 19 }} /></span><span>QUÉ BUENO TENERTE AQUÍ</span></div>
    <Typography component="h2" variant="h2" sx={{ mt: 3.5, mb: 1 }}>Tu espacio te espera.</Typography>
    <Typography color="text.secondary" sx={{ mb: 4 }}>Ingresa con tu correo y contraseña<br />para continuar donde lo dejaste.</Typography>
    <Stack spacing={2.5} component="form" onSubmit={submit} noValidate aria-label="Iniciar sesión">
      {location.state?.registered && <Alert severity="success" role="status">Tu perfil está creado. Ahora inicia sesión.</Alert>}
      {message && <Alert severity="error" role="alert">{message}</Alert>}
      <TextField label="Correo electrónico" name="email" type="email" autoComplete="username" required fullWidth
        value={email} onChange={event => { setEmail(event.target.value); setErrors(current => ({ ...current, email: '' })); }}
        error={!!errors.email} helperText={errors.email} disabled={busy} />
      <PasswordField name="password" autoComplete="current-password" required value={password}
        onChange={event => { setPassword(event.target.value); setErrors(current => ({ ...current, password: '' })); }}
        error={!!errors.password} helperText={errors.password} disabled={busy} />
      <Button type="submit" variant="contained" fullWidth size="large" disabled={busy}
        endIcon={busy ? <CircularProgress size={18} color="inherit" /> : <ArrowForwardRounded />}>
        {busy ? 'Ingresando…' : 'Iniciar sesión'}
      </Button>
    </Stack>
    <Box sx={{ mt: 3, p: 2, background: '#f8f8fa', borderRadius: 2 }}>
      <Typography variant="body2" color="text.secondary">¿Es tu primera vez? Crea un perfil con contraseña para poder ingresar.</Typography>
    </Box>
    <Typography variant="body2" sx={{ textAlign: 'center', mt: 4, pt: 3, borderTop: '1px solid', borderColor: 'divider' }}>
      Tu próximo paso empieza aquí. <Link component={RouterLink} to="/crear-perfil">Crear perfil</Link>
    </Typography>
  </AccessLayout>;
}
