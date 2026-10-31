import { afterEach, beforeEach, expect, it, vi } from 'vitest';
import { ApiError, apiRequest, resetCsrfToken } from './client';

const response = (status, data) => ({ ok: status >= 200 && status < 300, status, json: async () => data });
beforeEach(() => { resetCsrfToken(); vi.stubGlobal('fetch', vi.fn()); });
afterEach(() => vi.unstubAllGlobals());
it('requests a CSRF token before submitting data with same-origin cookies', async () => {
  fetch.mockResolvedValueOnce(response(200, { token: 'csrf-test' })).mockResolvedValueOnce(response(201, { id: 'user-1' }));
  await expect(apiRequest('/users', { method: 'POST', body: { fullName: 'Ana' } })).resolves.toEqual({ id: 'user-1' });
  expect(fetch).toHaveBeenNthCalledWith(2, '/api/users', expect.objectContaining({
    method: 'POST', credentials: 'same-origin', headers: expect.objectContaining({ 'X-CSRF-TOKEN': 'csrf-test' }),
  }));
});
it('preserves field validation errors returned by the backend', async () => {
  fetch.mockResolvedValue(response(400, { title: 'Datos inválidos', errors: { Email: ['Correo inválido'] } }));
  await expect(apiRequest('/users')).rejects.toMatchObject({ status: 400, errors: { Email: ['Correo inválido'] } });
});
it('reports connection errors without pretending the request succeeded', async () => {
  fetch.mockRejectedValue(new TypeError('Failed to fetch'));
  await expect(apiRequest('/auth/me')).rejects.toBeInstanceOf(ApiError);
});
it('clears the session through an event when a protected request returns 401', async () => {
  const listener = vi.fn(); window.addEventListener('tuya:session-expired', listener);
  fetch.mockResolvedValue(response(401, { title: 'Inicia sesión' }));
  await expect(apiRequest('/auth/me')).rejects.toMatchObject({ status: 401 });
  expect(listener).toHaveBeenCalledTimes(1); window.removeEventListener('tuya:session-expired', listener);
});
