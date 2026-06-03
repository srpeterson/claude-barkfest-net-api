import { useCallback, useEffect, useRef, useState } from 'react'
import { flushSync } from 'react-dom'
import { ChevronRight, Loader2, Star, X } from 'lucide-react'
import { useDropzone } from 'react-dropzone'
import { useQuery } from '@tanstack/react-query'
import { cn } from '@/lib/utils'
import { BarkfestMark } from '@/components/BarkfestMark'
import { DropZone } from '@/components/ui/DropZone'
import { PetTypeBreedFormFields } from '@/components/PetTypeBreedFormFields'
import { getBlobImageUrl } from '@/lib/imageUrl'
import {
  addPetImages,
  getBrowseBreeds,
  getBrowsePetTypes,
  removePetImage,
  setFeaturedImage,
  updatePet,
} from '@/lib/api'
import type { PetDto } from '@/lib/api'

const MAX_IMAGES = 6

const inputCls = [
  'w-full h-11 rounded-xl border-[1.5px] border-border',
  'bg-background text-foreground px-3.5 text-sm',
  'outline-none box-border transition',
  'focus:border-primary focus:ring-2 focus:ring-primary/30',
].join(' ')

interface EditPetModalProps {
  pet: PetDto
  onClose: () => void
  onSuccess?: () => void
}

