import { createContext, useCallback, useContext, useEffect, useState } from 'react';
import { authApi } from '../services/authApi';

const AuthContext = createContext(null);
export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const restore = useCallback(async () => {
    setLoading(true);
    try { setUser(await authApi.me()); }
    catch { setUser(null); }
    finally { setLoading(false); }
  }, []);
  useEffect(() => {
    restore();
    const expired = () => setUser(null);
    window.addEventListener('tuya:session-expired', expired);
    return () => window.removeEventListener('tuya:session-expired', expired);
  }, [restore]);
  const login = async values => { setUser(await authApi.login(values)); };
  const logout = async () => { await authApi.logout(); setUser(null); };
  return <AuthContext.Provider value={{ user, loading, login, logout, restore }}>{children}</AuthContext.Provider>;
}
export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error('useAuth requires AuthProvider');
  return value;
}
