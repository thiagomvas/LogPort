const useSsl = import.meta.env.LOGPORT_USESSL === 'true';
const agentUrl = import.meta.env.LOGPORT_AGENT_URL || '';
const API_BASE_URL = agentUrl
  ? `${useSsl ? 'https' : 'http'}://${agentUrl}`
  : `${window.location.origin}`;

export async function login(username: string, password: string) {
  const data = await baseFetch<{ token: string }>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password })
  });

  sessionStorage.setItem('jwtToken', data.token);
  return data.token;
}

export async function baseFetch<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`;
  const token = sessionStorage.getItem('jwtToken');

  // Use Record<string, string> so TypeScript knows we can index with strings
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string> | undefined)
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    ...options,
    headers,
    credentials: 'include'
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(
      `API request failed: ${response.status} ${response.statusText} - ${text}`
    );
  }

  return (await response.json()) as T;
}
