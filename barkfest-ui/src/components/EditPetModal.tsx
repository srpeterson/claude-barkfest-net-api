import { useEffect, useRef, useState } from 'react'
import { flushSync } from 'react-dom'
import { ChevronRight, Loader2, Star, X } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { BarkfestMark } from '@/components/BarkfestMark'
import { PetTypeBreedFormFields } from '@/components/PetTypeBreedFormFields'
import { getBlobImageUrl } from '@/lib/imageUrl'
import {
  addPetImages,
  getBrowseBreeds,
  removePetImage,
  setFeaturedImage,
  updatePet,
} from '@/lib/api'
import type { PetDto } from '@/lib/api'

const MAX_IMAGES = 6

// ── Helpers ──────────────────────────────────────────────────────────

function petTypeNameToValue(name: string): number {
  if (name === 'Dog') return 1
  if (name === 'Cat') return 2
  return 0
}

// ── Shared input style ────────────────────────────────────────────────
const INP: React.CSSProperties = {
  width: '100%', height: 44,
  border: '1.5px solid var(--border)', borderRadius: 12,
  background: 'var(--background)', color: 'var(--foreground)',
  padding: '0 14px',
  fontFamily: "'DM Sans', sans-serif", fontSize: 14,
  outline: 'none', boxSizing: 'border-box',
  transition: 'border-color 0.15s, box-shadow 0.15s',
}
const LABEL: React.CSSProperties = {
  display: 'block', fontSize: 13, fontWeight: 600,
  marginBottom: 6, color: 'var(--foreground)',
}
function focusIn(e: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement>) {
  e.target.style.borderColor = 'var(--primary)'
  e.target.style.boxShadow = '0 0 0 3px var(--primary-10)'
}
function focusOut(e: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement>) {
  e.target.style.borderColor = 'var(--border)'
  e.target.style.boxShadow = 'none'
}

// ── Props ─────────────────────────────────────────────────────────────

interface EditPetModalProps {
  pet: PetDto
  onClose: () => void
  onSuccess?: () => void
}

// ── EditPetModal ──────────────────────────────────────────────────────

