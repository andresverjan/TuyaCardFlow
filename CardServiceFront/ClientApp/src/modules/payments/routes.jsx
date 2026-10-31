import { lazy } from 'react';

const PaymentsPage = lazy(() => import('./pages/PaymentsPage').then(module => ({ default: module.PaymentsPage })));

export const paymentRoutes = [
  { path: '/pagos', element: <PaymentsPage />, access: 'private' },
];
