import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { LogOut, Plus, UserCircle } from 'lucide-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/hooks/useAuth'
import { useIsMobile } from '@/hooks/useIsMobile'
import { logout, getBrowseBreeds, getBrowsePetTypes, getOwnerById } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { getPetTypeLabel } from '@/config/petTypes'
import { BarkfestMark } from '@/components/BarkfestMark'
import { AddPetDialog } from '@/components/AddPetDialog'
import { UpdateOwnerProfileDialog } from '@/components/UpdateOwnerProfileDialog'
import { PetTypeBreedSelector } from '@/components/PetTypeBreedSelector'

interface FilterProps {
  petTypeValue: number
  onPetTypeChange: (value: number) => void
  breedValue: number
  onBreedChange: (value: number) => void
}

interface NavbarProps {
  filterProps?: FilterProps
}

// ── Inline SVG icons for the mobile filter sheet ──────────────────────────────

function FilterIcon() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <line x1="4" y1="6" x2="20" y2="6"/>
      <line x1="8" y1="12" x2="16" y2="12"/>
      <line x1="11" y1="18" x2="13" y2="18"/>
    </svg>
  )
}

function XIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
      <path d="M18 6 6 18"/><path d="m6 6 12 12"/>
    </svg>
  )
}

function SearchIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
      <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
    </svg>
  )
}

function CheckIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><polyline points="20 6 9 17 4 12"/></svg>
  )
}

// ── Lucide-compatible paw icon for the owner dropdown ─────────────────────────

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

// ─────────────────────────────────────────────────────────────────────────────