export function EditPetModal({ pet, onClose, onSuccess }: EditPetModalProps) {
  const [step, setStep] = useState<1 | 2>(1)

  // ── Step 1 — pre-filled ───────────────────────────────────────────
  const initialPetTypeValue = petTypeNameToValue(pet.petType)
  const [name, setName]           = useState(pet.name)
  const [petTypeValue, setPetTypeValue] = useState(initialPetTypeValue)
  const [userBreedValue, setUserBreedValue] = useState<number | null>(null)
  const [dateOfBirth, setDateOfBirth]   = useState(pet.dateOfBirth ?? '')
  const [description, setDescription]   = useState(pet.description ?? '')

  // Resolve initial breed value from name once the breed list loads
  const { data: initialBreeds = [] } = useQuery({
    queryKey: ['browse', 'breeds', initialPetTypeValue],
    queryFn: () => getBrowseBreeds(initialPetTypeValue),
    enabled: !!initialPetTypeValue,
    staleTime: Infinity,
  })
  const resolvedBreedValue = initialBreeds.find(b => b.name === pet.breed)?.value ?? 0
  const breedValue = userBreedValue ?? resolvedBreedValue

  const today = new Date().toISOString().split('T')[0]
  const step1Valid = name.trim() !== '' && petTypeValue !== 0 && breedValue !== 0 && description.trim() !== ''

  // ── Step 2 — photo management ─────────────────────────────────────
  const [removedIds, setRemovedIds] = useState<Set<string>>(new Set())
  const existingVisible = pet.images.filter(i => !removedIds.has(i.petImageId))

  // New files added via file input
  type NewImg = { file: File; previewUrl: string }
  const [newImages, setNewImages] = useState<NewImg[]>([])
  const [isDragging, setIsDragging] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const createdUrls  = useRef<string[]>([])

  // Cleanup object URLs on unmount
  useEffect(() => {
    return () => { createdUrls.current.forEach(url => URL.revokeObjectURL(url)) }
  }, [])

  function addFiles(files: FileList | null) {
    if (!files) return
    const valid = Array.from(files).filter(f =>
      ['image/jpeg', 'image/jpg', 'image/png'].includes(f.type)
    )
    const remaining = MAX_IMAGES - existingVisible.length - newImages.length
    const toAdd = valid.slice(0, remaining).map(f => {
      const url = URL.createObjectURL(f)
      createdUrls.current.push(url)
      return { file: f, previewUrl: url }
    })
    setNewImages(prev => [...prev, ...toAdd])
  }

  function removeNew(index: number) {
    const removed = newImages[index]
    // Revoke and deselect featured if needed
    URL.revokeObjectURL(removed.previewUrl)
    createdUrls.current = createdUrls.current.filter(u => u !== removed.previewUrl)
    setNewImages(prev => prev.filter((_, i) => i !== index))
    if (featuredKey === removed.previewUrl) setFeaturedKey(null)
  }

  // Unified featured key: petImageId for existing, previewUrl for new
  const originalFeaturedId = pet.images.find(i => i.isFeaturedImage)?.petImageId ?? null
  const [featuredKey, setFeaturedKey] = useState<string | null>(originalFeaturedId)

  // If the featured existing image was removed, clear or recover
  useEffect(() => {
    if (!featuredKey || featuredKey.startsWith('blob:')) return
    if (removedIds.has(featuredKey)) {
      const nextExisting = existingVisible.find(i => i.petImageId !== featuredKey)
      setFeaturedKey(nextExisting?.petImageId ?? null)
    }
  }, [removedIds]) // eslint-disable-line react-hooks/exhaustive-deps

  const totalImages = existingVisible.length + newImages.length
  const slotsLeft = MAX_IMAGES - totalImages

  // ── Submission ────────────────────────────────────────────────────
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSave() {
    flushSync(() => { setError(null); setIsSubmitting(true) })

    try {
      // 1 — Update pet info
      await updatePet(pet.petId, {
        name: name.trim(),
        petTypeValue,
        breedValue,
        ...(dateOfBirth && { dateOfBirth }),
        ...(description.trim() && { description: description.trim() }),
      })

      // 2 — Delete removed existing images
      if (removedIds.size > 0) {
        await Promise.all([...removedIds].map(id => removePetImage(pet.petId, id)))
      }

      // 3 — Upload new images
      let uploadedIds: (string | null)[] = []
      if (newImages.length > 0) {
        const result = await addPetImages(pet.petId, newImages.map(img => img.file))
        uploadedIds = result.results.map(r => (r.success ? r.imageId : null))
      }

      // 4 — Update featured image if changed
      if (featuredKey !== null) {
        if (featuredKey.startsWith('blob:')) {
          // Featured is a newly uploaded image
          const newIdx = newImages.findIndex(img => img.previewUrl === featuredKey)
          const targetId = newIdx !== -1 ? uploadedIds[newIdx] : null
          if (targetId) await setFeaturedImage(pet.petId, targetId)
        } else if (featuredKey !== originalFeaturedId && !removedIds.has(featuredKey)) {
          await setFeaturedImage(pet.petId, featuredKey)
        }
      }

      onSuccess?.()
      onClose()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  // ── Render ────────────────────────────────────────────────────────
  return (
    <div
      style={{
        position: 'fixed', inset: 0, zIndex: 400,
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        background: 'rgba(0,0,0,0.5)', backdropFilter: 'blur(4px)',
        padding: 16,
      }}
    >
      <div
        style={{
          width: '100%', maxWidth: 480, maxHeight: '90vh', overflowY: 'auto',
          background: 'var(--card)', borderRadius: 24,
          boxShadow: '0 32px 80px rgba(0,0,0,0.22)',
          padding: 32, position: 'relative',
        }}
      >
        {/* Close */}
        <button
          onClick={onClose}
          disabled={isSubmitting}
          style={{
            position: 'absolute', top: 14, right: 14,
            width: 32, height: 32, borderRadius: 8,
            border: 'none', background: 'transparent', cursor: 'pointer',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: 'var(--muted-foreground)',
          }}
        >
          <X className="w-5 h-5" />
        </button>

        {/* Brand */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 18 }}>
          <BarkfestMark size={22} />
          <span className="font-heading font-bold" style={{ fontSize: 17 }}>Barkfest</span>
        </div>

        {/* Title */}
        <div style={{ marginBottom: 16 }}>
          <h2 className="font-heading font-bold" style={{ fontSize: 22, margin: '0 0 4px' }}>
            {step === 1 ? `Edit ${pet.name}` : 'Update photos'}
          </h2>
          <p style={{ fontSize: 13.5, color: 'var(--muted-foreground)', margin: 0 }}>
            {step === 1 ? 'Update the details below.' : `Manage photos for ${name}.`}
          </p>
        </div>

        {/* Progress bar */}
        <div style={{ display: 'flex', gap: 8, marginBottom: 20 }}>
          {[{ label: 'Details', s: 1 }, { label: 'Photos', s: 2 }].map(({ label, s }) => (
            <div key={s} style={{ flex: 1 }}>
              <div style={{ height: 4, borderRadius: 2, background: s <= step ? 'var(--primary)' : 'var(--border)', transition: 'background 0.3s' }} />
              <span style={{ display: 'block', fontSize: 11, fontWeight: s === step ? 600 : 400, color: s <= step ? 'var(--primary)' : 'var(--muted-foreground)', marginTop: 4 }}>
                {label}
              </span>
            </div>
          ))}
        </div>

        {/* ── Step 1: Details ── */}
        {step === 1 && (
          <div>
            <div style={{ marginBottom: 16 }}>
              <label style={LABEL}>Name *</label>
              <input
                value={name} onChange={e => setName(e.target.value)}
                maxLength={75} autoFocus
                style={INP} onFocus={focusIn} onBlur={focusOut}
              />
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12, marginBottom: 16 }}>
              <PetTypeBreedFormFields
                petTypeValue={petTypeValue}
                onPetTypeChange={v => { setPetTypeValue(v); setUserBreedValue(0) }}
                breedValue={breedValue}
                onBreedChange={setUserBreedValue}
              />
            </div>

            <div style={{ marginBottom: 16 }}>
              <label style={LABEL}>Date of birth</label>
              <input
                type="date" max={today} value={dateOfBirth}
                onChange={e => setDateOfBirth(e.target.value)}
                style={INP} onFocus={focusIn} onBlur={focusOut}
              />
            </div>

            <div style={{ marginBottom: 16 }}>
              <label style={LABEL}>Description *</label>
              <textarea
                value={description} onChange={e => setDescription(e.target.value)}
                placeholder="Tell us about this pet…"
                style={{ ...INP, height: 80, padding: '10px 14px', resize: 'none' }}
                onFocus={focusIn} onBlur={focusOut}
              />
            </div>

            <div style={{ display: 'flex', gap: 8 }}>
              <OutlineBtn onClick={onClose}>Cancel</OutlineBtn>
              <PrimaryBtn onClick={() => setStep(2)} disabled={!step1Valid}>
                Next <ChevronRight className="w-4 h-4" />
              </PrimaryBtn>
            </div>
          </div>
        )}

        {/* ── Step 2: Photos ── */}
        {step === 2 && (
          <div>
            <p style={{ fontSize: 13, color: 'var(--muted-foreground)', marginBottom: 12 }}>
              Tap a photo to set it as the cover. Add or remove photos (max {MAX_IMAGES}).
            </p>

            {/* Photo grid */}
            {totalImages > 0 && (
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 8, marginBottom: 12 }}>

                {existingVisible.map((img) => {
                  const isFeatured = featuredKey === img.petImageId
                  return (
                    <div
                      key={img.petImageId}
                      onClick={() => setFeaturedKey(img.petImageId)}
                      style={{
                        aspectRatio: '1', borderRadius: 10, overflow: 'hidden',
                        cursor: 'pointer', position: 'relative',
                        outline: isFeatured ? '2px solid var(--primary)' : '2px solid transparent',
                        outlineOffset: 2, transition: 'outline-color 0.15s',
                      }}
                    >
                      <img src={getBlobImageUrl(img.blobName)} alt="" style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }} />
                      {isFeatured && <FeaturedBadge />}
                      <RemoveBtn onClick={e => { e.stopPropagation(); setRemovedIds(prev => new Set([...prev, img.petImageId])) }} />
                    </div>
                  )
                })}

                {newImages.map((img, idx) => {
                  const isFeatured = featuredKey === img.previewUrl
                  return (
                    <div
                      key={img.previewUrl}
                      onClick={() => setFeaturedKey(img.previewUrl)}
                      style={{
                        aspectRatio: '1', borderRadius: 10, overflow: 'hidden',
                        cursor: 'pointer', position: 'relative',
                        outline: isFeatured ? '2px solid var(--primary)' : '2px solid transparent',
                        outlineOffset: 2,
                      }}
                    >
                      <img src={img.previewUrl} alt={`New photo ${idx + 1}`} style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }} />
                      {isFeatured && <FeaturedBadge />}
                      <RemoveBtn onClick={e => { e.stopPropagation(); removeNew(idx) }} />
                    </div>
                  )
                })}
              </div>
            )}

            {/* Add more drop target */}
            {slotsLeft > 0 && (
              <label
                onDragOver={e => { e.preventDefault(); setIsDragging(true) }}
                onDragLeave={() => setIsDragging(false)}
                onDrop={e => { e.preventDefault(); setIsDragging(false); addFiles(e.dataTransfer.files) }}
                onClick={() => fileInputRef.current?.click()}
                style={{
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  height: 60, borderRadius: 12, marginBottom: 12,
                  cursor: 'pointer',
                  border: `2px dashed ${isDragging ? 'var(--primary)' : 'var(--border)'}`,
                  background: isDragging ? 'var(--primary-10)' : 'var(--secondary)',
                  color: isDragging ? 'var(--primary)' : 'var(--muted-foreground)',
                  fontSize: 13, transition: 'border-color 0.15s, background 0.15s',
                }}
              >
                + Add more · {slotsLeft} remaining
                <input
                  ref={fileInputRef} type="file"
                  accept=".jpg,.jpeg,.png" multiple style={{ display: 'none' }}
                  onChange={e => { addFiles(e.target.files); e.target.value = '' }}
                />
              </label>
            )}

            {totalImages === 0 && (
              <p style={{ fontSize: 13, color: 'var(--destructive)', textAlign: 'center', marginBottom: 10 }}>
                At least 1 photo is required.
              </p>
            )}

            {error && (
              <p style={{ fontSize: 13, color: 'var(--destructive)', marginBottom: 10 }}>
                {error}
              </p>
            )}

            <div style={{ display: 'flex', gap: 8 }}>
              <OutlineBtn onClick={() => { setStep(1); setError(null) }} disabled={isSubmitting}>
                Back
              </OutlineBtn>
              <PrimaryBtn
                onClick={handleSave}
                disabled={totalImages === 0 || isSubmitting}
              >
                {isSubmitting
                  ? <><Loader2 className="w-4 h-4 animate-spin" />Saving…</>
                  : 'Save changes'}
              </PrimaryBtn>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

