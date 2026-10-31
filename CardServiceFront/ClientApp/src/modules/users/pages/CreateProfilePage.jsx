import { useState } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { Alert, Box, Button, CircularProgress, Link, Stack, TextField, Typography } from '@mui/material';
import ArrowForwardRounded from '@mui/icons-material/ArrowForwardRounded';
import CheckCircleOutlineRounded from '@mui/icons-material/CheckCircleOutlineRounded';
import { AccessLayout } from '../../../shared/layouts/AccessLayout';
import { PasswordField } from '../../../shared/components/PasswordField';
import { usersApi } from '../services/usersApi';
import { validateProfile } from '../validation/profileValidation';

export function CreateProfilePage() {
  const [values, setValues] = useState({ fullName: '', email: '', password: '', confirmation: '' });
  const [errors, setErrors] = useState({});
  const [message, setMessage] = useState('');
  const [busy, setBusy] = useState(false);
  const navigate = useNavigate();
  const change = name => event => {
    setValues(current => ({ ...current, [name]: event.target.value }));
    setErrors(current => ({ ...current, [name]: '' })); setMessage('');
  };
  const submit = async event => {
    event.preventDefault();
    if (busy) return;
    const validation = validateProfile(values); setErrors(validation); setMessage('');
    if (Object.keys(validation).length) return;
    setBusy(true);
    try {
      const user = await usersApi.create({ fullName: values.fullName.trim(), email: values.email.trim(), password: values.password });
      setValues(current => ({ ...current, password: '', confirmation: '' }));
      navigate('/ingresar', { replace: true, state: { registered: true, email: user.email } });
    } catch (error) {
      if (error.status === 409) setErrors({ email: 'Este correo ya tiene un perfil. Puedes iniciar sesión.' });
      else {
        const mapped = {};
        for (const [key, messages] of Object.entries(error.errors || {}))
          mapped[key.charAt(0).toLowerCase() + key.slice(1)] = messages[0];
        setErrors(mapped); setMessage(error.message);
      }
    } finally { setBusy(false); }
  };
  return <AccessLayout mode="register">
    <div className="step-line"><span className="step-pill">01</span><span>CREA TU PERFIL</span><span className="step-connector" /><span className="step-muted">02 &nbsp; INICIA SESIÓN</span></div>
    <Typography component="h2" variant="h2" sx={{ mt: 3.5, mb: 1 }}>Todo empieza contigo.</Typography>
    <Typography color="text.secondary" sx={{ mb: 3.5 }}>Crea tu perfil y da el primer paso.<br /></Typography>
    <Box component="form" onSubmit={submit} noValidate aria-label="Crear perfil">
      <Stack spacing={2.5}>
        {message && <Alert severity="error" role="alert">{message}</Alert>}
        <TextField label="Nombre completo" name="fullName" autoComplete="name" fullWidth required
          value={values.fullName} onChange={change('fullName')} error={!!errors.fullName} helperText={errors.fullName}
          disabled={busy} slotProps={{ htmlInput: { maxLength: 150 } }} />
        <TextField label="Correo electrónico" name="email" type="email" autoComplete="email" fullWidth required
          value={values.email} onChange={change('email')} error={!!errors.email} helperText={errors.email}
          disabled={busy} slotProps={{ htmlInput: { maxLength: 254 } }} />
        <PasswordField name="password" autoComplete="new-password" required disabled={busy}
          value={values.password} onChange={change('password')} error={!!errors.password}
          helperText={errors.password || 'Usa al menos 12 caracteres. Una frase funciona muy bien.'} />
        <PasswordField label="Confirmar contraseña" name="confirmation" autoComplete="new-password" required disabled={busy}
          value={values.confirmation} onChange={change('confirmation')} error={!!errors.confirmation} helperText={errors.confirmation} />
        <Button type="submit" variant="contained" size="large" fullWidth disabled={busy}
          endIcon={busy ? <CircularProgress size={18} color="inherit" /> : <ArrowForwardRounded />}>
          {busy ? 'Creando tu perfil…' : 'Crear mi perfil'}
        </Button>
      </Stack>
    </Box>
    <Stack direction="row" spacing={.8} sx={{ alignItems: 'flex-start', mt: 2.3, color: 'text.secondary' }}>
      <CheckCircleOutlineRounded sx={{ fontSize: 16, mt: '.2rem', color: '#23775a' }} />
      <Typography variant="body2" sx={{ fontSize: '.75rem' }}>Tu contraseña se protege y nunca se muestra en tu perfil.</Typography>
    </Stack>
    <Typography variant="body2" sx={{ textAlign: 'center', mt: 3.5, pt: 3, borderTop: '1px solid', borderColor: 'divider' }}>
      ¿Ya eres parte? <Link component={RouterLink} to="/ingresar">Inicia sesión</Link>
    </Typography>
  </AccessLayout>;
}
