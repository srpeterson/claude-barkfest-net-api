import { Link } from 'react-router-dom'
import { PawPrint } from 'lucide-react'
import { buttonVariants } from '@/components/ui/button'
import { cn } from '@/lib/utils'

/**
 * Public navbar — shown on the unauthenticated home page.
 *
 * When you wire up auth, replace the Sign In link with a branch that shows
 * the user avatar + "Post a Pet" button (see Base44/src/components/Navbar.jsx
 * for the authenticated variant to port across).
 */
export function Navbar() {
  return (
    <nav className="sticky top-0 z-50 backdrop-blur-md bg-primary/20 border-b border-primary/30">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 h-16 flex items-center justify-between">
        {/* Logo */}
        <Link to="/" className="flex items-center gap-2 hover:opacity-80 transition-opacity">
          <PawPrint className="w-6 h-6 text-primary" />
          <span className="font-heading text-xl font-semibold tracking-tight">Barkfest</span>
        </Link>

        {/* Auth actions */}
        <Link
          to="/login"
          className={cn(
            buttonVariants({ variant: 'ghost', size: 'sm' }),
            'font-medium hover:bg-primary/20 hover:text-primary'
          )}
        >
          Sign In
        </Link>
      </div>
    </nav>
  )
}
