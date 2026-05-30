import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { LogOut, Plus, UserCircle } from 'lucide-react'
import { useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/hooks/useAuth'
import { useIsMobile } from '@/hooks/useIsMobile'
import { logout } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { BarkfestMark } from '@/components/BarkfestMark'
import { AddPetDialog } from '@/components/AddPetDialog'
import { UpdateOwnerProfileDialog } from '@/components/UpdateOwnerProfileDialog'

// Lucide-compatible paw icon (filled) for the dropdown
function PawIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
      <circle cx="5.5" cy="12.5" r="2" />
      <circle cx="9.5" cy="7.5" r="2" />
      <circle cx="14.5" cy="7.5" r="2" />
      <circle cx="18.5" cy="12.5" r="2" />
      <path d="M12 12c-2.5 0-4.5 2-4.5 4.2 0 1.6 1.2 2.3 2.4 2.3.9 0 1.4-.4 2.1-.4s1.2.4 2.1.4c1.2 0 2.4-.7 2.4-2.3C16.5 14 14.5 12 12 12z" />
    </svg>
  )
}

export function Navbar() {
  const navigate = useNavigate()
  const isMobile = useIsMobile()
  const { isAuthenticated, accountType, profileImageBlobName, signOut } = useAuth()
  const [addPetOpen, setAddPetOpen] = useState(false)
  const [profileOpen, setProfileOpen] = useState(false)
  const [dropdownOpen, setDropdownOpen] = useState(false)
  const dropdownRef = useRef<HTMLDivElement>(null)
  const queryClient = useQueryClient()

  function handlePetAdded() {
    queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
    queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
    queryClient.invalidateQueries({ queryKey: ['owner', 'pets'] })
  }

  async function handleSignOut() {
    setDropdownOpen(false)
    await logout()
    signOut()
    navigate('/')
  }

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setDropdownOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [])

  return (
    <>
      <nav
        style={{
          position: 'sticky',
          top: 0,
          zIndex: 50,
          backdropFilter: 'blur(12px)',
          WebkitBackdropFilter: 'blur(12px)',
          background: 'rgba(250,247,244,0.85)',
          borderBottom: '1px solid var(--border)',
        }}
      >
        <div
          style={{
            maxWidth: '72rem',
            margin: '0 auto',
            padding: '0 20px',
            height: 64,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
          }}
        >
          {/* Logo */}
          <Link
            to="/"
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 8,
              textDecoration: 'none',
            }}
          >
            <BarkfestMark size={28} />
            <span
              className="font-heading"
              style={{
                fontSize: 20,
                fontWeight: 600,
                letterSpacing: '-0.02em',
                color: 'var(--foreground)',
              }}
            >
              Barkfest
            </span>
          </Link>

          {/* Right — Admin */}
          {isAuthenticated && accountType === 'admin' ? (
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              {!isMobile && (
                <span
                  style={{
                    fontSize: 13,
                    fontWeight: 500,
                    color: 'var(--muted-foreground)',
                  }}
                >
                  Administrator
                </span>
              )}
              <button
                onClick={handleSignOut}
                style={{
                  height: 32,
                  padding: '0 12px',
                  borderRadius: 8,
                  border: 'none',
                  background: 'transparent',
                  color: 'var(--muted-foreground)',
                  fontFamily: "'DM Sans', sans-serif",
                  fontSize: 13,
                  fontWeight: 500,
                  cursor: 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  gap: 6,
                }}
              >
                <LogOut style={{ width: 16, height: 16 }} />
                Sign Out
              </button>
            </div>

          ) : isAuthenticated && accountType === 'owner' ? (
            /* Owner */
            <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
              {/* Add Pet button */}
              {isMobile ? (
                <button
                  onClick={() => setAddPetOpen(true)}
                  aria-label="Add Pet"
                  style={{
                    width: 44,
                    height: 44,
                    borderRadius: 10,
                    border: 'none',
                    background: 'var(--primary)',
                    color: '#fff',
                    cursor: 'pointer',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  <Plus style={{ width: 18, height: 18 }} />
                </button>
              ) : (
                <button
                  onClick={() => setAddPetOpen(true)}
                  style={{
                    height: 32,
                    padding: '0 14px',
                    borderRadius: 8,
                    border: 'none',
                    background: 'var(--primary)',
                    color: '#fff',
                    fontFamily: "'DM Sans', sans-serif",
                    fontSize: 13,
                    fontWeight: 600,
                    cursor: 'pointer',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 6,
                    whiteSpace: 'nowrap',
                  }}
                >
                  <Plus style={{ width: 14, height: 14 }} />
                  Add Pet
                </button>
              )}

              {/* Avatar + dropdown */}
              <div style={{ position: 'relative' }} ref={dropdownRef}>
                {dropdownOpen && (
                  <div
                    onClick={() => setDropdownOpen(false)}
                    style={{ position: 'fixed', inset: 0, zIndex: 298 }}
                  />
                )}
                <button
                  onClick={e => { e.stopPropagation(); setDropdownOpen(v => !v) }}
                  aria-label="Account menu"
                  style={{
                    width: 44,
                    height: 44,
                    borderRadius: '50%',
                    border: 'none',
                    cursor: 'pointer',
                    background: 'var(--secondary)',
                    color: 'var(--muted-foreground)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    overflow: 'hidden',
                    outline: dropdownOpen ? '2px solid var(--primary)' : 'none',
                    outlineOffset: 2,
                    position: 'relative',
                    zIndex: 300,
                    padding: 0,
                  }}
                >
                  {profileImageBlobName ? (
                    <img
                      src={getBlobImageUrl(profileImageBlobName, 'owner-profile-images')}
                      alt="Profile"
                      style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                    />
                  ) : (
                    <UserCircle style={{ width: 28, height: 28 }} />
                  )}
                </button>

                {dropdownOpen && (
                  <div
                    style={{
                      position: 'absolute',
                      top: 'calc(100% + 8px)',
                      right: 0,
                      width: 172,
                      background: 'var(--card)',
                      border: '1px solid var(--border)',
                      borderRadius: 12,
                      boxShadow: '0 8px 32px rgba(0,0,0,0.13)',
                      overflow: 'hidden',
                      zIndex: 300,
                    }}
                  >
                    {[
                      {
                        label: 'My Pets',
                        icon: <PawIcon />,
                        action: () => { setDropdownOpen(false); navigate('/manage') },
                      },
                      {
                        label: 'Edit Profile',
                        icon: <UserCircle style={{ width: 16, height: 16 }} />,
                        action: () => { setDropdownOpen(false); setProfileOpen(true) },
                      },
                      {
                        label: 'Sign Out',
                        icon: <LogOut style={{ width: 16, height: 16 }} />,
                        action: handleSignOut,
                      },
                    ].map(({ label, icon, action }) => (
                      <DropdownItem
                        key={label}
                        label={label}
                        icon={icon}
                        onClick={action}
                        dividerAbove={label === 'Sign Out'}
                      />
                    ))}
                  </div>
                )}
              </div>
            </div>

          ) : (
            /* Guest */
            <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <GhostBtn onClick={() => navigate('/login')}>Sign In</GhostBtn>
              <OutlinedBtn onClick={() => navigate('/register')}>Get started</OutlinedBtn>
            </div>
          )}
        </div>
      </nav>

      {addPetOpen && (
        <AddPetDialog
          onClose={() => setAddPetOpen(false)}
          onSuccess={handlePetAdded}
        />
      )}
      {profileOpen && (
        <UpdateOwnerProfileDialog onClose={() => setProfileOpen(false)} />
      )}
    </>
  )
}

