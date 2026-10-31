import { beforeEach, expect, it, vi } from 'vitest';

const apiRequest = vi.hoisted(() => vi.fn());
vi.mock('../../../shared/api/client', () => ({ apiRequest }));
const { cardsApi } = await import('./cardsApi');

const card = { holderName: 'Ana Pérez', brand: 'Visa', last4: '4242', expirationMonth: 8, expirationYear: 2030 };
beforeEach(() => apiRequest.mockReset());

it('requests cards for a user', async () => {
  apiRequest.mockResolvedValue([]);
  await expect(cardsApi.list('user-123')).resolves.toEqual([]);
  expect(apiRequest).toHaveBeenCalledWith('/cards/user/user-123');
});

it('creates a card with the user id', async () => {
  await cardsApi.create({ ...card, userId: 'user-123' });
  expect(apiRequest).toHaveBeenCalledWith('/cards', { method: 'POST', body: { ...card, userId: 'user-123' } });
});

it('updates a card', async () => {
  await cardsApi.update('card-1', card);
  expect(apiRequest).toHaveBeenCalledWith('/cards/card-1', { method: 'PUT', body: card });
});

it('deletes a card', async () => {
  await cardsApi.remove('card-1');
  expect(apiRequest).toHaveBeenCalledWith('/cards/card-1', { method: 'DELETE' });
});
