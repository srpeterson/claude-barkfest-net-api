import { useEffect } from 'react'
import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'

export function ProtectedRoute() {
  const { isAuthenticated, accountType, openLoginDialog } = useAuth()
  const isOwner = isAuthenticated && accountType === 'owner'

  useEffect(() => {
    if (!isAuthenticated) {
      openLoginDialog()
    }
  }, [isAuthenticated, openLoginDialog])

  return isOwner ? <Outlet /> : <Navigate to="/" replace />
}
