import { beforeEach, expect, it, vi } from 'vitest';

const apiRequest = vi.hoisted(() => vi.fn());
vi.mock('../../../shared/api/client', () => ({ apiRequest }));
const { paymentsApi } = await import('./paymentsApi');

beforeEach(() => apiRequest.mockReset());

it('registers a payment without a security code', async () => {
  const payment = { userId: 'user-1', cardId: 'card-1', amount: 12500, currency: 'COP', description: 'Compra' };
  await paymentsApi.register(payment);
  expect(apiRequest).toHaveBeenCalledWith('/payments', { method: 'POST', body: payment });
});

it('loads paginated transactions and a transaction detail for the user', async () => {
  await paymentsApi.history('user-1', 2, 10);
  await paymentsApi.transaction('user-1', 'transaction-1');
  expect(apiRequest).toHaveBeenNthCalledWith(1, '/users/user-1/transactions?page=2&pageSize=10');
  expect(apiRequest).toHaveBeenNthCalledWith(2, '/users/user-1/transactions/transaction-1');
});