function GhostBtn({
  children,
  onClick,
}: {
  children: React.ReactNode
  onClick: () => void
}) {
  return (
    <button
      onClick={onClick}
      style={{
        height: 32,
        padding: '0 12px',
        borderRadius: 8,
        border: 'none',
        background: 'transparent',
        color: 'var(--foreground)',
        fontFamily: "'DM Sans', sans-serif",
        fontSize: 13,
        fontWeight: 500,
        cursor: 'pointer',
        display: 'flex',
        alignItems: 'center',
        whiteSpace: 'nowrap',
      }}
      onMouseEnter={e => (e.currentTarget.style.background = 'var(--primary-20)')}
      onMouseLeave={e => (e.currentTarget.style.background = 'transparent')}
    >
      {children}
    </button>
  )
}

function OutlinedBtn({
  children,
  onClick,
}: {
  children: React.ReactNode
  onClick: () => void
}) {
  return (
    <button
      onClick={onClick}
      style={{
        height: 32,
        padding: '0 14px',
        borderRadius: 8,
        border: '1.5px solid rgba(223,103,73,0.45)',
        background: 'transparent',
        color: 'var(--primary)',
        fontFamily: "'DM Sans', sans-serif",
        fontSize: 13,
        fontWeight: 500,
        cursor: 'pointer',
        display: 'flex',
        alignItems: 'center',
        whiteSpace: 'nowrap',
      }}
      onMouseEnter={e => {
        e.currentTarget.style.background = 'var(--primary-10)'
        e.currentTarget.style.borderColor = 'var(--primary)'
      }}
      onMouseLeave={e => {
        e.currentTarget.style.background = 'transparent'
        e.currentTarget.style.borderColor = 'rgba(223,103,73,0.45)'
      }}
    >
      {children}
    </button>
  )
}

function DropdownItem({
  label,
  icon,
  onClick,
  dividerAbove,
}: {
  label: string
  icon: React.ReactNode
  onClick: () => void
  dividerAbove?: boolean
}) {
  return (
    <button
      onClick={onClick}
      style={{
        width: '100%',
        height: 40,
        padding: '0 14px',
        display: 'flex',
        alignItems: 'center',
        gap: 9,
        background: 'transparent',
        border: 'none',
        borderTop: dividerAbove ? '1px solid var(--border)' : 'none',
        cursor: 'pointer',
        color: label === 'Sign Out' ? 'var(--muted-foreground)' : 'var(--foreground)',
        fontFamily: "'DM Sans', sans-serif",
        fontSize: 13,
        fontWeight: 500,
        textAlign: 'left',
      }}
      onMouseEnter={e => (e.currentTarget.style.background = 'var(--muted)')}
      onMouseLeave={e => (e.currentTarget.style.background = 'transparent')}
    >
      <span style={{ color: label === 'Sign Out' ? 'var(--muted-foreground)' : 'var(--primary)', display: 'flex' }}>
        {icon}
      </span>
      {label}
    </button>
  )
}
