import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { CreateProfilePage } from './CreateProfilePage';
import { usersApi } from '../services/usersApi';
import { theme } from '../../../app/theme';

vi.mock('../services/usersApi', () => ({ usersApi: { create: vi.fn() } }));
function LoginDestination() { const { state } = useLocation(); return <p>Perfil creado: {state?.email}</p>; }
function setup() {
  render(<ThemeProvider theme={theme}><MemoryRouter initialEntries={['/crear-perfil']}><Routes>
    <Route path="/crear-perfil" element={<CreateProfilePage />} /><Route path="/ingresar" element={<LoginDestination />} />
  </Routes></MemoryRouter></ThemeProvider>);
}
async function fillForm() {
  const user = userEvent.setup();
  await user.type(screen.getByLabelText(/Nombre completo/), 'Ana Pérez');
  await user.type(screen.getByLabelText(/Correo electrónico/), 'ana@example.com');
  await user.type(screen.getByLabelText(/^Contraseña/), 'una frase muy segura');
  await user.type(screen.getByLabelText(/^Confirmar contraseña/), 'una frase muy segura');
  return user;
}
describe('create profile', () => {
  beforeEach(() => { usersApi.create.mockReset(); });
  it('does not call the API with missing required fields', async () => {
    setup(); fireEvent.submit(screen.getByRole('form', { name: 'Crear perfil' }));
    expect(await screen.findByText('Escribe tu nombre completo.')).toBeVisible();
    expect(usersApi.create).not.toHaveBeenCalled();
  });
  it('creates the profile through the service and routes to login', async () => {
    usersApi.create.mockResolvedValue({ id: 'user-1', email: 'ana@example.com' }); setup();
    const user = await fillForm(); await user.click(screen.getByRole('button', { name: 'Crear mi perfil' }));
    expect(await screen.findByText('Perfil creado: ana@example.com')).toBeVisible();
    expect(usersApi.create).toHaveBeenCalledWith({ fullName: 'Ana Pérez', email: 'ana@example.com', password: 'una frase muy segura' });
  });
  it('shows an existing email without reporting success', async () => {
    usersApi.create.mockRejectedValue(Object.assign(new Error('Correo duplicado'), { status: 409 })); setup();
    const user = await fillForm(); await user.click(screen.getByRole('button', { name: 'Crear mi perfil' }));
    expect(await screen.findByText(/Este correo ya tiene un perfil/)).toBeVisible();
    expect(screen.queryByText(/Perfil creado:/)).not.toBeInTheDocument();
  });
  it('lets the user reveal and conceal the password', async () => {
    setup(); const user = userEvent.setup(); const input = screen.getByLabelText(/^Contraseña/);
    expect(input).toHaveAttribute('type', 'password');
    await user.click(screen.getAllByRole('button', { name: 'Mostrar contraseña' })[0]);
    expect(input).toHaveAttribute('type', 'text');
    await user.click(screen.getByRole('button', { name: 'Ocultar contraseña' }));
    expect(input).toHaveAttribute('type', 'password');
  });
});
