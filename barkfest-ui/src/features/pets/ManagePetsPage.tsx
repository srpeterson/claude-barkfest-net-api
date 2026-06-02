import { forwardRef, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Eye, Loader2, PawPrint, Pencil, Plus, Trash2 } from 'lucide-react'
import { cn } from '@/lib/utils'
import { deletePet, getOwnerById, getOwnerPets, setOwnerVisibility } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { formatAge } from '@/lib/formatAge'
import { useAuth } from '@/hooks/useAuth'
import { useIsMobile } from '@/hooks/useIsMobile'
import { BarkfestMark } from '@/components/BarkfestMark'
import { Navbar } from '@/components/Navbar'
import { AddPetDialog } from '@/components/AddPetDialog'
import { EditPetModal } from '@/components/EditPetModal'
import type { PetDto } from '@/lib/api'

// ── Switch ────────────────────────────────────────────────────────────
function Switch({ checked, onChange, id }: { checked: boolean; onChange: (v: boolean) => void; id: string }) {
  return (
    <button
      role="switch"
      aria-checked={checked}
      id={id}
      onClick={() => onChange(!checked)}
      className={cn(
        'relative w-[46px] h-[27px] rounded-full border-0 cursor-pointer p-0 shrink-0 transition-colors',
        checked ? 'bg-primary' : 'bg-[#d8cfc6]'
      )}
    >
      <span
        className={cn(
          'absolute top-[3px] w-[21px] h-[21px] rounded-full bg-white shadow-[0_1px_3px_rgba(0,0,0,0.25)] transition-[left]',
          checked ? 'left-[22px]' : 'left-[3px]'
        )}
      />
    </button>
  )
}

// ── ShowInGalleryToggle ───────────────────────────────────────────────
function HidePetsToggle({ isVisible, onChange, error }: { isVisible: boolean; onChange: (v: boolean) => void; error?: string | null }) {
  return (
    <div>
      <div className={cn(
        'flex items-center gap-3.5 bg-card rounded-xl px-4 py-2.5 border transition-colors',
        isVisible ? 'border-primary' : 'border-border'
      )}>
        <div>
          <label htmlFor="show-in-gallery" className="block text-sm font-semibold cursor-pointer">
            Show in gallery
          </label>
          <p className={cn('m-0 text-xs font-medium', isVisible ? 'text-primary' : 'text-muted-foreground font-normal')}>
            {isVisible ? 'Visible to everyone' : 'Hidden for everyone'}
          </p>
        </div>
        <Switch id="show-in-gallery" checked={isVisible} onChange={onChange} />
      </div>
      {error && <p className="text-xs text-destructive mt-1">{error}</p>}
    </div>
  )
}

