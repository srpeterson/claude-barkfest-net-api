import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { LogOut, Plus, UserCircle } from 'lucide-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { cn } from '@/lib/utils'
import { useAuth } from '@/hooks/useAuth'
import { useIsMobile } from '@/hooks/useIsMobile'
import { logout, getBrowseBreeds, getBrowsePetTypes, getOwnerById, getOwnerPets } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { getPetTypeLabel, MAX_PETS_PER_OWNER } from '@/config/petTypes'
import { BarkfestMark } from '@/components/BarkfestMark'
import { AddPetDialog } from '@/components/AddPetDialog'
import { UpdateOwnerProfileDialog } from '@/components/UpdateOwnerProfileDialog'
import { PetTypeBreedSelector } from '@/components/PetTypeBreedSelector'
import { MobileFilterSheet } from '@/components/MobileFilterSheet'

interface FilterProps {
  petTypeValue: number
  onPetTypeChange: (value: number) => void
  breedValue: number
  onBreedChange: (value: number) => void
}

interface NavbarProps {
  filterProps?: FilterProps
  maxWidth?: string
}

function FilterIcon() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <line x1="4" y1="6" x2="20" y2="6"/>
      <line x1="8" y1="12" x2="16" y2="12"/>
      <line x1="11" y1="18" x2="13" y2="18"/>
    </svg>
  )
}

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

