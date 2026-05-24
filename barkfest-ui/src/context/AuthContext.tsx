import { createContext, useContext, useState, type ReactNode } from 'react'

interface AuthState {
  isAuthenticated: boolean
  accountId: string | null
  accountType: 'owner' | 'admin' | null
}

interface AuthContextValue extends AuthState {
  signIn: (accountId: string, accountType: 'owner' | 'admin') => void
  signOut: () => void
  modal: 'login' | 'register' | null
  openLoginModal: () => void
  openRegisterModal: () => void
  closeModal: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<AuthState>({
    isAuthenticated: !!sessionStorage.getItem('barkfest_authenticated'),
    accountId: sessionStorage.getItem('barkfest_account_id'),
    accountType: (sessionStorage.getItem('barkfest_account_type') as AuthState['accountType']) ?? null,
  })
  const [modal, setModal] = useState<'login' | 'register' | null>(null)

  function signIn(accountId: string, accountType: 'owner' | 'admin') {
    sessionStorage.setItem('barkfest_authenticated', 'true')
    sessionStorage.setItem('barkfest_account_id', accountId)
    sessionStorage.setItem('barkfest_account_type', accountType)
    setAuth({ isAuthenticated: true, accountId, accountType })
  }

  function signOut() {
    sessionStorage.clear()
    setAuth({ isAuthenticated: false, accountId: null, accountType: null })
  }

  return (
    <AuthContext.Provider value={{
      ...auth,
      signIn,
      signOut,
      modal,
      openLoginModal: () => setModal('login'),
      openRegisterModal: () => setModal('register'),
      closeModal: () => setModal(null),
    }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuthContext(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuthContext must be used within AuthProvider')
  return ctx
}
