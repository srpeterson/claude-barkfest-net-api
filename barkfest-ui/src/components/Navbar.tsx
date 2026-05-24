import { Link, useNavigate } from 'react-router-dom'
import { LogOut, PawPrint, Plus, UserCircle } from 'lucide-react'
import { buttonVariants } from '@/components/ui/button'
import { cn } from '@/lib/utils'
import { useAuth } from '@/hooks/useAuth'
import { logout } from '@/lib/api'

export function Navbar() {
  const { isAuthenticated, accountType, signOut, openLoginModal } = useAuth()
  const navigate = useNavigate()

  async function handleSignOut() {
    await logout()
    signOut()
  }

  return (
    <nav className="sticky top-0 z-50 backdrop-blur-md bg-primary/20 border-b border-primary/30">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 h-16 flex items-center justify-between">
        <Link to="/" className="flex items-center gap-2 hover:opacity-80 transition-opacity">
          <PawPrint className="w-6 h-6 text-primary" />
          <span className="font-heading text-xl font-semibold tracking-tight">Barkfest</span>
        </Link>

        {isAuthenticated && accountType === 'admin' ? (
          <div className="flex items-center gap-3">
            <span className="text-sm text-muted-foreground font-medium">
              Logged in as Administrator
            </span>
            <button
              onClick={handleSignOut}
              className={cn(
                buttonVariants({ variant: 'ghost', size: 'sm' }),
                'font-medium hover:bg-primary/20 hover:text-primary gap-1.5'
              )}
            >
              <LogOut className="w-4 h-4" />
              Sign Out
            </button>
          </div>
        ) : isAuthenticated && accountType === 'owner' ? (
          <div className="flex items-center gap-3">
            <button
              onClick={() => navigate('/pets')}
              className={cn(
                buttonVariants({ size: 'sm' }),
                'gap-1.5 font-medium'
              )}
            >
              <Plus className="w-4 h-4" />
              Post a Pet
            </button>

            <button className="w-9 h-9 rounded-full bg-secondary flex items-center justify-center text-muted-foreground hover:text-foreground transition-colors overflow-hidden">
              <UserCircle className="w-7 h-7" />
            </button>

            <button
              onClick={handleSignOut}
              className={cn(
                buttonVariants({ variant: 'ghost', size: 'sm' }),
                'font-medium hover:bg-primary/20 hover:text-primary gap-1.5'
              )}
            >
              <LogOut className="w-4 h-4" />
              Sign Out
            </button>
          </div>
        ) : (
          <button
            onClick={openLoginModal}
            className={cn(
              buttonVariants({ variant: 'ghost', size: 'sm' }),
              'font-medium hover:bg-primary/20 hover:text-primary'
            )}
          >
            Sign In
          </button>
        )}
      </div>
    </nav>
  )
}
