import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, expect, it, vi } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { LoginPage } from './LoginPage';
import { useAuth } from '../context/AuthContext';
import { theme } from '../../../app/theme';

vi.mock('../context/AuthContext', () => ({ useAuth: vi.fn() }));
const login = vi.fn();
beforeEach(() => { login.mockReset(); useAuth.mockReturnValue({ login }); });
function setup() {
  render(<ThemeProvider theme={theme}><MemoryRouter initialEntries={['/ingresar']}><Routes>
    <Route path="/ingresar" element={<LoginPage />} /><Route path="/mi-perfil" element={<p>Mi perfil privado</p>} />
  </Routes></MemoryRouter></ThemeProvider>);
}
async function enterCredentials() {
  const user = userEvent.setup();
  await user.type(screen.getByLabelText(/Correo electrónico/), 'ana@example.com');
  await user.type(screen.getByLabelText(/^Contraseña/), 'una frase muy segura');
  await user.click(screen.getByRole('button', { name: 'Iniciar sesión' }));
}
it('enters the private area only after a successful login', async () => {
  login.mockResolvedValue(undefined); setup(); await enterCredentials();
  expect(await screen.findByText('Mi perfil privado')).toBeVisible();
  expect(login).toHaveBeenCalledWith({ email: 'ana@example.com', password: 'una frase muy segura' });
});
it('stays on the login form for incorrect credentials', async () => {
  login.mockRejectedValue(Object.assign(new Error('Credenciales inválidas'), { status: 401 })); setup(); await enterCredentials();
  expect(await screen.findByRole('alert')).toHaveTextContent('El correo o la contraseña no son correctos');
  expect(screen.queryByText('Mi perfil privado')).not.toBeInTheDocument();
});