// ── ManagePetsPage ────────────────────────────────────────────────────
export function ManagePetsPage() {
  const navigate    = useNavigate()
  const queryClient = useQueryClient()
  const isMobile    = useIsMobile()
  const { accountId } = useAuth()

  const [selected, setSelected]               = useState<Set<string>>(new Set())
  const [deleteTarget, setDeleteTarget]       = useState<{ id: string; name: string } | null>(null)
  const [bulkDeleteOpen, setBulkDeleteOpen]   = useState(false)
  const [isDeleting, setIsDeleting]           = useState(false)
  const [addPetOpen, setAddPetOpen]           = useState(false)
  const [editPet, setEditPet]                 = useState<PetDto | null>(null)
  const [optimisticIsVisible, setOptimisticIsVisible] = useState<boolean | null>(null)
  const [visibilityError, setVisibilityError] = useState<string | null>(null)
  const selectAllRef = useRef<HTMLInputElement>(null)

  const { data: ownerData } = useQuery({
    queryKey: ['owner', accountId],
    queryFn: () => getOwnerById(accountId!),
    enabled: !!accountId,
  })

  const isVisible = optimisticIsVisible ?? (ownerData?.isVisible ?? true)

  const { data: rawPets = [], isLoading } = useQuery({
    queryKey: ['owner', 'pets', accountId],
    queryFn: () => getOwnerPets(accountId!),
    enabled: !!accountId,
  })

  const pets = rawPets.slice().sort((a, b) => a.name.localeCompare(b.name))

  if (selectAllRef.current) {
    selectAllRef.current.indeterminate = selected.size > 0 && selected.size < pets.length
  }

  const allSelected = pets.length > 0 && selected.size === pets.length

  function toggleAll() {
    setSelected(allSelected ? new Set() : new Set(pets.map(p => p.petId)))
  }

  function toggleOne(petId: string) {
    setSelected(s => {
      const next = new Set(s)
      if (next.has(petId)) next.delete(petId)
      else next.add(petId)
      return next
    })
  }

  async function handleDeleteOne(petId: string) {
    setIsDeleting(true)
    try {
      await deletePet(petId)
      queryClient.invalidateQueries({ queryKey: ['owner', 'pets', accountId] })
      queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
      queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
    } finally {
      setIsDeleting(false)
      setDeleteTarget(null)
    }
  }

  async function handleBulkDelete() {
    setIsDeleting(true)
    try {
      await Promise.all([...selected].map(id => deletePet(id)))
      setSelected(new Set())
      queryClient.invalidateQueries({ queryKey: ['owner', 'pets', accountId] })
      queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
      queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
    } finally {
      setIsDeleting(false)
      setBulkDeleteOpen(false)
    }
  }

  function handlePetAdded() {
    queryClient.invalidateQueries({ queryKey: ['owner', 'pets', accountId] })
    queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
    queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
  }

  async function handleVisibilityChange(newIsVisible: boolean) {
    const previous = isVisible
    setOptimisticIsVisible(newIsVisible)
    setVisibilityError(null)
    try {
      await setOwnerVisibility(accountId!, newIsVisible)
      queryClient.invalidateQueries({ queryKey: ['owner', accountId] })
    } catch {
      setOptimisticIsVisible(previous)
      setVisibilityError('Failed to update visibility. Please try again.')
    }
  }

  const desktopCols = '48px 56px 1fr 80px 80px 100px'
  const mobileCols  = '40px 52px 1fr 96px'
  const cols = isMobile ? mobileCols : desktopCols

  return (
    <div className="min-h-screen bg-background">
      <Navbar maxWidth="max-w-[900px]" />

      <div className="max-w-[900px] mx-auto px-6 pt-6 pb-16">

        {/* Back nav */}
        <button
          onClick={() => navigate('/')}
          className="inline-flex items-center gap-1.5 text-[13px] text-muted-foreground bg-transparent border-0 cursor-pointer p-0 mb-6 transition-colors hover:text-foreground"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to Barkfest
        </button>

        {/* Page header */}
        <div className="flex items-end justify-between mb-6 flex-wrap gap-4">
          <div>
            <h1 className="font-heading text-[28px] font-bold mb-1">My Pets</h1>
            <p className="text-sm text-muted-foreground m-0">
              {pets.length} {pets.length === 1 ? 'pet' : 'pets'} in your profile
            </p>
          </div>
          <HidePetsToggle isVisible={isVisible} onChange={handleVisibilityChange} error={visibilityError} />
        </div>

        {/* Bulk delete bar */}
        {selected.size > 0 && (
          <div className="sticky top-[68px] z-40 mb-4 bg-primary text-white rounded-xl px-4 h-[52px] flex items-center justify-between">
            <span className="text-sm font-medium">
              {selected.size} pet{selected.size > 1 ? 's' : ''} selected
            </span>
            <div className="flex gap-2.5 items-center">
              <button
                onClick={() => setSelected(new Set())}
                className="h-8 px-3.5 rounded-lg border-[1.5px] border-white bg-white/[0.18] text-white text-[13px] font-semibold cursor-pointer"
              >
                Clear
              </button>
              <button
                onClick={() => setBulkDeleteOpen(true)}
                className="h-8 px-3.5 rounded-lg border-0 bg-white text-destructive text-[13px] font-bold cursor-pointer flex items-center gap-1.5"
              >
                <Trash2 className="w-3.5 h-3.5" />
                Delete {selected.size}
              </button>
            </div>
          </div>
        )}

        {/* Table */}
        <div>
          {isLoading ? (
            <div className="flex justify-center py-16">
              <Loader2 className="w-8 h-8 text-primary animate-spin" />
            </div>
          ) : pets.length === 0 ? (
            <div className="flex flex-col items-center py-20 bg-card border border-border rounded-2xl">
              <PawPrint className="w-12 h-12 mb-4 text-primary opacity-40" />
              <p className="font-heading text-[18px] font-semibold mb-1.5">No pets yet</p>
              <p className="text-sm text-muted-foreground mb-6">
                Share your first pet with the Barkfest community!
              </p>
              <button
                onClick={() => setAddPetOpen(true)}
                className="flex items-center gap-1.5 h-10 px-5 rounded-[10px] border-0 bg-primary text-white text-sm font-semibold cursor-pointer"
              >
                <Plus className="w-4 h-4" />
                Add your first pet
              </button>
            </div>
          ) : (
            <div className="bg-card border border-border rounded-2xl overflow-hidden">

              {/* Desktop header row */}
              {!isMobile && (
                <div
                  className="grid items-center px-4 h-11 border-b border-border bg-primary/10"
                  style={{ gridTemplateColumns: cols }}
                >
                  <div className="flex justify-center">
                    <Checkbox ref={selectAllRef} checked={allSelected} onChange={toggleAll} />
                  </div>
                  <div />
                  {['Name', 'Type', 'Age', 'Actions'].map((h, i) => (
                    <span
                      key={h}
                      className={cn(
                        'text-[11px] font-bold tracking-[0.06em] uppercase text-primary',
                        i === 3 ? 'text-right' : 'text-left'
                      )}
                    >
                      {h}
                    </span>
                  ))}
                </div>
              )}

              {/* Mobile select-all row */}
              {isMobile && (
                <div className="flex items-center gap-2.5 px-3.5 py-2.5 border-b border-border bg-primary/10">
                  <Checkbox ref={selectAllRef} checked={allSelected} onChange={toggleAll} />
                  <span className="text-xs font-bold tracking-[0.06em] uppercase text-primary">
                    {allSelected ? 'Deselect all' : 'Select all'}
                  </span>
                </div>
              )}

              {/* Pet rows */}
              {pets.map((pet, i) => {
                const age      = formatAge(pet.dateOfBirth, 'short')
                const isSel    = selected.has(pet.petId)
                const featured = pet.images.find(img => img.isFeaturedImage) ?? pet.images[0]
                return (
                  <div
                    key={pet.petId}
                    className={cn(
                      'grid items-center transition-colors',
                      isSel ? 'bg-primary/10' : 'bg-transparent',
                      i === 0 ? '' : 'border-t border-border'
                    )}
                    style={{
                      gridTemplateColumns: cols,
                      padding: `0 ${isMobile ? 12 : 16}px`,
                      height: isMobile ? 64 : 60,
                    }}
                  >
                    {/* Checkbox */}
                    <div className="flex justify-center">
                      <Checkbox checked={isSel} onChange={() => toggleOne(pet.petId)} />
                    </div>

                    {/* Thumbnail */}
                    <div
                      className="rounded-lg overflow-hidden border-[1.5px] border-border"
                      style={{ width: isMobile ? 40 : 38, height: isMobile ? 40 : 38 }}
                    >
                      {featured ? (
                        <img
                          src={getBlobImageUrl(featured.blobName)}
                          alt={pet.name}
                          className="w-full h-full object-cover block"
                        />
                      ) : (
                        <div className="w-full h-full bg-secondary flex items-center justify-center">
                          <PawPrint className="w-[18px] h-[18px] text-primary" />
                        </div>
                      )}
                    </div>

                    {/* Name + breed */}
                    <div className="pl-1 min-w-0">
                      <p className={cn('m-0 font-semibold overflow-hidden text-ellipsis whitespace-nowrap', isMobile ? 'text-[13px]' : 'text-sm')}>
                        {pet.name}
                      </p>
                      <p className="m-0 text-[11px] text-muted-foreground overflow-hidden text-ellipsis whitespace-nowrap">
                        {isMobile
                          ? [pet.breed, pet.petType, age].filter(Boolean).join(' · ')
                          : pet.breed}
                      </p>
                    </div>

                    {/* Desktop-only: Type */}
                    {!isMobile && (
                      <span className="text-[13px] text-muted-foreground">{pet.petType}</span>
                    )}

                    {/* Desktop-only: Age */}
                    {!isMobile && (
                      <span className="text-[13px] text-muted-foreground">{age ?? '—'}</span>
                    )}

                    {/* Actions */}
                    <div className={cn('flex justify-end', isMobile ? 'gap-0' : 'gap-0.5')}>
                      <IconBtn title="View" onClick={() => navigate(`/pets/${pet.petId}?from=manage`)}>
                        <Eye className="w-[15px] h-[15px]" />
                      </IconBtn>
                      <IconBtn title="Edit" onClick={() => setEditPet(pet)}>
                        <Pencil className="w-[15px] h-[15px]" />
                      </IconBtn>
                      <IconBtn title="Delete" danger onClick={() => setDeleteTarget({ id: pet.petId, name: pet.name })}>
                        <Trash2 className="w-[15px] h-[15px]" />
                      </IconBtn>
                    </div>
                  </div>
                )
              })}
            </div>
          )}
        </div>
      </div>

      {/* ── Dialogs ── */}

      {addPetOpen && (
        <AddPetDialog onClose={() => setAddPetOpen(false)} onSuccess={handlePetAdded} />
      )}

      {editPet && (
        <EditPetModal
          pet={editPet}
          onClose={() => setEditPet(null)}
          onSuccess={() => {
            queryClient.invalidateQueries({ queryKey: ['owner', 'pets', accountId] })
            queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
            queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
            setEditPet(null)
          }}
        />
      )}

      {deleteTarget && (
        <ConfirmDelete
          name={deleteTarget.name}
          isDeleting={isDeleting}
          onCancel={() => setDeleteTarget(null)}
          onConfirm={() => handleDeleteOne(deleteTarget.id)}
        />
      )}

      {bulkDeleteOpen && (
        <ConfirmDelete
          name={`${selected.size} pets`}
          isDeleting={isDeleting}
          onCancel={() => setBulkDeleteOpen(false)}
          onConfirm={handleBulkDelete}
        />
      )}
    </div>
  )
}

