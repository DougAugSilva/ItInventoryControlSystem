const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5080';

const TOKEN_KEY = 'inventory_token';

export function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token) {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}

async function handleResponse(response) {
  if (response.status === 204) {
    return null;
  }

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    const message = data?.message || data?.title || 'Unexpected error while contacting the server.';
    throw new Error(message);
  }

  return data;
}

export async function apiJson(path, { method = 'GET', body } = {}) {
  const headers = { 'Content-Type': 'application/json' };
  const token = getToken();
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(`${API_URL}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });

  return handleResponse(response);
}

export async function apiForm(path, { method = 'POST', formData }) {
  const headers = {};
  const token = getToken();
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(`${API_URL}${path}`, {
    method,
    headers,
    body: formData,
  });

  return handleResponse(response);
}

export function fileUrl(path) {
  if (!path) return null;
  return `${API_URL}${path}`;
}
