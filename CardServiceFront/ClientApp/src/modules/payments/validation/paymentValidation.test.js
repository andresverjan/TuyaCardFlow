import { expect, it } from 'vitest';
import { validatePayment } from './paymentValidation';

const card = { id: 'card-1', brand: 'Visa' };

it('requires a selected card and a positive amount with at most two decimals', () => {
  expect(validatePayment({ card: null, amount: '20', securityCode: '123' }).field).toBe('cardId');
  expect(validatePayment({ card, amount: '0', securityCode: '123' }).field).toBe('amount');
  expect(validatePayment({ card, amount: '12.345', securityCode: '123' }).field).toBe('amount');
  expect(validatePayment({ card, amount: '20.50', securityCode: '123' })).toBeNull();
});

it('checks only the expected security code format for the card brand', () => {
  expect(validatePayment({ card, amount: '20', securityCode: '12' }).field).toBe('securityCode');
  expect(validatePayment({ card: { ...card, brand: 'American Express' }, amount: '20', securityCode: '123' }).field).toBe('securityCode');
  expect(validatePayment({ card: { ...card, brand: 'American Express' }, amount: '20', securityCode: '1234' })).toBeNull();
});
