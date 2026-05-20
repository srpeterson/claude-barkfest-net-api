import { useState } from 'react'

interface AuthState {
  isAuthenticated: boolean
  accountId: string | null
  accountType: 'owner' | 'admin' | null
}

const initialState: AuthState = {
  isAuthenticated: !!sessionStorage.getItem('barkfest_authenticated'),
  accountId: sessionStorage.getItem('barkfest_account_id'),
  accountType: (sessionStorage.getItem('barkfest_account_type') as AuthState['accountType']) ?? null,
}

export function useAuth() {
  const [auth, setAuth] = useState<AuthState>(initialState)

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

  return { ...auth, signIn, signOut }
}
