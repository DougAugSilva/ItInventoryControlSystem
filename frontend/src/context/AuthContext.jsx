import { createContext, useContext, useEffect, useState } from 'react';
import { apiJson, clearToken, getToken, setToken } from '../services/api';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function restoreSession() {
      if (!getToken()) {
        setLoading(false);
        return;
      }

      try {
        const data = await apiJson('/api/auth/me');
        setUser(data);
      } catch {
        clearToken();
      } finally {
        setLoading(false);
      }
    }

    restoreSession();
  }, []);

  async function login(username, password) {
    const data = await apiJson('/api/auth/login', {
      method: 'POST',
      body: { username, password },
    });

    setToken(data.token);
    setUser({ username: data.username, fullName: data.fullName, isAdmin: data.isAdmin });
  }

  function logout() {
    clearToken();
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used inside an AuthProvider.');
  }
  return context;
}
