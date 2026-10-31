export class ApiError extends Error {
  constructor(message, status = 0, errors = {}) {
    super(message); this.name = 'ApiError'; this.status = status; this.errors = errors;
  }
}

let csrfToken;
export function resetCsrfToken() { csrfToken = undefined; }

async function request(path, { method = 'GET', body, signal } = {}) {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), 15000);
  const abort = () => controller.abort();
  signal?.addEventListener('abort', abort, { once: true });
  if (signal?.aborted) controller.abort();
  try {
    const response = await fetch(`/api${path}`, {
      method, credentials: 'same-origin', signal: controller.signal,
      headers: {
        Accept: 'application/json',
        ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
        ...(method !== 'GET' && csrfToken ? { 'X-CSRF-TOKEN': csrfToken } : {}),
      },
      ...(body !== undefined ? { body: JSON.stringify(body) } : {}),
    });
    if (response.status === 204) return null;
    const data = await response.json().catch(() => null);
    if (!response.ok) {
      const message = response.status === 429 ? 'Has realizado varios intentos. Espera un minuto y vuelve a intentarlo.'
        : data?.detail || data?.title || 'No pudimos completar la solicitud. Inténtalo de nuevo.';
      if (response.status === 401 && path !== '/auth/login')
        window.dispatchEvent(new Event('tuya:session-expired'));
      throw new ApiError(message, response.status, data?.errors || {});
    }
    if (data === null) throw new ApiError('La respuesta del servidor no es válida.');
    return data;
  } catch (error) {
    if (error instanceof ApiError) throw error;
    if (signal?.aborted) throw error;
    throw new ApiError('No pudimos conectarnos. Comprueba que el backend esté iniciado y vuelve a intentarlo.');
  } finally {
    clearTimeout(timer); signal?.removeEventListener('abort', abort);
  }
}

export async function apiRequest(path, options = {}) {
  if (options.method && options.method !== 'GET' && !csrfToken)
    csrfToken = (await request('/auth/csrf')).token;
  return request(path, options);
}
