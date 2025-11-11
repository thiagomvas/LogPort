const useSsl = import.meta.env.LOGPORT_USESSL === 'true';
const API_BASE_URL = `${useSsl ? 'https' : 'http'}://${import.meta.env.LOGPORT_AGENT_URL}`;


export async function baseFetch<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const url = `${API_BASE_URL}${endpoint}`;

    const response = await fetch(url, options);
    if (!response.ok) {
        return response.text().then((text) => {
            throw new Error(`API request failed: ${response.status} ${response.statusText} - ${text}`);
        });
    }
    return await (response.json() as Promise<T>);
}


