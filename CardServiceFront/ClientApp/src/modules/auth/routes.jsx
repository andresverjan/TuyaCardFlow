import { lazy } from 'react';

const LoginPage = lazy(() => import('./pages/LoginPage').then(module => ({ default: module.LoginPage })));

export const authRoutes = [{ path: '/ingresar', element: <LoginPage />, access: 'guest' }];