export function Navbar({ filterProps }: NavbarProps) {
  const navigate = useNavigate()
  const isMobile = useIsMobile()
  const { isAuthenticated, accountType, profileImageBlobName, accountId, signOut } = useAuth()

  // Keep the profile image in sync with the server so changes on another
  // device (or another tab) are picked up when the user returns to this one.
  const { data: freshBlobName } = useQuery({
    queryKey: ['owner', accountId, 'profile-image'],
    queryFn: async () => {
      const owner = await getOwnerById(accountId!)
      return owner.profileImage?.blobName ?? null
    },
    enabled: isAuthenticated && accountType === 'owner' && !!accountId,
    staleTime: 30 * 1000,
    initialData: profileImageBlobName,
  })

  const displayBlobName = freshBlobName ?? profileImageBlobName
  const [addPetOpen, setAddPetOpen] = useState(false)
  const [profileOpen, setProfileOpen] = useState(false)
  const [dropdownOpen, setDropdownOpen] = useState(false)
  const dropdownRef = useRef<HTMLDivElement>(null)
  const queryClient = useQueryClient()

  // ── Mobile filter sheet state ──────────────────────────────────────────────
  const [sheetOpen, setSheetOpen] = useState(false)
  const [pendingType, setPendingType] = useState(filterProps?.petTypeValue ?? 0)
  const [pendingBreed, setPendingBreed] = useState(filterProps?.breedValue ?? 0)
  const [breedSearch, setBreedSearch] = useState('')

  const hasActiveFilters = !!filterProps && (filterProps.petTypeValue !== 0 || filterProps.breedValue !== 0)

  // ── Queries for filter (used on mobile sheet + desktop pill label) ─────────
  const { data: petTypes = [] } = useQuery({
    queryKey: ['browse', 'pet-types'],
    queryFn: getBrowsePetTypes,
    enabled: !!filterProps,
    staleTime: Infinity,
  })

  const { data: sheetBreeds = [] } = useQuery({
    queryKey: ['browse', 'breeds', pendingType],
    queryFn: () => getBrowseBreeds(pendingType),
    enabled: !!filterProps && !!pendingType,
    staleTime: Infinity,
  })

  // Also fetch breeds for the active type so the pill label resolves correctly
  const { data: activeBreeds = [] } = useQuery({
    queryKey: ['browse', 'breeds', filterProps?.petTypeValue ?? 0],
    queryFn: () => getBrowseBreeds(filterProps!.petTypeValue),
    enabled: !!filterProps && !!filterProps.petTypeValue,
    staleTime: Infinity,
  })

  const filteredSheetBreeds = [
    { name: 'All Breeds', value: 0 } as const,
    ...sheetBreeds
      .filter(b => b.name.toLowerCase().includes(breedSearch.toLowerCase()))
      .sort((a, b) => {
        if (a.name === 'Other') return 1
        if (b.name === 'Other') return -1
        if (a.name === 'Mixed') return 1
        if (b.name === 'Mixed') return -1
        return a.name.localeCompare(b.name)
      }),
  ]

  // ── Helpers ────────────────────────────────────────────────────────────────

  function getMobileFilterLabel() {
    if (!filterProps || !filterProps.petTypeValue) return 'All Pets'
    const pt = petTypes.find(p => p.value === filterProps.petTypeValue)
    const typeName = pt ? getPetTypeLabel(pt.name) : 'Pets'
    if (!filterProps.breedValue) return typeName
    const breed = activeBreeds.find(b => b.value === filterProps.breedValue)
    return breed ? breed.name : typeName
  }

  function openSheet() {
    setPendingType(filterProps?.petTypeValue ?? 0)
    setPendingBreed(filterProps?.breedValue ?? 0)
    setBreedSearch('')
    setSheetOpen(true)
  }

  function applyFilters() {
    if (!filterProps) return
    if (pendingType !== filterProps.petTypeValue) {
      filterProps.onPetTypeChange(pendingType)
      filterProps.onBreedChange(0)
    } else {
      filterProps.onBreedChange(pendingBreed)
    }
    setSheetOpen(false)
  }

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

  // ── Render ─────────────────────────────────────────────────────────────────

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
            position: 'relative',
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
              flexShrink: 0,
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

          {/* ── Desktop filter — absolutely centered ── */}
          {filterProps && !isMobile && (
            <div
              style={{
                position: 'absolute',
                left: '50%',
                transform: 'translateX(-50%)',
                display: 'flex',
                alignItems: 'center',
                gap: 10,
                pointerEvents: 'auto',
              }}
            >
              <span
                style={{
                  fontSize: 14,
                  fontWeight: 700,
                  color: 'var(--primary)',
                  whiteSpace: 'nowrap',
                }}
              >
                Show me:
              </span>
              <PetTypeBreedSelector
                petTypeValue={filterProps.petTypeValue}
                onPetTypeChange={filterProps.onPetTypeChange}
                breedValue={filterProps.breedValue}
                onBreedChange={filterProps.onBreedChange}
                petTypeClassName="w-36"
                breedClassName="w-44"
              />
            </div>
          )}

          {/* ── Mobile filter pill — centered between logo and auth ── */}
          {filterProps && isMobile && (
            <div
              style={{
                flex: 1,
                display: 'flex',
                justifyContent: 'center',
                padding: '0 8px',
              }}
            >
              <button
                onClick={openSheet}
                style={{
                  height: 36,
                  padding: '0 14px',
                  borderRadius: 18,
                  display: 'flex',
                  alignItems: 'center',
                  gap: 6,
                  border: `1.5px solid ${hasActiveFilters ? 'var(--primary)' : 'var(--border)'}`,
                  background: hasActiveFilters ? 'var(--primary-10)' : 'var(--card)',
                  color: hasActiveFilters ? 'var(--primary)' : 'var(--foreground)',
                  fontFamily: "'DM Sans', sans-serif",
                  fontSize: 13,
                  fontWeight: hasActiveFilters ? 600 : 400,
                  cursor: 'pointer',
                  whiteSpace: 'nowrap',
                  maxWidth: 160,
                  overflow: 'hidden',
                }}
              >
                <FilterIcon />
                <span
                  style={{
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap',
                  }}
                >
                  {getMobileFilterLabel()}
                </span>
                {hasActiveFilters && (
                  <span
                    style={{
                      width: 6,
                      height: 6,
                      borderRadius: '50%',
                      background: 'var(--primary)',
                      flexShrink: 0,
                    }}
                  />
                )}
              </button>
            </div>
          )}

          {/* ── Right side — push to end ── */}
          <div style={{ marginLeft: filterProps && isMobile ? 0 : 'auto', flexShrink: 0 }}>
            {/* Admin */}
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
                      width: 36,
                      height: 36,
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
                    <Plus style={{ width: 16, height: 16 }} />
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
                      width: 36,
                      height: 36,
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
                    <UserCircle style={{ width: 24, height: 24 }} />
                    {displayBlobName && (
                      <img
                        src={getBlobImageUrl(displayBlobName, 'owner-profile-images')}
                        alt="Profile"
                        onError={e => { e.currentTarget.style.display = 'none' }}
                        style={{ position: 'absolute', inset: 0, width: '100%', height: '100%', objectFit: 'cover' }}
                      />
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
        </div>
      </nav>

      {/* ── Mobile filter bottom sheet ───────────────────────────────────────── */}
      {filterProps && sheetOpen && (
        <>
          {/* Backdrop */}
          <div
            onClick={() => setSheetOpen(false)}
            className="animate-backdrop-in"
            style={{
              position: 'fixed',
              inset: 0,
              zIndex: 200,
              background: 'rgba(0,0,0,0.3)',
              backdropFilter: 'blur(2px)',
            }}
          />

          {/* Sheet */}
          <div
            className="animate-sheet-in"
            style={{
              position: 'fixed',
              bottom: 0,
              left: 0,
              right: 0,
              zIndex: 201,
              background: 'var(--card)',
              borderRadius: '20px 20px 0 0',
              boxShadow: '0 -8px 40px rgba(0,0,0,0.15)',
              display: 'flex',
              flexDirection: 'column',
              maxHeight: '80vh',
            }}
          >
            {/* Drag handle */}
            <div style={{ display: 'flex', justifyContent: 'center', padding: '12px 0 4px', flexShrink: 0 }}>
              <div style={{ width: 36, height: 4, borderRadius: 2, background: 'var(--border)' }} />
            </div>

            {/* Header */}
            <div
              style={{
                padding: '8px 20px 14px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                flexShrink: 0,
              }}
            >
              <h3
                className="font-heading"
                style={{ margin: 0, fontSize: 18, fontWeight: 700, color: 'var(--foreground)' }}
              >
                Filters
              </h3>
              <button
                onClick={() => setSheetOpen(false)}
                style={{
                  width: 32,
                  height: 32,
                  borderRadius: 8,
                  border: 'none',
                  background: 'var(--secondary)',
                  cursor: 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'var(--muted-foreground)',
                }}
              >
                <XIcon />
              </button>
            </div>

            {/* Scrollable content */}
            <div style={{ overflowY: 'auto', padding: '0 20px', flex: 1 }}>
              {/* Pet Type chips */}
              <div style={{ marginBottom: 20 }}>
                <p
                  style={{
                    margin: '0 0 10px',
                    fontSize: 11,
                    fontWeight: 700,
                    letterSpacing: '0.08em',
                    textTransform: 'uppercase',
                    color: 'var(--muted-foreground)',
                  }}
                >
                  Pet Type
                </p>
                <div style={{ display: 'flex', gap: 8 }}>
                  {[
                    { value: 0, label: 'All Pets' },
                    ...petTypes.map(pt => ({ value: pt.value, label: getPetTypeLabel(pt.name) })),
                  ].map(({ value, label }) => (
                    <button
                      key={value}
                      onClick={() => { setPendingType(value); setPendingBreed(0) }}
                      style={{
                        height: 40,
                        padding: '0 18px',
                        borderRadius: 20,
                        border: `1.5px solid ${pendingType === value ? 'var(--primary)' : 'var(--border)'}`,
                        background: pendingType === value ? 'var(--primary-10)' : 'transparent',
                        color: pendingType === value ? 'var(--primary)' : 'var(--foreground)',
                        fontFamily: "'DM Sans', sans-serif",
                        fontSize: 13,
                        fontWeight: pendingType === value ? 600 : 400,
                        cursor: 'pointer',
                        transition: 'all 0.15s',
                      }}
                    >
                      {label}
                    </button>
                  ))}
                </div>
              </div>

              {/* Breed list — only when a type is selected */}
              {!!pendingType && (
                <div style={{ marginBottom: 16 }}>
                  <p
                    style={{
                      margin: '0 0 10px',
                      fontSize: 11,
                      fontWeight: 700,
                      letterSpacing: '0.08em',
                      textTransform: 'uppercase',
                      color: 'var(--muted-foreground)',
                    }}
                  >
                    Breed
                    <span
                      style={{
                        fontWeight: 400,
                        textTransform: 'none',
                        letterSpacing: 0,
                        marginLeft: 6,
                        fontSize: 12,
                      }}
                    >
                      ({sheetBreeds.length} breeds)
                    </span>
                  </p>

                  {/* Search box */}
                  <div style={{ position: 'relative', marginBottom: 8 }}>
                    <span
                      style={{
                        position: 'absolute',
                        left: 12,
                        top: '50%',
                        transform: 'translateY(-50%)',
                        color: 'var(--muted-foreground)',
                        pointerEvents: 'none',
                        display: 'flex',
                      }}
                    >
                      <SearchIcon />
                    </span>
                    <input
                      placeholder="Search breeds…"
                      value={breedSearch}
                      onChange={e => setBreedSearch(e.target.value)}
                      style={{
                        width: '100%',
                        height: 40,
                        borderRadius: 10,
                        border: '1.5px solid var(--border)',
                        background: 'var(--secondary)',
                        padding: '0 36px 0 34px',
                        fontFamily: "'DM Sans', sans-serif",
                        fontSize: 13,
                        outline: 'none',
                        boxSizing: 'border-box',
                        color: 'var(--foreground)',
                      }}
                    />
                    {breedSearch && (
                      <button
                        onClick={() => setBreedSearch('')}
                        style={{
                          position: 'absolute',
                          right: 10,
                          top: '50%',
                          transform: 'translateY(-50%)',
                          background: 'none',
                          border: 'none',
                          cursor: 'pointer',
                          color: 'var(--muted-foreground)',
                          padding: 0,
                          display: 'flex',
                        }}
                      >
                        <XIcon />
                      </button>
                    )}
                  </div>

                  {/* Breed list */}
                  <div
                    style={{
                      border: '1.5px solid var(--border)',
                      borderRadius: 12,
                      overflow: 'hidden',
                      maxHeight: 220,
                      overflowY: 'auto',
                    }}
                  >
                    {filteredSheetBreeds.length === 0 ? (
                      <p
                        style={{
                          padding: '14px 16px',
                          margin: 0,
                          fontSize: 13,
                          color: 'var(--muted-foreground)',
                          textAlign: 'center',
                        }}
                      >
                        No breeds found
                      </p>
                    ) : (
                      filteredSheetBreeds.map((b, i) => (
                        <button
                          key={b.value}
                          onClick={() => setPendingBreed(b.value)}
                          style={{
                            width: '100%',
                            height: 44,
                            padding: '0 16px',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'space-between',
                            background: pendingBreed === b.value ? 'var(--primary-10)' : 'transparent',
                            border: 'none',
                            borderBottom:
                              i < filteredSheetBreeds.length - 1 ? '1px solid var(--border)' : 'none',
                            cursor: 'pointer',
                            fontFamily: "'DM Sans', sans-serif",
                            fontSize: 14,
                            fontWeight: pendingBreed === b.value ? 600 : 400,
                            color: pendingBreed === b.value ? 'var(--primary)' : 'var(--foreground)',
                            textAlign: 'left',
                          }}
                        >
                          {b.name}
                          {pendingBreed === b.value && (
                            <span style={{ color: 'var(--primary)', display: 'flex' }}>
                              <CheckIcon />
                            </span>
                          )}
                        </button>
                      ))
                    )}
                  </div>
                </div>
              )}
            </div>

            {/* Show results CTA */}
            <div style={{ padding: '12px 20px 32px', flexShrink: 0 }}>
              <button
                onClick={applyFilters}
                style={{
                  width: '100%',
                  height: 48,
                  borderRadius: 12,
                  border: 'none',
                  background: 'var(--primary)',
                  color: '#fff',
                  fontFamily: "'DM Sans', sans-serif",
                  fontSize: 15,
                  fontWeight: 600,
                  cursor: 'pointer',
                }}
              >
                Show results
              </button>
            </div>
          </div>
        </>
      )}

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
