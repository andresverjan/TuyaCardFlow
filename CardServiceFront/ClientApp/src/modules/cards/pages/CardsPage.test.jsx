import { render, screen } from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { ThemeProvider } from '@mui/material';
import { theme } from '../../../app/theme';
import { CardsPage } from './CardsPage';

const api = vi.hoisted(() => ({ list: vi.fn(), create: vi.fn(), update: vi.fn(), remove: vi.fn() }));
vi.mock('../services/cardsApi', () => ({ cardsApi: api }));
vi.mock('../../../modules/auth/context/AuthContext', () => ({ useAuth: () => ({ user: { id: 'user-123' } }) }));

beforeEach(() => {
  vi.clearAllMocks();
  api.list.mockResolvedValue([{ id: 'card-1', brand: 'Visa', last4: '4242', holderName: 'Ana Pérez', expirationMonth: 8, expirationYear: 2030 }]);
});

it('lists only the signed-in user cards', async () => {
  render(<ThemeProvider theme={theme}><MemoryRouter><CardsPage /></MemoryRouter></ThemeProvider>);
  expect(await screen.findByText('Visa ···· 4242')).toBeVisible();
  expect(api.list).toHaveBeenCalledWith('user-123');
});
