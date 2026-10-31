export function validatePayment({ card, amount, securityCode }) {
  if (!card) return { field: 'cardId', message: 'Selecciona una tarjeta.' };
  const value = String(amount).trim();
  if (!/^\d+(\.\d{1,2})?$/.test(value) || !Number.isFinite(Number(value)) || Number(value) <= 0 || Number(value) > 9999999999999999.99)
    return { field: 'amount', message: 'Ingresa un monto positivo con máximo dos decimales.' };

  const code = String(securityCode).trim();
  const isAmex = /american\s*express|amex/i.test(card.brand);
  const validCode = isAmex ? /^\d{4}$/.test(code) : /^\d{3}$/.test(code);
  if (!validCode) return { field: 'securityCode', message: isAmex
    ? 'El código de seguridad debe tener cuatro dígitos.'
    : 'El código de seguridad debe tener tres dígitos.' };
  return null;
}
