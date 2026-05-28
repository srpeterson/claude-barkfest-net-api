import { useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Eye, Loader2, PawPrint, Pencil, Plus, Trash2 } from 'lucide-react'
import { deletePet, getOwnerPets } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { useAuth } from '@/hooks/useAuth'
import { BarkfestMark } from '@/components/BarkfestMark'
import { Navbar } from '@/components/Navbar'
import { AddPetDialog } from '@/components/AddPetDialog'

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

export function ManagePetsPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { accountId } = useAuth()

  const [selected, setSelected] = useState<Set<string>>(new Set())
  const [deleteTarget, setDeleteTarget] = useState<string | null>(null)
  const [bulkDeleteOpen, setBulkDeleteOpen] = useState(false)
  const [isDeleting, setIsDeleting] = useState(false)
  const [addPetOpen, setAddPetOpen] = useState(false)
  const selectAllRef = useRef<HTMLInputElement>(null)

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

  const targetPet = pets.find(p => p.petId === deleteTarget)

  return (
    <div className="min-h-screen" style={{ background: 'var(--background)' }}>
      <Navbar />

      {/* Bulk delete bar */}
      {selected.size > 0 && (
        <div className="sticky top-[72px] z-40 px-4 pt-2">
          <div
            className="flex items-center justify-between h-[52px] px-5 rounded-xl"
            style={{ background: 'var(--primary)' }}
          >
            <span className="text-sm font-semibold text-white">
              {selected.size} selected
            </span>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setSelected(new Set())}
                className="px-4 h-8 rounded-lg text-sm font-medium text-white transition-colors hover:bg-white/20"
                style={{ border: '1.5px solid rgba(255,255,255,0.5)', background: 'rgba(255,255,255,0.18)' }}
              >
                Clear
              </button>
              <button
                onClick={() => setBulkDeleteOpen(true)}
                className="px-4 h-8 rounded-lg text-sm font-semibold transition-opacity hover:opacity-90"
                style={{ background: 'white', color: 'var(--destructive)' }}
              >
                Delete {selected.size}
              </button>
            </div>
          </div>
        </div>
      )}

      <main className="max-w-[900px] mx-auto px-6 py-8 space-y-6">
        {/* Page header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="font-heading text-[28px] font-bold">My Pets</h1>
            <p className="text-sm mt-0.5" style={{ color: 'var(--muted-foreground)' }}>
              {pets.length} {pets.length === 1 ? 'pet' : 'pets'} in your profile
            </p>
          </div>
          <button
            onClick={() => setAddPetOpen(true)}
            className="flex items-center gap-1.5 px-4 text-sm font-semibold text-white transition-opacity hover:opacity-90"
            style={{
              height: '42px',
              borderRadius: '10px',
              background: 'var(--primary)',
            }}
          >
            <Plus className="w-4 h-4" />
            Add Pet
          </button>
        </div>

        {/* Table */}
        {isLoading ? (
          <div className="flex justify-center py-16">
            <Loader2 className="w-8 h-8 animate-spin text-primary" />
          </div>
        ) : pets.length === 0 ? (
          <div
            className="flex flex-col items-center py-20 rounded-2xl"
            style={{ background: 'var(--card)', border: '1px solid var(--border)' }}
          >
            <PawPrint className="w-12 h-12 mb-4" style={{ color: 'var(--primary)', opacity: 0.4 }} />
            <p className="font-heading font-semibold text-lg mb-1">No pets yet</p>
            <p className="text-sm mb-6" style={{ color: 'var(--muted-foreground)' }}>
              Share your first pet with the Barkfest community!
            </p>
            <button
              onClick={() => setAddPetOpen(true)}
              className="flex items-center gap-1.5 px-5 h-10 rounded-xl text-sm font-semibold text-white"
              style={{ background: 'var(--primary)' }}
            >
              <Plus className="w-4 h-4" />
              Add your first pet
            </button>
          </div>
        ) : (
          <div
            className="overflow-hidden rounded-2xl"
            style={{ background: 'var(--card)', border: '1px solid var(--border)' }}
          >
            {/* Header row */}
            <div
              className="hidden sm:grid items-center h-11 px-4 text-[11px] font-bold uppercase tracking-wider"
              style={{
                background: 'rgba(223,103,73,0.08)',
                color: 'var(--primary)',
                gridTemplateColumns: '48px 56px 1fr 80px 80px 100px',
              }}
            >
              <div className="flex items-center justify-center">
                <input
                  ref={selectAllRef}
                  type="checkbox"
                  checked={allSelected}
                  onChange={toggleAll}
                  className="w-[18px] h-[18px] rounded-[5px] cursor-pointer accent-primary"
                />
              </div>
              <div />
              <div>Name</div>
              <div>Type</div>
              <div>Age</div>
              <div className="text-right">Actions</div>
            </div>

            {/* Mobile select-all row */}
            <div
              className="sm:hidden flex items-center h-11 px-4 gap-3 text-sm font-medium"
              style={{ background: 'rgba(223,103,73,0.08)', color: 'var(--primary)' }}
            >
              <input
                type="checkbox"
                checked={allSelected}
                onChange={toggleAll}
                className="w-[18px] h-[18px] rounded-[5px] cursor-pointer accent-primary"
              />
              <span>Select all</span>
            </div>

            {/* Pet rows */}
            {pets.map((pet, i) => {
              const age = formatAge(pet.dateOfBirth)
              const isSelected = selected.has(pet.petId)
              return (
                <div
                  key={pet.petId}
                  className="grid items-center px-4 transition-colors"
                  style={{
                    height: '60px',
                    gridTemplateColumns: '48px 56px 1fr 80px 80px 100px',
                    background: isSelected ? 'rgba(223,103,73,0.10)' : 'transparent',
                    borderTop: i === 0 ? 'none' : '1px solid var(--border)',
                  }}
                >
                  {/* Checkbox */}
                  <div className="flex items-center justify-center">
                    <input
                      type="checkbox"
                      checked={isSelected}
                      onChange={() => toggleOne(pet.petId)}
                      className="w-[18px] h-[18px] rounded-[5px] cursor-pointer accent-primary"
                    />
                  </div>

                  {/* Thumbnail */}
                  <div>
                    {(() => {
                      const featured = pet.images.find(i => i.isFeaturedImage) ?? pet.images[0]
                      return featured ? (
                        <img
                          src={getBlobImageUrl(featured.blobName)}
                          alt={pet.name}
                          className="w-10 h-10 rounded-lg object-cover"
                        />
                      ) : (
                        <div
                          className="w-10 h-10 rounded-lg flex items-center justify-center"
                          style={{ background: 'var(--secondary)' }}
                        >
                          <PawPrint className="w-5 h-5" style={{ color: 'var(--primary)' }} />
                        </div>
                      )
                    })()}
                  </div>

                  {/* Name + breed (desktop) / stacked (mobile) */}
                  <div className="min-w-0 pr-2">
                    <p className="font-medium text-sm truncate">{pet.name}</p>
                    <p className="text-xs truncate" style={{ color: 'var(--muted-foreground)' }}>
                      {pet.breed}
                    </p>
                  </div>

                  {/* Type (hidden on mobile via grid col) */}
                  <div className="hidden sm:block text-sm" style={{ color: 'var(--muted-foreground)' }}>
                    {pet.petType}
                  </div>

                  {/* Age */}
                  <div className="hidden sm:block text-sm" style={{ color: 'var(--muted-foreground)' }}>
                    {age ?? '—'}
                  </div>

                  {/* Actions */}
                  <div className="flex items-center justify-end gap-1">
                    <button
                      onClick={() => navigate(`/pets/${pet.petId}?from=manage`)}
                      className="w-8 h-8 rounded-lg flex items-center justify-center hover:bg-secondary transition-colors"
                      title="View"
                    >
                      <Eye className="w-4 h-4 text-muted-foreground" />
                    </button>
                    <button
                      onClick={() => {/* TODO: EditPetModal */}}
                      className="w-8 h-8 rounded-lg flex items-center justify-center hover:bg-secondary transition-colors"
                      title="Edit"
                    >
                      <Pencil className="w-4 h-4 text-muted-foreground" />
                    </button>
                    <button
                      onClick={() => setDeleteTarget(pet.petId)}
                      className="w-8 h-8 rounded-lg flex items-center justify-center hover:bg-secondary transition-colors"
                      title="Delete"
                    >
                      <Trash2 className="w-4 h-4 text-destructive" />
                    </button>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </main>

      {/* Single delete modal */}
      {deleteTarget && targetPet && (
        <DeleteModal
          petName={targetPet.name}
          isDeleting={isDeleting}
          onCancel={() => setDeleteTarget(null)}
          onConfirm={() => handleDeleteOne(deleteTarget)}
        />
      )}

      {/* Bulk delete modal */}
      {bulkDeleteOpen && (
        <DeleteModal
          petName={`${selected.size} pets`}
          isDeleting={isDeleting}
          onCancel={() => setBulkDeleteOpen(false)}
          onConfirm={handleBulkDelete}
        />
      )}

      {addPetOpen && <AddPetDialog onClose={() => setAddPetOpen(false)} onSuccess={handlePetAdded} />}
    </div>
  )
}

function DeleteModal({
  petName,
  isDeleting,
  onCancel,
  onConfirm,
}: {
  petName: string
  isDeleting: boolean
  onCancel: () => void
  onConfirm: () => void
}) {
  return (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center animate-backdrop-in px-4"
      style={{ background: 'rgba(0,0,0,0.5)', backdropFilter: 'blur(4px)' }}
    >
      <div
        className="w-full max-w-[360px] animate-dialog-appear"
        style={{
          background: 'var(--card)',
          borderRadius: '20px',
          padding: '28px',
          boxShadow: '0 32px 80px rgba(0,0,0,0.22)',
        }}
      >
        <div className="flex items-center gap-2 mb-5">
          <BarkfestMark size={22} />
          <span className="font-heading font-bold" style={{ fontSize: '17px' }}>Barkfest</span>
        </div>
        <h3 className="font-heading font-bold text-lg mb-2">Delete {petName}?</h3>
        <p className="text-sm mb-6" style={{ color: 'var(--muted-foreground)' }}>
          This will permanently remove {petName} and all associated photos. This cannot be undone.
        </p>
        <div className="flex gap-3">
          <button
            onClick={onCancel}
            className="flex-1 h-11 rounded-xl text-sm font-medium transition-colors hover:bg-secondary"
            style={{ border: '1.5px solid var(--border)', background: 'transparent', color: 'var(--muted-foreground)' }}
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            disabled={isDeleting}
            className="flex-1 h-11 rounded-xl text-sm font-semibold text-white flex items-center justify-center gap-2 transition-opacity hover:opacity-90 disabled:opacity-50"
            style={{ background: 'var(--destructive)' }}
          >
            {isDeleting && <Loader2 className="w-4 h-4 animate-spin" />}
            Delete
          </button>
        </div>
      </div>
    </div>
  )
}
