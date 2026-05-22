import { Route, Routes } from 'react-router-dom'
import { LoginPage }    from '@/features/auth/LoginPage'
import { RegisterPage } from '@/features/auth/RegisterPage'
import { OwnersPage }   from '@/features/owners/OwnersPage'
import { PetsPage }     from '@/features/pets/PetsPage'
import { ShellLayout }  from '@/layouts/ShellLayout'
import { HomePage }     from '@/pages/HomePage'

export function App() {
  return (
    <Routes>
      {/* Public home — has its own Navbar */}
      <Route index element={<HomePage />} />

      {/* Internal/auth pages — wrapped in ShellLayout */}
      <Route element={<ShellLayout />}>
        <Route path="login"    element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route path="owners"   element={<OwnersPage />} />
        <Route path="pets"     element={<PetsPage />} />
      </Route>
    </Routes>
  )
}
