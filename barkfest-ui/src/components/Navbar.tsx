import { useState } from 'react'
import { Link } from 'react-router-dom'
import { LogOut, PawPrint, Plus, UserCircle } from 'lucide-react'
import { useQueryClient } from '@tanstack/react-query'
import { buttonVariants } from '@/components/ui/button'
import { cn } from '@/lib/utils'
import { useAuth } from '@/hooks/useAuth'
import { logout } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { AddPetDialog } from '@/components/AddPetDialog'
import { UpdateOwnerProfileDialog } from '@/components/UpdateOwnerProfileDialog'

export function Navbar() {
  const { isAuthenticated, accountType, profileImageBlobName, signOut, openLoginDialog, openRegisterDialog } = useAuth()
  const [addPetOpen, setAddPetOpen] = useState(false)
  const [profileOpen, setProfileOpen] = useState(false)
  const queryClient = useQueryClient()

  function handlePetAdded() {
    queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
    queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
  }

  async function handleSignOut() {
    await logout()
    signOut()
  }

  return (
    <>
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
              onClick={() => setAddPetOpen(true)}
              className={cn(
                buttonVariants({ size: 'sm' }),
                'gap-1.5 font-medium'
              )}
            >
              <Plus className="w-4 h-4" />
              Post a Pet
            </button>

            {/* Avatar — opens UpdateOwnerProfileDialog */}
            <button
              onClick={() => setProfileOpen(true)}
              className="w-9 h-9 rounded-full bg-secondary flex items-center justify-center text-muted-foreground hover:text-foreground hover:ring-2 hover:ring-primary/40 transition-all overflow-hidden"
            >
              {profileImageBlobName ? (
                <img
                  src={getBlobImageUrl(profileImageBlobName, 'owner-profile-images')}
                  alt="Profile"
                  className="w-full h-full object-cover"
                />
              ) : (
                <UserCircle className="w-7 h-7" />
              )}
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
          <div className="flex items-center gap-2">
            <button
              onClick={openLoginDialog}
              className={cn(
                buttonVariants({ variant: 'ghost', size: 'sm' }),
                'font-medium hover:bg-primary/20 hover:text-primary'
              )}
            >
              Sign In
            </button>
            <button
              onClick={openRegisterDialog}
              className={cn(
                buttonVariants({ size: 'sm' }),
                'font-medium'
              )}
            >
              Join the Barkfest!
            </button>
          </div>
        )}
      </div>
    </nav>

    {addPetOpen && <AddPetDialog onClose={() => setAddPetOpen(false)} onSuccess={handlePetAdded} />}
    {profileOpen && <UpdateOwnerProfileDialog onClose={() => setProfileOpen(false)} />}
  </>
  )
}
