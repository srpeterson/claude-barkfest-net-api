import { Link, Outlet, useNavigate } from 'react-router-dom'
import { LogOut, PawPrint } from 'lucide-react'
import { useAuth } from '@/hooks/useAuth'
import { logout } from '@/lib/api'

export function ShellLayout() {
  const { signOut } = useAuth()
  const navigate = useNavigate()

  async function handleSignOut() {
    await logout()
    signOut()
    navigate('/login?signed-out=true')
  }

  return (
    <div className="min-h-screen flex flex-col bg-background">
      <header className="sticky top-0 z-50 border-b border-border bg-card/80 backdrop-blur-md px-6 h-16 flex items-center justify-between">
        <div className="flex items-center gap-6">
          <Link to="/" className="flex items-center gap-2 hover:opacity-80 transition-opacity">
            <PawPrint className="w-5 h-5 text-primary" />
            <span className="font-heading text-lg font-semibold tracking-tight">Barkfest</span>
          </Link>
          <nav className="flex gap-4 text-sm font-medium text-muted-foreground">
            <Link to="/pets" className="hover:text-foreground transition-colors">Pets</Link>
          </nav>
        </div>
        <button
          onClick={handleSignOut}
          className="flex items-center gap-1.5 text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
        >
          <LogOut className="w-4 h-4" />
          Sign Out
        </button>
      </header>
      <main className="flex-1 p-6">
        <Outlet />
      </main>
    </div>
  )
}
