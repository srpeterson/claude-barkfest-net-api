import { useEffect } from 'react'
import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'

export function ProtectedRoute() {
  const { isAuthenticated, accountType, openLoginModal } = useAuth()
  const isOwner = isAuthenticated && accountType === 'owner'

  useEffect(() => {
    if (!isAuthenticated) {
      openLoginModal()
    }
  }, [])

  return isOwner ? <Outlet /> : <Navigate to="/" replace />
}
