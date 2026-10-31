import { apiRequest } from '../../../shared/api/client';

export const paymentsApi = {
  register: payment => apiRequest('/payments', { method: 'POST', body: payment }),
  history: (userId, page = 1, pageSize = 10) =>
    apiRequest(`/users/${userId}/transactions?page=${page}&pageSize=${pageSize}`),
  transaction: (userId, id) => apiRequest(`/users/${userId}/transactions/${id}`),
};
