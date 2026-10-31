import { apiRequest, resetCsrfToken } from '../../../shared/api/client';

export const authApi = {
  me: options => apiRequest('/auth/me', options),
  async login(values) {
    resetCsrfToken();
    const user = await apiRequest('/auth/login', { method: 'POST', body: values });
    resetCsrfToken();
    return user;
  },
  async logout() {
    await apiRequest('/auth/logout', { method: 'POST' });
    resetCsrfToken();
  },
};
