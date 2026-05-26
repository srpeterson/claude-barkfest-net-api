import { createContext, useContext, useState, type ReactNode } from 'react'

interface AuthState {
  isAuthenticated: boolean
  accountId: string | null
  accountType: 'owner' | 'admin' | null
  token: string | null
}

interface AuthContextValue extends AuthState {
  signIn: (accountId: string, accountType: 'owner' | 'admin', token: string) => void
  signOut: () => void
  dialog: 'login' | 'register' | null
  openLoginDialog: () => void
  openRegisterDialog: () => void
  closeDialog: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<AuthState>({
    isAuthenticated: !!sessionStorage.getItem('barkfest_authenticated'),
    accountId: sessionStorage.getItem('barkfest_account_id'),
    accountType: (sessionStorage.getItem('barkfest_account_type') as AuthState['accountType']) ?? null,
    token: sessionStorage.getItem('barkfest_token'),
  })
  const [dialog, setDialog] = useState<'login' | 'register' | null>(null)

  function signIn(accountId: string, accountType: 'owner' | 'admin', token: string) {
    sessionStorage.setItem('barkfest_authenticated', 'true')
    sessionStorage.setItem('barkfest_account_id', accountId)
    sessionStorage.setItem('barkfest_account_type', accountType)
    sessionStorage.setItem('barkfest_token', token)
    setAuth({ isAuthenticated: true, accountId, accountType, token })
  }

  function signOut() {
    sessionStorage.clear()
    setAuth({ isAuthenticated: false, accountId: null, accountType: null, token: null })
  }

  return (
    <AuthContext.Provider value={{
      ...auth,
      signIn,
      signOut,
      dialog,
      openLoginDialog: () => setDialog('login'),
      openRegisterDialog: () => setDialog('register'),
      closeDialog: () => setDialog(null),
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