// ── Shared micro-components ───────────────────────────────────────────

const Checkbox = forwardRef<HTMLInputElement, { checked: boolean; onChange: () => void }>(
  ({ checked, onChange }, ref) => (
    <input
      ref={ref}
      type="checkbox"
      checked={checked}
      onChange={onChange}
      className="w-[18px] h-[18px] rounded-[5px] border-[1.5px] border-border cursor-pointer shrink-0 transition-all accent-primary"
      style={{ background: checked ? 'var(--primary)' : 'var(--card)', appearance: 'none', WebkitAppearance: 'none' }}
    />
  )
)
Checkbox.displayName = 'Checkbox'

function IconBtn({
  children, title, onClick, danger,
}: {
  children: React.ReactNode
  title: string
  onClick: () => void
  danger?: boolean
}) {
  return (
    <button
      title={title}
      aria-label={title}
      onClick={onClick}
      className={cn(
        'w-[34px] h-[34px] rounded-lg border-0 bg-transparent cursor-pointer flex items-center justify-center text-muted-foreground shrink-0 transition-colors',
        danger
          ? 'hover:bg-[#e5484d]/10 hover:text-destructive'
          : 'hover:bg-secondary hover:text-foreground'
      )}
    >
      {children}
    </button>
  )
}

function ConfirmDelete({
  name, isDeleting, onCancel, onConfirm,
}: {
  name: string
  isDeleting: boolean
  onCancel: () => void
  onConfirm: () => void
}) {
  return (
    <div className="fixed inset-0 z-[200] bg-black/40 backdrop-blur-sm flex items-center justify-center p-6">
      <div className="bg-card rounded-[20px] p-7 max-w-[360px] w-full shadow-[0_24px_64px_rgba(0,0,0,0.15)]">
        <div className="flex items-center gap-2 mb-4">
          <BarkfestMark size={22} />
          <span className="font-heading text-[17px] font-bold">Barkfest</span>
        </div>
        <h3 className="font-heading text-xl font-bold mb-2.5">Delete {name}?</h3>
        <p className="text-sm text-muted-foreground leading-relaxed mb-[22px]">
          This will permanently remove {name} and all their photos. This can't be undone.
        </p>
        <div className="flex gap-2.5">
          <button
            onClick={onCancel}
            className="flex-1 h-[42px] rounded-[10px] border-[1.5px] border-border bg-transparent text-foreground text-sm font-medium cursor-pointer"
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            disabled={isDeleting}
            className="flex-1 h-[42px] rounded-[10px] border-0 bg-destructive text-white text-sm font-semibold cursor-pointer flex items-center justify-center gap-2 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isDeleting && <Loader2 className="w-3.5 h-3.5 animate-spin" />}
            Delete
          </button>
        </div>
      </div>
    </div>
  )
}