export function EditPetModal({ pet, onClose, onSuccess }: EditPetModalProps) {
  const [step, setStep] = useState<1 | 2>(1)

  // ── Step 1 — pre-filled ───────────────────────────────────────────
  const [name, setName]                     = useState(pet.name)
  const [petTypeValue, setPetTypeValue]     = useState(0)
  const [userBreedValue, setUserBreedValue] = useState<number | null>(null)
  const [dateOfBirth, setDateOfBirth]       = useState(pet.dateOfBirth ?? '')
  const [description, setDescription]       = useState(pet.description ?? '')

  // Resolve initial pet type value from the browse API — avoids hardcoding SmartEnum integers
  const { data: petTypes = [] } = useQuery({
    queryKey: ['browse', 'pet-types'],
    queryFn: getBrowsePetTypes,
    staleTime: Infinity,
  })
  const resolvedPetTypeValue = petTypes.find(pt => pt.name === pet.petType)?.value ?? 0

  // Set petTypeValue once the browse API resolves the name → value mapping
  const petTypeInitialised = useRef(false)
  useEffect(() => {
    if (!petTypeInitialised.current && resolvedPetTypeValue !== 0) {
      setPetTypeValue(resolvedPetTypeValue)
      petTypeInitialised.current = true
    }
  }, [resolvedPetTypeValue])

  // Resolve initial breed value from name once the breed list loads
  const { data: initialBreeds = [] } = useQuery({
    queryKey: ['browse', 'breeds', resolvedPetTypeValue],
    queryFn: () => getBrowseBreeds(resolvedPetTypeValue),
    enabled: !!resolvedPetTypeValue,
    staleTime: Infinity,
  })
  const resolvedBreedValue = initialBreeds.find(b => b.name === pet.breed)?.value ?? 0
  const breedValue = userBreedValue ?? resolvedBreedValue

  const today     = new Date().toISOString().split('T')[0]
  const step1Valid = name.trim() !== '' && petTypeValue !== 0 && breedValue !== 0 && description.trim() !== ''

  // ── Step 2 — photo management ─────────────────────────────────────
  const [removedIds, setRemovedIds] = useState<Set<string>>(new Set())
  const existingVisible = pet.images.filter(i => !removedIds.has(i.petImageId))

  type NewImg = { file: File; previewUrl: string }
  const [newImages, setNewImages] = useState<NewImg[]>([])
  const createdUrls = useRef<string[]>([])

  useEffect(() => {
    return () => { createdUrls.current.forEach(url => URL.revokeObjectURL(url)) }
  }, [])

  const onDrop = useCallback((accepted: File[]) => {
    const remaining = MAX_IMAGES - existingVisible.length - newImages.length
    const toAdd = accepted.slice(0, remaining).map(f => {
      const url = URL.createObjectURL(f)
      createdUrls.current.push(url)
      return { file: f, previewUrl: url }
    })
    setNewImages(prev => [...prev, ...toAdd])
  }, [existingVisible.length, newImages.length])

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'image/jpeg': ['.jpg', '.jpeg'], 'image/png': ['.png'] },
    multiple: true,
    disabled: existingVisible.length + newImages.length >= MAX_IMAGES,
  })

  function removeNew(index: number) {
    const removed = newImages[index]
    URL.revokeObjectURL(removed.previewUrl)
    createdUrls.current = createdUrls.current.filter(u => u !== removed.previewUrl)
    setNewImages(prev => prev.filter((_, i) => i !== index))
    if (featuredKey === removed.previewUrl) setFeaturedKey(null)
  }

  const originalFeaturedId = pet.images.find(i => i.isFeaturedImage)?.petImageId ?? null
  const [featuredKey, setFeaturedKey] = useState<string | null>(originalFeaturedId)

  // If the featured existing image was removed, clear or recover
  useEffect(() => {
    if (!featuredKey || featuredKey.startsWith('blob:')) return
    if (removedIds.has(featuredKey)) {
      const visible = pet.images.filter(i => !removedIds.has(i.petImageId))
      const nextExisting = visible.find(i => i.petImageId !== featuredKey)
      setFeaturedKey(nextExisting?.petImageId ?? null)
    }
  }, [removedIds, featuredKey, pet.images])

  const totalImages = existingVisible.length + newImages.length
  const slotsLeft   = MAX_IMAGES - totalImages

  // ── Submission ────────────────────────────────────────────────────
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError]               = useState<string | null>(null)

  async function handleSave() {
    flushSync(() => { setError(null); setIsSubmitting(true) })
    try {
      await updatePet(pet.petId, {
        name: name.trim(),
        petTypeValue,
        breedValue,
        ...(dateOfBirth && { dateOfBirth }),
        ...(description.trim() && { description: description.trim() }),
      })

      if (removedIds.size > 0) {
        await Promise.all([...removedIds].map(id => removePetImage(pet.petId, id)))
      }

      let uploadedIds: (string | null)[] = []
      if (newImages.length > 0) {
        const result = await addPetImages(pet.petId, newImages.map(img => img.file))
        uploadedIds = result.results.map(r => (r.success ? r.imageId : null))
      }

      if (featuredKey !== null) {
        if (featuredKey.startsWith('blob:')) {
          const newIdx  = newImages.findIndex(img => img.previewUrl === featuredKey)
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

  return (
    <div className="fixed inset-0 z-[400] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
      <div className="relative w-full max-w-[480px] max-h-[90vh] overflow-y-auto bg-card rounded-3xl shadow-[0_32px_80px_rgba(0,0,0,0.22)] p-8">

        {/* Close */}
        <button
          onClick={onClose}
          disabled={isSubmitting}
          aria-label="Close"
          className="absolute top-3.5 right-3.5 flex items-center justify-center w-8 h-8 rounded-lg bg-transparent border-0 cursor-pointer text-muted-foreground hover:text-foreground transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <X className="w-5 h-5" />
        </button>

        {/* Brand */}
        <div className="flex items-center gap-2 mb-[18px]">
          <BarkfestMark size={22} />
          <span className="font-heading font-bold text-[17px]">Barkfest</span>
        </div>

        {/* Title */}
        <div className="mb-4">
          <h2 className="font-heading font-bold text-[22px] mb-1 [font-variant-numeric:lining-nums]">
            {step === 1 ? `Edit ${pet.name}` : <>Manage photos for <strong className="text-primary">{name}</strong>.</>}
          </h2>
          {step === 1 && (
            <p className="text-[13.5px] text-muted-foreground">Update the details below.</p>
          )}
        </div>

        {/* Progress bar */}
        <div className="flex gap-2 mb-5">
          {[{ label: 'Details', s: 1 }, { label: 'Photos', s: 2 }].map(({ label, s }) => (
            <div key={s} className="flex-1">
              <div className={cn('h-1 rounded-sm transition-colors', s <= step ? 'bg-primary' : 'bg-border')} />
              <span className={cn('block text-[11px] mt-1', s <= step ? 'font-semibold text-primary' : 'text-muted-foreground')}>
                {label}
              </span>
            </div>
          ))}
        </div>

        {/* ── Step 1: Details ── */}
        {step === 1 && (
          <div className="space-y-4">
            <div>
              <label className="block text-[13px] font-semibold mb-1.5 text-foreground">Name *</label>
              <input
                value={name}
                onChange={e => setName(e.target.value)}
                maxLength={75}
                autoFocus
                className={inputCls}
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <PetTypeBreedFormFields
                petTypeValue={petTypeValue}
                onPetTypeChange={v => { setPetTypeValue(v); setUserBreedValue(0) }}
                breedValue={breedValue}
                onBreedChange={setUserBreedValue}
              />
            </div>

            <div>
              <label className="block text-[13px] font-semibold mb-1.5 text-foreground">Date of birth</label>
              <input
                type="date"
                max={today}
                value={dateOfBirth}
                onChange={e => setDateOfBirth(e.target.value)}
                className={inputCls}
              />
            </div>

            <div>
              <label className="block text-[13px] font-semibold mb-1.5 text-foreground">Description *</label>
              <textarea
                value={description}
                onChange={e => setDescription(e.target.value)}
                placeholder="Tell us about this pet…"
                className={cn(inputCls, 'h-20 py-2.5 resize-none')}
              />
            </div>

            <div className="flex gap-2">
              <OutlineBtn onClick={onClose}>Cancel</OutlineBtn>
              <PrimaryBtn onClick={() => setStep(2)} disabled={!step1Valid}>
                Next <ChevronRight className="w-4 h-4" />
              </PrimaryBtn>
            </div>
          </div>
        )}

        {/* ── Step 2: Photos ── */}
        {step === 2 && (
          <div className="space-y-3">

            {/* Add more drop target */}
            <p className="text-[13px] text-muted-foreground">
              Add up to {MAX_IMAGES} photos of {name.trim() || pet.name}.
            </p>
            <DropZone
              getRootProps={getRootProps}
              getInputProps={getInputProps}
              isDragActive={isDragActive}
              disabled={slotsLeft === 0}
              hint={slotsLeft > 0 ? `JPG or PNG · max 10 MB · up to ${slotsLeft} more` : undefined}
            />

            {/* Photo grid */}
            {totalImages > 0 && (
              <p className="text-[13px] text-muted-foreground">
                Tap a photo to set it as the cover. Click on 'X' to remove.
              </p>
            )}
            {totalImages > 0 && (
              <div className="grid grid-cols-3 gap-2">
                {existingVisible.map((img) => {
                  const isFeatured = featuredKey === img.petImageId
                  return (
                    <div
                      key={img.petImageId}
                      onClick={() => setFeaturedKey(img.petImageId)}
                      className={cn(
                        'relative aspect-square rounded-[10px] overflow-hidden cursor-pointer outline-2 outline-offset-2 transition-[outline-color]',
                        isFeatured ? 'outline outline-primary' : 'outline outline-transparent'
                      )}
                    >
                      <img src={getBlobImageUrl(img.blobName)} alt="" className="w-full h-full object-cover block" />
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
                      className={cn(
                        'relative aspect-square rounded-[10px] overflow-hidden cursor-pointer outline-2 outline-offset-2',
                        isFeatured ? 'outline outline-primary' : 'outline outline-transparent'
                      )}
                    >
                      <img src={img.previewUrl} alt={`New photo ${idx + 1}`} className="w-full h-full object-cover block" />
                      {isFeatured && <FeaturedBadge />}
                      <RemoveBtn onClick={e => { e.stopPropagation(); removeNew(idx) }} />
                    </div>
                  )
                })}
              </div>
            )}

            {totalImages === 0 && (
              <p className="text-[13px] text-destructive text-center">
                At least 1 photo is required.
              </p>
            )}

            {error && (
              <p className="text-[13px] text-destructive">{error}</p>
            )}

            <div className="flex gap-2">
              <OutlineBtn onClick={() => { setStep(1); setError(null) }} disabled={isSubmitting}>
                Back
              </OutlineBtn>
              <PrimaryBtn onClick={handleSave} disabled={totalImages === 0 || isSubmitting}>
                {isSubmitting
                  ? <><Loader2 className="w-4 h-4 animate-spin" />Saving…</>
                  : 'Save pet!'}
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
    <div className="absolute top-[5px] left-[5px] w-[18px] h-[18px] rounded-full bg-primary flex items-center justify-center">
      <Star className="w-[10px] h-[10px] fill-white stroke-none" />
    </div>
  )
}

function RemoveBtn({ onClick }: { onClick: (e: React.MouseEvent) => void }) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-label="Remove photo"
      className="absolute top-[5px] right-[5px] w-5 h-5 rounded-full bg-black/60 text-white border-0 cursor-pointer flex items-center justify-center text-sm font-bold leading-none hover:bg-black/80 transition-colors"
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
      type="button"
      onClick={onClick}
      disabled={disabled}
      className="flex-1 h-11 rounded-xl border-[1.5px] border-border bg-transparent text-muted-foreground text-sm font-medium cursor-pointer disabled:cursor-not-allowed disabled:opacity-50 transition-opacity"
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
      type="button"
      onClick={onClick}
      disabled={disabled}
      className="flex-1 h-11 rounded-xl bg-primary text-white text-sm font-semibold flex items-center justify-center gap-1.5 cursor-pointer disabled:cursor-not-allowed disabled:opacity-50 transition-opacity"
    >
      {children}
    </button>
  )
}
