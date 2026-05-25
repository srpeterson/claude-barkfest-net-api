import { useEffect } from 'react'
import { Route, Routes } from 'react-router-dom'
import { OwnersPage }      from '@/features/owners/OwnersPage'
import { PetsPage }        from '@/features/pets/PetsPage'
import { ShellLayout }     from '@/layouts/ShellLayout'
import { ProtectedRoute }  from '@/components/ProtectedRoute'
import { LoginModal }      from '@/components/LoginModal'
import { RegisterModal }   from '@/components/RegisterModal'
import { HomePage }        from '@/pages/HomePage'
import { useAuth }         from '@/hooks/useAuth'
import { setAuthToken, setUnauthorizedHandler } from '@/lib/api'

export function App() {
  const { token, signOut, openLoginModal } = useAuth()

  // Keep the api module's token in sync with auth state
  useEffect(() => {
    setAuthToken(token)
  }, [token])

  useEffect(() => {
    setUnauthorizedHandler(() => {
      setAuthToken(null)
      signOut()
      openLoginModal()
    })
  }, [signOut, openLoginModal])

  return (
    <>
      <Routes>
        {/* Public home — has its own Navbar */}
        <Route index element={<HomePage />} />

        {/* Protected pages — redirect to / if not authenticated */}
        <Route element={<ProtectedRoute />}>
          <Route element={<ShellLayout />}>
            <Route path="owners" element={<OwnersPage />} />
            <Route path="pets"   element={<PetsPage />} />
          </Route>
        </Route>
      </Routes>

      {/* Auth modals — rendered over any page */}
      <LoginModal />
      <RegisterModal />
    </>
  )
}
