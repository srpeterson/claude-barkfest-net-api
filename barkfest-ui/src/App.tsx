import { Navigate, Route, Routes } from 'react-router-dom'
import { LoginPage } from '@/features/auth/LoginPage'
import { RegisterPage } from '@/features/auth/RegisterPage'
import { OwnersPage } from '@/features/owners/OwnersPage'
import { PetsPage } from '@/features/pets/PetsPage'
import { ShellLayout } from '@/layouts/ShellLayout'

export function App() {
  return (
    <Routes>
      <Route path="/" element={<ShellLayout />}>
        <Route index element={<Navigate to="/login" replace />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route path="owners" element={<OwnersPage />} />
        <Route path="pets" element={<PetsPage />} />
      </Route>
    </Routes>
  )
}
