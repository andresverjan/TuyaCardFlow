import { apiRequest } from '../../../shared/api/client';

export const cardsApi = {
  list: userId => apiRequest(`/cards/user/${userId}`),
  create: card => apiRequest('/cards', { method: 'POST', body: card }),
  update: (id, card) => apiRequest(`/cards/${id}`, { method: 'PUT', body: card }),
  remove: id => apiRequest(`/cards/${id}`, { method: 'DELETE' }),
};
