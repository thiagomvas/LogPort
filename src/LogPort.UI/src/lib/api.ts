const useSsl = import.meta.env.LOGPORT_USESSL === 'true';
const agentUrl = import.meta.env.LOGPORT_AGENT_URL || '';
const API_BASE_URL = agentUrl
  ? `${useSsl ? 'https' : 'http'}://${agentUrl}`
  : `${window.location.origin}`;

const DEV_USER = import.meta.env.LOGPORT_USER || 'admin';
const DEV_PASS = import.meta.env.LOGPORT_PASS || 'changeme';

export async function baseFetch<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`;

  const headers = new Headers(options.headers);

  if (!headers.has('Authorization') && DEV_USER && DEV_PASS) {
    const encoded = btoa(`${DEV_USER}:${DEV_PASS}`);
    headers.set('Authorization', `Basic ${encoded}`);
  }

  const response = await fetch(url, { ...options, headers });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(
      `API request failed: ${response.status} ${response.statusText} - ${text}`
    );
  }

  return (await response.json()) as T;
}
