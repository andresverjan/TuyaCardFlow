import { lazy } from 'react';

const CreateProfilePage = lazy(() => import('./pages/CreateProfilePage').then(module => ({ default: module.CreateProfilePage })));
const MyProfilePage = lazy(() => import('./pages/MyProfilePage').then(module => ({ default: module.MyProfilePage })));

export const userRoutes = [
  { path: '/crear-perfil', element: <CreateProfilePage />, access: 'guest' },
  { path: '/mi-perfil', element: <MyProfilePage />, access: 'private' },
];
