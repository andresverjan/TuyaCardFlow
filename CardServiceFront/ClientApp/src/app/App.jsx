import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import { Suspense } from 'react';
import { Box, CircularProgress, Stack, Typography } from '@mui/material';
import { useAuth } from '../modules/auth/context/AuthContext';
import { authRoutes } from '../modules/auth/routes';
import { userRoutes } from '../modules/users/routes';
import { cardRoutes } from '../modules/cards/routes';
import { paymentRoutes } from '../modules/payments/routes';

export function App() {
  const { user, loading } = useAuth();
  const location = useLocation();
  const routes = [...userRoutes, ...cardRoutes, ...paymentRoutes, ...authRoutes];
  const isProtectedPage = routes.some(route => route.access === 'private' && route.path === location.pathname);
  if (loading && isProtectedPage) 
    return <Stack sx={{ minHeight: '100vh', alignItems: 'center', justifyContent: 'center' }} spacing={2}>
    <CircularProgress size={28} /><Typography color="text.secondary">Preparando tu espacio…</Typography>
  </Stack>;
  return <Suspense fallback={<Box sx={{ p: 8, textAlign: 'center' }}><CircularProgress aria-label="Cargando página" /></Box>}><Routes>
    {routes.map(route => <Route key={route.path} path={route.path}
      element={route.access === 'private' && !user ? <Navigate to="/ingresar" replace />
        : route.access === 'guest' && user ? <Navigate to="/mi-perfil" replace /> : route.element} />)}
    <Route path="/" element={<Navigate to={user ? '/mi-perfil' : '/ingresar'} replace />} />
    <Route path="*" element={<Navigate to={user ? '/mi-perfil' : '/ingresar'} replace />} />
  </Routes></Suspense>;
}
