import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { LogOut, PawPrint, Plus, Settings, UserCircle } from 'lucide-react'
import { useQueryClient } from '@tanstack/react-query'
import { cn } from '@/lib/utils'
import { useAuth } from '@/hooks/useAuth'
import { logout } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { BarkfestMark } from '@/components/BarkfestMark'
import { AddPetDialog } from '@/components/AddPetDialog'
import { UpdateOwnerProfileDialog } from '@/components/UpdateOwnerProfileDialog'

export function Navbar() {
  const navigate = useNavigate()
  const { isAuthenticated, accountType, profileImageBlobName, signOut } = useAuth()
  const [addPetOpen, setAddPetOpen] = useState(false)
  const [profileOpen, setProfileOpen] = useState(false)
  const [dropdownOpen, setDropdownOpen] = useState(false)
  const dropdownRef = useRef<HTMLDivElement>(null)
  const queryClient = useQueryClient()

  function handlePetAdded() {
    queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
    queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
  }

  async function handleSignOut() {
    await logout()
    signOut()
    navigate('/')
  }

  // Close dropdown when clicking outside
  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setDropdownOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  return (
    <>
      {/* Sticky transparent wrapper — pill floats 10px from top */}
      <div className="sticky top-0 z-50 px-4 pt-[10px] pb-[10px]">
        <nav
          className="flex items-center justify-between h-[52px] px-[22px] rounded-full"
          style={{
            background: 'var(--primary)',
            boxShadow: '0 4px 24px rgba(0,0,0,0.18)',
          }}
        >
          {/* Logo */}
          <Link
            to="/"
            className="flex items-center gap-2 hover:opacity-85 transition-opacity"
          >
            <BarkfestMark inverted size={28} />
            <span
              className="font-heading font-bold text-white"
              style={{ fontSize: '20px' }}
            >
              Barkfest
            </span>
          </Link>

          {/* Right side — Admin */}
          {isAuthenticated && accountType === 'admin' ? (
            <div className="flex items-center gap-3">
              <span className="text-sm text-white/80 font-medium hidden sm:block">
                Logged in as Administrator
              </span>
              <button
                onClick={handleSignOut}
                className="flex items-center gap-1.5 text-sm font-medium text-white/90 hover:text-white transition-colors px-3 py-1.5 rounded-full hover:bg-white/10"
              >
                <LogOut className="w-4 h-4" />
                <span className="hidden sm:inline">Sign Out</span>
              </button>
            </div>
          ) : isAuthenticated && accountType === 'owner' ? (
            /* Owner state */
            <div className="flex items-center gap-2">
              {/* + Post a Pet button */}
              <button
                onClick={() => setAddPetOpen(true)}
                className="w-11 h-11 rounded-full bg-white flex items-center justify-center text-primary hover:bg-white/90 transition-colors shadow-sm"
                title="Post a Pet"
              >
                <Plus className="w-5 h-5" />
              </button>

              {/* Avatar with dropdown */}
              <div className="relative" ref={dropdownRef}>
                <button
                  onClick={() => setDropdownOpen(o => !o)}
                  className="w-11 h-11 rounded-full overflow-hidden border-2 border-white/50 hover:border-white transition-colors"
                >
                  {profileImageBlobName ? (
                    <img
                      src={getBlobImageUrl(profileImageBlobName, 'owner-profile-images')}
                      alt="Profile"
                      className="w-full h-full object-cover"
                    />
                  ) : (
                    <div className="w-full h-full bg-white/20 flex items-center justify-center">
                      <UserCircle className="w-7 h-7 text-white" />
                    </div>
                  )}
                </button>

                {dropdownOpen && (
                  <div
                    className="absolute right-0 top-[calc(100%+8px)] w-48 rounded-2xl overflow-hidden"
                    style={{
                      background: 'var(--card)',
                      border: '1px solid var(--border)',
                      boxShadow: '0 16px 48px rgba(0,0,0,0.12)',
                    }}
                  >
                    <button
                      onClick={() => { setDropdownOpen(false); navigate('/manage') }}
                      className="w-full flex items-center gap-3 px-4 py-3 text-sm font-medium text-left hover:bg-secondary transition-colors"
                      style={{ color: 'var(--foreground)' }}
                    >
                      <PawPrint className="w-4 h-4 text-primary shrink-0" />
                      My Pets
                    </button>
                    <button
                      onClick={() => { setDropdownOpen(false); setProfileOpen(true) }}
                      className="w-full flex items-center gap-3 px-4 py-3 text-sm font-medium text-left hover:bg-secondary transition-colors"
                      style={{ color: 'var(--foreground)' }}
                    >
                      <Settings className="w-4 h-4 text-primary shrink-0" />
                      Edit Profile
                    </button>
                    <div className="mx-4 border-t" style={{ borderColor: 'var(--border)' }} />
                    <button
                      onClick={() => { setDropdownOpen(false); handleSignOut() }}
                      className="w-full flex items-center gap-3 px-4 py-3 text-sm font-medium text-left hover:bg-secondary transition-colors"
                      style={{ color: 'var(--muted-foreground)' }}
                    >
                      <LogOut className="w-4 h-4 shrink-0" />
                      Sign Out
                    </button>
                  </div>
                )}
              </div>
            </div>
          ) : (
            /* Guest state */
            <div className="flex items-center gap-2">
              <Link
                to="/login"
                className="px-4 py-2 text-sm font-medium text-white hover:bg-white/10 rounded-full transition-colors"
              >
                Sign In
              </Link>
              <Link
                to="/register"
                className={cn(
                  'px-4 py-2 text-sm font-medium text-white rounded-full transition-colors hover:bg-white/10',
                  'hidden sm:block'
                )}
                style={{ border: '1.5px solid rgba(255,255,255,0.55)' }}
              >
                Get started
              </Link>
            </div>
          )}
        </nav>
      </div>

      {addPetOpen && <AddPetDialog onClose={() => setAddPetOpen(false)} onSuccess={handlePetAdded} />}
      {profileOpen && <UpdateOwnerProfileDialog onClose={() => setProfileOpen(false)} />}
    </>
  )
}
