import { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Alert, Avatar, Box, Button, Chip, Container, Divider, Paper, Stack, Typography } from '@mui/material';
import LogoutRounded from '@mui/icons-material/LogoutRounded';
import CheckCircleOutlineRounded from '@mui/icons-material/CheckCircleOutlineRounded';
import CreditCardRounded from '@mui/icons-material/CreditCardRounded';
import PaymentsRounded from '@mui/icons-material/PaymentsRounded';
import PersonOutlineRounded from '@mui/icons-material/PersonOutlineRounded';
import LockOutlined from '@mui/icons-material/LockOutlined';
import { Brand } from '../../../shared/components/Brand';
import { useAuth } from '../../../modules/auth/context/AuthContext';

export function MyProfilePage() {
  const { user, logout } = useAuth();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const signOut = async () => {
    setBusy(true); setError('');
    try { await logout(); } catch (problem) { setError(problem.message); } finally { setBusy(false); }
  };
  return <Box sx={{ minHeight: '100vh', background: '#f7f8fa' }}>
    <header className="profile-header"><Brand /><Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
      <Button component={RouterLink} to="/tarjetas" color="inherit" startIcon={<CreditCardRounded />}>Administrar tarjetas</Button>
      <Button component={RouterLink} to="/pagos" color="inherit" startIcon={<PaymentsRounded />}>Pagos e historial</Button>
      <Button onClick={signOut} disabled={busy} color="inherit" startIcon={<LogoutRounded />}>
        {busy ? 'Cerrando sesión…' : 'Cerrar sesión'}
      </Button>
    </Stack></header>
    <Container maxWidth="md" sx={{ py: { xs: 5, md: 9 } }}>
      <Typography variant="h1" component="h1">Hola, {user.fullName.split(' ')[0]}<span className="accent-text">.</span></Typography>
      <Typography className="eyebrow" sx={{ color: 'primary.main', mb: 2 }}>ESTE ES TU ESPACIO</Typography>
      {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
      <Paper elevation={0} sx={{ border: '1px solid #e8e8ee', p: { xs: 3, md: 4 }, borderRadius: 4 }}>
        <Stack direction="row" spacing={2} sx={{ alignItems: 'center', mb: 3 }}>
          <Avatar sx={{ width: 56, height: 56, bgcolor: 'secondary.light', color: 'primary.main' }}><PersonOutlineRounded /></Avatar>
          <Box sx={{ flex: 1 }}><Typography variant="h3" component="h2">Mi perfil</Typography><Typography variant="body2" color="text.secondary">Tus datos, en un solo lugar.</Typography></Box>
          <Chip label="Activo" size="small" icon={<CheckCircleOutlineRounded />} sx={{ background: '#eaf5ee', color: '#23775a' }} />
        </Stack>
        <Divider />
        <dl className="profile-details">
          <div><dt>Nombre completo</dt><dd>{user.fullName}</dd></div>
          <div><dt>Correo electrónico</dt><dd>{user.email}</dd></div>
          <div><dt>Miembro desde</dt><dd>{new Intl.DateTimeFormat('es-CO', { dateStyle: 'long' }).format(new Date(user.createdAtUtc))}</dd></div>
          <div><dt>Identificador de perfil</dt><dd className="profile-id">{user.id}</dd></div>
        </dl>
      </Paper>
      <Stack direction="row" spacing={1} sx={{ color: 'text.secondary', mt: 3 }}>
        <LockOutlined sx={{ fontSize: 18, mt: .2 }} /><Typography variant="body2">Tu sesión se protege en el navegador. Puedes cerrarla cuando quieras.</Typography>
      </Stack>
    </Container>
  </Box>;
}
