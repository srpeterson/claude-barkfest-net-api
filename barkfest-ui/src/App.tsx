import { useEffect } from 'react'
import { Route, Routes } from 'react-router-dom'
import { OwnersPage }        from '@/features/owners/OwnersPage'
import { PetsPage }          from '@/features/pets/PetsPage'
import { PetDetailPage }     from '@/features/pets/PetDetailPage'
import { ManagePetsPage }    from '@/features/pets/ManagePetsPage'
import { LoginPage }         from '@/features/auth/LoginPage'
import { RegisterPage }      from '@/features/auth/RegisterPage'
import { ShellLayout }       from '@/layouts/ShellLayout'
import { ProtectedRoute }    from '@/components/ProtectedRoute'
import { LoginDialog }       from '@/components/LoginDialog'
import { RegisterDialog }    from '@/components/RegisterDialog'
import { HomePage }          from '@/pages/HomePage'
import { useAuth }           from '@/hooks/useAuth'
import { setAuthToken, setUnauthorizedHandler } from '@/lib/api'

export function App() {
  const { token, signOut, openLoginDialog } = useAuth()

  // Keep the api module's token in sync with auth state
  useEffect(() => {
    setAuthToken(token)
  }, [token])

  useEffect(() => {
    setUnauthorizedHandler(() => {
      setAuthToken(null)
      signOut()
      openLoginDialog()
    })
  }, [signOut, openLoginDialog])

  return (
    <>
      <Routes>
        {/* Public pages */}
        <Route index element={<HomePage />} />
        <Route path="login"    element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route path="pets/:petId" element={<PetDetailPage />} />

        {/* Protected pages — redirect to / if not authenticated */}
        <Route element={<ProtectedRoute />}>
          <Route path="manage" element={<ManagePetsPage />} />
          <Route element={<ShellLayout />}>
            <Route path="owners" element={<OwnersPage />} />
            <Route path="pets"   element={<PetsPage />} />
          </Route>
        </Route>
      </Routes>

      {/* Auth dialogs — rendered over any page (session expiry fallback) */}
      <LoginDialog />
      <RegisterDialog />
    </>
  )
}
