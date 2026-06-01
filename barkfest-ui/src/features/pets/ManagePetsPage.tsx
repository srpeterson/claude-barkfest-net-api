import { useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Eye, Loader2, PawPrint, Pencil, Plus, Trash2 } from 'lucide-react'
import { deletePet, getOwnerById, getOwnerPets, setOwnerVisibility } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { useAuth } from '@/hooks/useAuth'
import { useIsMobile } from '@/hooks/useIsMobile'
import { BarkfestMark } from '@/components/BarkfestMark'
import { Navbar } from '@/components/Navbar'
import { AddPetDialog } from '@/components/AddPetDialog'
import { EditPetModal } from '@/components/EditPetModal'
import type { PetDto } from '@/lib/api'

// ── Age formatter ─────────────────────────────────────────────────────
function formatAge(dateOfBirth?: string): string | null {
  if (!dateOfBirth) return null
  const dob = new Date(dateOfBirth)
  const today = new Date()
  let months = (today.getFullYear() - dob.getFullYear()) * 12 + (today.getMonth() - dob.getMonth())
  if (today.getDate() < dob.getDate()) months--
  months = Math.max(months, 0)
  if (months < 12) return months === 1 ? '1 mo' : `${months} mo`
  const years = Math.floor(months / 12)
  return years === 1 ? '1 yr' : `${years} yr`
}

// ── Switch ────────────────────────────────────────────────────────────
function Switch({ checked, onChange, id }: { checked: boolean; onChange: (v: boolean) => void; id: string }) {
  return (
    <button
      role="switch"
      aria-checked={checked}
      id={id}
      onClick={() => onChange(!checked)}
      style={{
        width: 46, height: 27, borderRadius: 999, border: 'none', cursor: 'pointer',
        padding: 0, flexShrink: 0,
        background: checked ? 'var(--primary)' : '#d8cfc6',
        position: 'relative', transition: 'background 0.2s',
      }}
    >
      <span
        style={{
          position: 'absolute', top: 3, left: checked ? 22 : 3,
          width: 21, height: 21, borderRadius: '50%',
          background: '#fff', transition: 'left 0.2s',
          boxShadow: '0 1px 3px rgba(0,0,0,0.25)',
        }}
      />
    </button>
  )
}

// ── HidePetsToggle ────────────────────────────────────────────────────
function HidePetsToggle({ hidden, onChange, error }: { hidden: boolean; onChange: (v: boolean) => void; error?: string | null }) {
  return (
    <div>
      <div
        style={{
          display: 'flex', alignItems: 'center', gap: 14,
          background: 'var(--card)',
          border: `1px solid ${hidden ? 'var(--primary)' : 'var(--border)'}`,
          borderRadius: 12, padding: '10px 14px 10px 16px',
          transition: 'border-color 0.2s',
        }}
      >
        <div>
          <label
            htmlFor="hide-pets"
            style={{ display: 'block', fontSize: 14, fontWeight: 600, cursor: 'pointer' }}
          >
            Hide my pets
          </label>
          <p
            style={{
              margin: 0, fontSize: 12,
              color: hidden ? 'var(--primary)' : 'var(--muted-foreground)',
              fontWeight: hidden ? 600 : 400,
            }}
          >
            {hidden ? 'Hidden from the public gallery' : 'Visible in the public gallery'}
          </p>
        </div>
        <Switch id="hide-pets" checked={hidden} onChange={onChange} />
      </div>
      {error && (
        <p style={{ fontSize: 12, color: 'var(--destructive)', margin: '4px 0 0' }}>{error}</p>
      )}
    </div>
  )
}