// ── Shared small components ───────────────────────────────────────────

function FeaturedBadge() {
  return (
    <div style={{ position: 'absolute', top: 5, left: 5, width: 18, height: 18, borderRadius: '50%', background: 'var(--primary)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <Star style={{ width: 10, height: 10, fill: '#fff', stroke: 'none' }} />
    </div>
  )
}

function RemoveBtn({ onClick }: { onClick: (e: React.MouseEvent) => void }) {
  return (
    <button
      type="button" onClick={onClick}
      style={{
        position: 'absolute', top: 5, right: 5,
        width: 20, height: 20, borderRadius: '50%',
        background: 'rgba(0,0,0,0.6)', color: '#fff',
        border: 'none', cursor: 'pointer',
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        fontSize: 14, fontWeight: 700, lineHeight: 1,
      }}
    >
      ×
    </button>
  )
}

function OutlineBtn({
  children, onClick, disabled,
}: { children: React.ReactNode; onClick: () => void; disabled?: boolean }) {
  return (
    <button
      type="button" onClick={onClick} disabled={disabled}
      style={{
        flex: 1, height: 44, borderRadius: 12,
        border: '1.5px solid var(--border)', background: 'transparent',
        color: 'var(--muted-foreground)',
        fontFamily: "'DM Sans', sans-serif", fontSize: 14, fontWeight: 500,
        cursor: disabled ? 'not-allowed' : 'pointer',
        opacity: disabled ? 0.5 : 1,
      }}
    >
      {children}
    </button>
  )
}

function PrimaryBtn({
  children, onClick, disabled,
}: { children: React.ReactNode; onClick: () => void; disabled?: boolean }) {
  return (
    <button
      type="button" onClick={onClick} disabled={disabled}
      style={{
        flex: 1, height: 44, borderRadius: 12,
        border: 'none', background: 'var(--primary)', color: '#fff',
        fontFamily: "'DM Sans', sans-serif", fontSize: 14, fontWeight: 600,
        display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6,
        cursor: disabled ? 'not-allowed' : 'pointer',
        opacity: disabled ? 0.5 : 1,
        transition: 'opacity 0.15s',
      }}
    >
      {children}
    </button>
  )
}
