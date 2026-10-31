import { describe, expect, it } from 'vitest';
import { validateProfile } from './profileValidation';

const valid = { fullName: 'Ana Pérez', email: 'ana@example.com', password: 'una frase muy segura', confirmation: 'una frase muy segura' };
describe('profile validation', () => {
  it('accepts a profile with a passphrase', () => expect(validateProfile(valid)).toEqual({}));
  it.each([
    ['fullName', ''], ['fullName', 'A'.repeat(151)], ['fullName', 'Ana\nPérez'],
    ['email', 'correo'], ['email', 'ana @example.com'], ['password', 'corta'],
    ['password', ' '.repeat(12)], ['password', 'a'.repeat(129)], ['confirmation', 'diferente'],
  ])('rejects invalid %s', (field, value) => expect(validateProfile({ ...valid, [field]: value })[field]).toBeTruthy());
});
