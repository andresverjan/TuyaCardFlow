export function validateProfile({ fullName, email, password, confirmation }) {
  const errors = {};
  if (!fullName?.trim()) errors.fullName = 'Escribe tu nombre completo.';
  else if (fullName.trim().length > 150 || /[\x00-\x1F\x7F]/.test(fullName)) errors.fullName = 'Usa hasta 150 caracteres, sin saltos de línea.';
  if (!email?.trim() || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) errors.email = 'Escribe un correo válido.';
  else if (email.trim().length > 254) errors.email = 'El correo admite hasta 254 caracteres.';
  if (!password?.trim() || password.length < 12 || password.length > 128) errors.password = 'Usa entre 12 y 128 caracteres.';
  if (confirmation !== password) errors.confirmation = 'Las contraseñas no coinciden.';
  return errors;
}