export function Navbar({ filterProps, maxWidth = 'max-w-[72rem]' }: NavbarProps) {
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
  const [addPetOpen, setAddPetOpen]     = useState(false)
  const [profileOpen, setProfileOpen]   = useState(false)
  const [dropdownOpen, setDropdownOpen] = useState(false)
  const dropdownRef = useRef<HTMLDivElement>(null)
  const queryClient = useQueryClient()

  const { data: ownerPets, isLoading: isLoadingPets } = useQuery({
    queryKey: ['owner', 'pets', accountId],
    queryFn: () => getOwnerPets(accountId!),
    enabled: isAuthenticated && accountType === 'owner' && !!accountId,
    staleTime: 30 * 1000,
  })

  const atPetLimit = (ownerPets?.length ?? 0) >= MAX_PETS_PER_OWNER
  const addPetDisabled = isLoadingPets || atPetLimit

  const [sheetOpen, setSheetOpen] = useState(false)
  const hasActiveFilters = !!filterProps && (filterProps.petTypeValue !== 0 || filterProps.breedValue !== 0)

  // Queries for the desktop pill label
  const { data: petTypes = [] } = useQuery({
    queryKey: ['browse', 'pet-types'],
    queryFn: getBrowsePetTypes,
    enabled: !!filterProps,
    staleTime: Infinity,
  })

  // Fetch breeds for the active type so the mobile pill label resolves correctly
  const { data: activeBreeds = [] } = useQuery({
    queryKey: ['browse', 'breeds', filterProps?.petTypeValue ?? 0],
    queryFn: () => getBrowseBreeds(filterProps!.petTypeValue),
    enabled: !!filterProps && !!filterProps.petTypeValue,
    staleTime: Infinity,
  })

  function getMobileFilterLabel() {
    if (!filterProps || !filterProps.petTypeValue) return 'All Pets'
    const pt = petTypes.find(p => p.value === filterProps.petTypeValue)
    const typeName = pt ? getPetTypeLabel(pt.name) : 'Pets'
    if (!filterProps.breedValue) return typeName
    const breed = activeBreeds.find(b => b.value === filterProps.breedValue)
    return breed ? breed.name : typeName
  }

  function handlePetAdded() {
    queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
    queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
    queryClient.invalidateQueries({ queryKey: ['owner', 'pets', accountId] })
  }

  async function handleSignOut() {
    setDropdownOpen(false)
    await logout()
    signOut()
    navigate('/login?signed-out=true')
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
      <nav className="sticky top-0 z-50 border-b border-border backdrop-blur-md"
        style={{ background: 'rgba(250,247,244,0.85)' }}>
        <div className={`${maxWidth} mx-auto px-5 h-16 flex items-center relative`}>

          {/* Logo */}
          <Link to="/" className="flex items-center gap-2 no-underline shrink-0">
            <BarkfestMark size={28} />
            <span className="font-heading text-xl font-semibold tracking-[-0.02em] text-foreground">
              Barkfest
            </span>
          </Link>

          {/* ── Desktop filter — absolutely centered ── */}
          {filterProps && !isMobile && (
            <div className="absolute left-1/2 -translate-x-1/2 flex items-center gap-2.5 pointer-events-auto">
              <span className="text-sm font-bold text-primary whitespace-nowrap">Show me:</span>
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
            <div className="flex-1 flex justify-center px-2">
              <button
                onClick={() => setSheetOpen(true)}
                className={cn(
                  'h-9 px-3.5 rounded-[18px] flex items-center gap-1.5 border-[1.5px] text-[13px] cursor-pointer whitespace-nowrap max-w-[160px] overflow-hidden transition-colors',
                  hasActiveFilters
                    ? 'border-primary bg-primary/10 text-primary font-semibold'
                    : 'border-border bg-card text-foreground font-normal'
                )}
              >
                <FilterIcon />
                <span className="overflow-hidden text-ellipsis whitespace-nowrap">
                  {getMobileFilterLabel()}
                </span>
                {hasActiveFilters && (
                  <span className="w-1.5 h-1.5 rounded-full bg-primary shrink-0" />
                )}
              </button>
            </div>
          )}

          {/* ── Right side ── */}
          <div className={cn('shrink-0', filterProps && isMobile ? '' : 'ml-auto')}>

            {/* Admin */}
            {isAuthenticated && accountType === 'admin' ? (
              <div className="flex items-center gap-2">
                {!isMobile && (
                  <span className="text-[13px] font-medium text-muted-foreground">Administrator</span>
                )}
                <button
                  onClick={handleSignOut}
                  className="h-8 px-3 rounded-lg border-0 bg-transparent text-muted-foreground text-[13px] font-medium cursor-pointer flex items-center gap-1.5 hover:text-foreground transition-colors"
                >
                  <LogOut className="w-4 h-4" />
                  Sign Out
                </button>
              </div>

            ) : isAuthenticated && accountType === 'owner' ? (
              <div className="flex items-center gap-3">

                {/* Add Pet button */}
                {isMobile ? (
                  <button
                    onClick={() => setAddPetOpen(true)}
                    aria-label="Add Pet"
                    disabled={addPetDisabled}
                    title={atPetLimit ? `You've reached the ${MAX_PETS_PER_OWNER} pet limit` : undefined}
                    className="w-9 h-9 rounded-[10px] border-0 bg-primary text-white cursor-pointer flex items-center justify-center hover:opacity-90 transition-opacity disabled:opacity-40 disabled:cursor-not-allowed"
                  >
                    <Plus className="w-4 h-4" />
                  </button>
                ) : (
                  <button
                    onClick={() => setAddPetOpen(true)}
                    disabled={addPetDisabled}
                    title={atPetLimit ? `You've reached the ${MAX_PETS_PER_OWNER} pet limit` : undefined}
                    className="h-8 px-3.5 rounded-lg border-0 bg-primary text-white text-[13px] font-semibold cursor-pointer flex items-center gap-1.5 whitespace-nowrap hover:opacity-90 transition-opacity disabled:opacity-40 disabled:cursor-not-allowed"
                  >
                    <Plus className="w-3.5 h-3.5" />
                    Add Pet
                  </button>
                )}

                {/* Avatar + dropdown */}
                <div className="relative" ref={dropdownRef}>
                  {dropdownOpen && (
                    <div
                      onClick={() => setDropdownOpen(false)}
                      className="fixed inset-0 z-[298]"
                    />
                  )}
                  <button
                    onClick={e => { e.stopPropagation(); setDropdownOpen(v => !v) }}
                    aria-label="Account menu"
                    className={cn(
                      'w-9 h-9 rounded-full border-0 cursor-pointer bg-secondary text-muted-foreground flex items-center justify-center overflow-hidden relative z-[300] p-0 transition-[outline]',
                      dropdownOpen ? 'outline outline-2 outline-primary outline-offset-2' : 'outline-none'
                    )}
                  >
                    <UserCircle className="w-6 h-6" />
                    {displayBlobName && (
                      <img
                        src={getBlobImageUrl(displayBlobName, 'owner-profile-images')}
                        alt="Profile"
                        onError={e => { e.currentTarget.style.display = 'none' }}
                        className="absolute inset-0 w-full h-full object-cover"
                      />
                    )}
                  </button>

                  {dropdownOpen && (
                    <div className="absolute top-[calc(100%+8px)] right-0 w-[172px] bg-card border border-border rounded-xl shadow-[0_8px_32px_rgba(0,0,0,0.13)] overflow-hidden z-[300]">
                      {[
                        {
                          label: 'My Pets',
                          icon: <PawIcon />,
                          action: () => { setDropdownOpen(false); navigate('/manage') },
                        },
                        {
                          label: 'Edit Profile',
                          icon: <UserCircle className="w-4 h-4" />,
                          action: () => { setDropdownOpen(false); setProfileOpen(true) },
                        },
                        {
                          label: 'Sign Out',
                          icon: <LogOut className="w-4 h-4" />,
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
              <div className="flex items-center gap-1.5">
                <GhostBtn onClick={() => navigate('/login')}>Sign In</GhostBtn>
                <OutlinedBtn onClick={() => navigate('/register')}>Get started</OutlinedBtn>
              </div>
            )}
          </div>
        </div>
      </nav>

      {/* ── Mobile filter bottom sheet ── */}
      {filterProps && sheetOpen && (
        <MobileFilterSheet
          filterProps={filterProps}
          onClose={() => setSheetOpen(false)}
        />
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

function GhostBtn({ children, onClick }: { children: React.ReactNode; onClick: () => void }) {
  return (
    <button
      onClick={onClick}
      className="h-8 px-3 rounded-lg border-0 bg-transparent text-foreground text-[13px] font-medium cursor-pointer flex items-center whitespace-nowrap hover:bg-primary/20 transition-colors"
    >
      {children}
    </button>
  )
}

function OutlinedBtn({ children, onClick }: { children: React.ReactNode; onClick: () => void }) {
  return (
    <button
      onClick={onClick}
      className="h-8 px-3.5 rounded-lg border-[1.5px] border-primary/40 bg-transparent text-primary text-[13px] font-medium cursor-pointer flex items-center whitespace-nowrap hover:bg-primary/10 hover:border-primary transition-colors"
    >
      {children}
    </button>
  )
}

function DropdownItem({
  label, icon, onClick, dividerAbove,
}: {
  label: string
  icon: React.ReactNode
  onClick: () => void
  dividerAbove?: boolean
}) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'w-full h-10 px-3.5 flex items-center gap-[9px] bg-transparent border-0 cursor-pointer text-[13px] font-medium text-left hover:bg-muted transition-colors',
        label === 'Sign Out' ? 'text-muted-foreground' : 'text-foreground',
        dividerAbove ? 'border-t border-border' : ''
      )}
    >
      <span className={cn('flex', label === 'Sign Out' ? 'text-muted-foreground' : 'text-primary')}>
        {icon}
      </span>
      {label}
    </button>
  )
}
