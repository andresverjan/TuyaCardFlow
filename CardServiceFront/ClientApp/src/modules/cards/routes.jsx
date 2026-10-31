import { lazy } from 'react';

const CardsPage = lazy(() => import('./pages/CardsPage').then(module => ({ default: module.CardsPage })));

export const cardRoutes = [
  { path: '/tarjetas', element: <CardsPage />, access: 'private' },
];