// ── ManagePetsPage ────────────────────────────────────────────────────
export function ManagePetsPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const isMobile = useIsMobile()
  const { accountId } = useAuth()

  const [selected, setSelected] = useState<Set<string>>(new Set())
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string } | null>(null)
  const [bulkDeleteOpen, setBulkDeleteOpen] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)
  const [addPetOpen, setAddPetOpen] = useState(false)
  const [editPet, setEditPet] = useState<PetDto | null>(null)
  const [optimisticHidden, setOptimisticHidden] = useState<boolean | null>(null)
  const [visibilityError, setVisibilityError] = useState<string | null>(null)
  const selectAllRef = useRef<HTMLInputElement>(null)

  // Fetch owner to get initial visibility
  const { data: ownerData } = useQuery({
    queryKey: ['owner', accountId],
    queryFn: () => getOwnerById(accountId!),
    enabled: !!accountId,
  })

  const hidden = optimisticHidden ?? (ownerData ? !ownerData.isVisible : false)

  const { data: pets = [], isLoading } = useQuery({
    queryKey: ['owner', 'pets', accountId],
    queryFn: () => getOwnerPets(accountId!),
    enabled: !!accountId,
  })

  // Keep indeterminate state on select-all checkbox in sync
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

  async function handleToggleHidden(newHidden: boolean) {
    const previous = hidden
    setOptimisticHidden(newHidden)
    setVisibilityError(null)
    try {
      await setOwnerVisibility(accountId!, newHidden)
    } catch {
      setOptimisticHidden(previous)
      setVisibilityError('Failed to update visibility. Please try again.')
    }
  }

  // Desktop grid columns
  const desktopCols = '48px 56px 1fr 80px 80px 100px'
  const mobileCols  = '40px 52px 1fr 96px'
  const cols = isMobile ? mobileCols : desktopCols

  return (
    <div style={{ minHeight: '100vh', background: 'var(--background)' }}>
      <Navbar />

      <div style={{ maxWidth: 900, margin: '0 auto', padding: '32px 24px 64px' }}>

        {/* Page header */}
        <div
          style={{
            display: 'flex',
            alignItems: 'flex-end',
            justifyContent: 'space-between',
            marginBottom: 24,
            flexWrap: 'wrap',
            gap: 16,
          }}
        >
          <div>
            <h1 className="font-heading" style={{ fontSize: 28, fontWeight: 700, marginBottom: 4 }}>
              My Pets
            </h1>
            <p style={{ fontSize: 14, color: 'var(--muted-foreground)', margin: 0 }}>
              {pets.length} {pets.length === 1 ? 'pet' : 'pets'} in your profile
            </p>
          </div>
          <HidePetsToggle hidden={hidden} onChange={handleToggleHidden} error={visibilityError} />
        </div>

        {/* Bulk delete bar — sticky inside the content column */}
        {selected.size > 0 && (
          <div
            style={{
              position: 'sticky', top: 68, zIndex: 40,
              margin: '0 0 16px',
              background: 'var(--primary)', color: '#fff',
              borderRadius: 12, padding: '0 16px', height: 52,
              display: 'flex', alignItems: 'center', justifyContent: 'space-between',
            }}
          >
            <span style={{ fontSize: 14, fontWeight: 500 }}>
              {selected.size} pet{selected.size > 1 ? 's' : ''} selected
            </span>
            <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
              <button
                onClick={() => setSelected(new Set())}
                style={{
                  height: 32, padding: '0 14px', borderRadius: 8,
                  border: '1.5px solid #fff',
                  background: 'rgba(255,255,255,0.18)',
                  color: '#fff',
                  fontFamily: "'DM Sans', sans-serif", fontSize: 13, fontWeight: 600, cursor: 'pointer',
                }}
              >
                Clear
              </button>
              <button
                onClick={() => setBulkDeleteOpen(true)}
                style={{
                  height: 32, padding: '0 14px', borderRadius: 8,
                  border: 'none', background: '#fff',
                  color: 'var(--destructive)',
                  fontFamily: "'DM Sans', sans-serif", fontSize: 13, fontWeight: 700, cursor: 'pointer',
                  display: 'flex', alignItems: 'center', gap: 6,
                }}
              >
                <Trash2 style={{ width: 14, height: 14 }} />
                Delete {selected.size}
              </button>
            </div>
          </div>
        )}

        {/* Table — dims when hidden */}
        <div style={{ opacity: hidden ? 0.5 : 1, transition: 'opacity 0.2s', pointerEvents: hidden ? 'none' : 'auto' }}>
          {isLoading ? (
            <div style={{ display: 'flex', justifyContent: 'center', padding: '64px 0' }}>
              <Loader2 style={{ width: 32, height: 32, color: 'var(--primary)', animation: 'spin 1s linear infinite' }} />
            </div>
          ) : pets.length === 0 ? (
            <div
              style={{
                display: 'flex', flexDirection: 'column', alignItems: 'center',
                padding: '80px 0',
                background: 'var(--card)', border: '1px solid var(--border)',
                borderRadius: 16,
              }}
            >
              <PawPrint style={{ width: 48, height: 48, marginBottom: 16, color: 'var(--primary)', opacity: 0.4 }} />
              <p className="font-heading" style={{ fontSize: 18, fontWeight: 600, marginBottom: 6 }}>No pets yet</p>
              <p style={{ fontSize: 14, color: 'var(--muted-foreground)', marginBottom: 24 }}>
                Share your first pet with the Barkfest community!
              </p>
              <button
                onClick={() => setAddPetOpen(true)}
                style={{
                  display: 'flex', alignItems: 'center', gap: 6,
                  height: 40, padding: '0 20px', borderRadius: 10,
                  border: 'none', background: 'var(--primary)', color: '#fff',
                  fontFamily: "'DM Sans', sans-serif", fontSize: 14, fontWeight: 600, cursor: 'pointer',
                }}
              >
                <Plus style={{ width: 16, height: 16 }} />
                Add your first pet
              </button>
            </div>
          ) : (
            <div
              style={{
                background: 'var(--card)',
                border: '1px solid var(--border)',
                borderRadius: 16,
                overflow: 'hidden',
              }}
            >
              {/* Desktop header row */}
              {!isMobile && (
                <div
                  style={{
                    display: 'grid', gridTemplateColumns: cols,
                    alignItems: 'center', padding: '0 16px', height: 44,
                    borderBottom: '1px solid var(--border)',
                    background: 'var(--primary-10)',
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'center' }}>
                    <Checkbox
                      ref={selectAllRef}
                      checked={allSelected}
                      onChange={toggleAll}
                    />
                  </div>
                  <div />
                  {['Name', 'Type', 'Age', 'Actions'].map((h, i) => (
                    <span
                      key={h}
                      style={{
                        fontSize: 11, fontWeight: 700, letterSpacing: '0.06em',
                        textTransform: 'uppercase', color: 'var(--primary)',
                        textAlign: i === 3 ? 'right' : 'left',
                      }}
                    >
                      {h}
                    </span>
                  ))}
                </div>
              )}

              {/* Mobile select-all row */}
              {isMobile && (
                <div
                  style={{
                    display: 'flex', alignItems: 'center', gap: 10,
                    padding: '10px 14px',
                    borderBottom: '1px solid var(--border)',
                    background: 'var(--primary-10)',
                  }}
                >
                  <Checkbox ref={selectAllRef} checked={allSelected} onChange={toggleAll} />
                  <span
                    style={{
                      fontSize: 12, fontWeight: 700, letterSpacing: '0.06em',
                      textTransform: 'uppercase', color: 'var(--primary)',
                    }}
                  >
                    {allSelected ? 'Deselect all' : 'Select all'}
                  </span>
                </div>
              )}

              {/* Pet rows */}
              {pets.map((pet, i) => {
                const age = formatAge(pet.dateOfBirth)
                const isSel = selected.has(pet.petId)
                const featured = pet.images.find(img => img.isFeaturedImage) ?? pet.images[0]
                return (
                  <div
                    key={pet.petId}
                    style={{
                      display: 'grid', gridTemplateColumns: cols,
                      alignItems: 'center',
                      padding: `0 ${isMobile ? 12 : 16}px`,
                      height: isMobile ? 64 : 60,
                      borderTop: i === 0 ? 'none' : '1px solid var(--border)',
                      background: isSel ? 'var(--primary-10)' : 'transparent',
                      transition: 'background 0.12s',
                    }}
                  >
                    {/* Checkbox */}
                    <div style={{ display: 'flex', justifyContent: 'center' }}>
                      <Checkbox checked={isSel} onChange={() => toggleOne(pet.petId)} />
                    </div>

                    {/* Thumbnail */}
                    <div
                      style={{
                        width: isMobile ? 40 : 38, height: isMobile ? 40 : 38,
                        borderRadius: 8, overflow: 'hidden',
                        border: '1.5px solid var(--border)',
                      }}
                    >
                      {featured ? (
                        <img
                          src={getBlobImageUrl(featured.blobName)}
                          alt={pet.name}
                          style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }}
                        />
                      ) : (
                        <div style={{ width: '100%', height: '100%', background: 'var(--secondary)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                          <PawPrint style={{ width: 18, height: 18, color: 'var(--primary)' }} />
                        </div>
                      )}
                    </div>

                    {/* Name + breed (mobile shows type/age too) */}
                    <div style={{ paddingLeft: 4, minWidth: 0 }}>
                      <p style={{ margin: 0, fontSize: isMobile ? 13 : 14, fontWeight: 600, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                        {pet.name}
                      </p>
                      <p style={{ margin: 0, fontSize: 11, color: 'var(--muted-foreground)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                        {isMobile
                          ? [pet.breed, pet.petType, age].filter(Boolean).join(' · ')
                          : pet.breed}
                      </p>
                    </div>

                    {/* Desktop-only: Type */}
                    {!isMobile && (
                      <span style={{ fontSize: 13, color: 'var(--muted-foreground)' }}>{pet.petType}</span>
                    )}

                    {/* Desktop-only: Age */}
                    {!isMobile && (
                      <span style={{ fontSize: 13, color: 'var(--muted-foreground)' }}>{age ?? '—'}</span>
                    )}

                    {/* Actions */}
                    <div style={{ display: 'flex', justifyContent: 'flex-end', gap: isMobile ? 0 : 2 }}>
                      <IconBtn title="View" onClick={() => navigate(`/pets/${pet.petId}?from=manage`)}>
                        <Eye style={{ width: 15, height: 15 }} />
                      </IconBtn>
                      <IconBtn title="Edit" onClick={() => setEditPet(pet)}>
                        <Pencil style={{ width: 15, height: 15 }} />
                      </IconBtn>
                      <IconBtn
                        title="Delete"
                        danger
                        onClick={() => setDeleteTarget({ id: pet.petId, name: pet.name })}
                      >
                        <Trash2 style={{ width: 15, height: 15 }} />
                      </IconBtn>
                    </div>
                  </div>
                )
              })}
            </div>
          )}
        </div>
      </div>

      {/* ── Dialogs ────────────────────────────────────────────────── */}

      {addPetOpen && (
        <AddPetDialog
          onClose={() => setAddPetOpen(false)}
          onSuccess={handlePetAdded}
        />
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

      {/* Single delete confirm */}
      {deleteTarget && (
        <ConfirmDelete
          name={deleteTarget.name}
          isDeleting={isDeleting}
          onCancel={() => setDeleteTarget(null)}
          onConfirm={() => handleDeleteOne(deleteTarget.id)}
        />
      )}

      {/* Bulk delete confirm */}
      {bulkDeleteOpen && (
        <ConfirmDelete
          name={`${selected.size} pets`}
          isDeleting={isDeleting}
          onCancel={() => setBulkDeleteOpen(false)}
          onConfirm={handleBulkDelete}
        />
      )}

      <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
    </div>
  )
}

// ── Shared micro-components ───────────────────────────────────────────

import { forwardRef } from 'react'

const Checkbox = forwardRef<
  HTMLInputElement,
  { checked: boolean; onChange: () => void }
>(({ checked, onChange }, ref) => (
  <input
    ref={ref}
    type="checkbox"
    checked={checked}
    onChange={onChange}
    style={{
      width: 18, height: 18, borderRadius: 5,
      border: '1.5px solid var(--border)',
      background: checked ? 'var(--primary)' : 'var(--card)',
      cursor: 'pointer',
      appearance: 'none', WebkitAppearance: 'none',
      flexShrink: 0, transition: 'all 0.12s',
      accentColor: 'var(--primary)',
    }}
  />
))
Checkbox.displayName = 'Checkbox'

function IconBtn({
  children,
  title,
  onClick,
  danger,
}: {
  children: React.ReactNode
  title: string
  onClick: () => void
  danger?: boolean
}) {
  return (
    <button
      title={title}
      onClick={onClick}
      style={{
        width: 34, height: 34, borderRadius: 8,
        border: 'none', background: 'transparent',
        cursor: 'pointer',
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        color: 'var(--muted-foreground)',
        transition: 'background 0.12s, color 0.12s',
        flexShrink: 0,
      }}
      onMouseEnter={e => {
        e.currentTarget.style.background = danger ? 'rgba(229,72,77,0.1)' : 'var(--secondary)'
        e.currentTarget.style.color = danger ? 'var(--destructive)' : 'var(--foreground)'
      }}
      onMouseLeave={e => {
        e.currentTarget.style.background = 'transparent'
        e.currentTarget.style.color = 'var(--muted-foreground)'
      }}
    >
      {children}
    </button>
  )
}

function ConfirmDelete({
  name,
  isDeleting,
  onCancel,
  onConfirm,
}: {
  name: string
  isDeleting: boolean
  onCancel: () => void
  onConfirm: () => void
}) {
  return (
    <div
      style={{
        position: 'fixed', inset: 0, zIndex: 200,
        background: 'rgba(0,0,0,0.4)', backdropFilter: 'blur(3px)',
        display: 'flex', alignItems: 'center', justifyContent: 'center', padding: 24,
      }}
    >
      <div
        style={{
          background: 'var(--card)', borderRadius: 20, padding: 28,
          maxWidth: 360, width: '100%',
          boxShadow: '0 24px 64px rgba(0,0,0,0.15)',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 16 }}>
          <BarkfestMark size={22} />
          <span className="font-heading" style={{ fontSize: 17, fontWeight: 700 }}>Barkfest</span>
        </div>
        <h3 className="font-heading" style={{ fontSize: 20, fontWeight: 700, marginBottom: 10 }}>
          Delete {name}?
        </h3>
        <p style={{ fontSize: 14, color: 'var(--muted-foreground)', lineHeight: 1.6, marginBottom: 22 }}>
          This will permanently remove {name} and all their photos. This can't be undone.
        </p>
        <div style={{ display: 'flex', gap: 10 }}>
          <button
            onClick={onCancel}
            style={{
              flex: 1, height: 42, borderRadius: 10,
              border: '1.5px solid var(--border)', background: 'transparent',
              fontFamily: "'DM Sans', sans-serif", fontSize: 14, fontWeight: 500,
              cursor: 'pointer', color: 'var(--foreground)',
            }}
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            disabled={isDeleting}
            style={{
              flex: 1, height: 42, borderRadius: 10,
              border: 'none', background: 'var(--destructive)', color: '#fff',
              fontFamily: "'DM Sans', sans-serif", fontSize: 14, fontWeight: 600,
              cursor: isDeleting ? 'not-allowed' : 'pointer',
              display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 8,
              opacity: isDeleting ? 0.6 : 1,
            }}
          >
            {isDeleting && <Loader2 style={{ width: 14, height: 14, animation: 'spin 1s linear infinite' }} />}
            Delete
          </button>
        </div>
      </div>
    </div>
  )
}
