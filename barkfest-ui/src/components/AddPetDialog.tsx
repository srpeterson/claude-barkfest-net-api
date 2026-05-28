import { useRef, useState } from 'react'
import { flushSync } from 'react-dom'
import {
  CalendarDays,
  ChevronRight,
  Loader2,
  Minus,
  Plus,
  Star,
  X,
} from 'lucide-react'
import { BarkfestMark } from '@/components/BarkfestMark'
import { cn } from '@/lib/utils'
import { DropZone } from '@/components/ui/DropZone'
import { PetTypeBreedFormFields } from '@/components/PetTypeBreedFormFields'
import { useImageUpload } from '@/hooks/useImageUpload'
import { addPetImages, createPet, setFeaturedImage } from '@/lib/api'
import type { CreatePetRequest } from '@/types/pet'

// Matches Pet.MaxImages in Barkfest.Domain
const MAX_IMAGES = 6

// Converts an approximate age (years) to a date of birth string (YYYY-MM-DD)
// by subtracting the given number of years from today's date.
function ageToDateOfBirth(years: number): string {
  const today = new Date()
  const year  = today.getFullYear() - years
  const month = String(today.getMonth() + 1).padStart(2, '0')
  const day   = String(today.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

interface AddPetDialogProps {
  onClose: () => void
  onSuccess?: () => void
}

export function AddPetDialog({ onClose, onSuccess }: AddPetDialogProps) {
  // ── Navigation ────────────────────────────────────────────────────────
  const [step, setStep] = useState<1 | 2>(1)

  // ── Step 1 fields ─────────────────────────────────────────────────────
  const [name, setName] = useState('')
  const [petTypeValue, setPetTypeValue] = useState(0)
  const [breedValue, setBreedValue] = useState(0)
  const [dateOfBirth, setDateOfBirth] = useState('')
  const [dobUnknown, setDobUnknown] = useState(false)
  const [age, setAge] = useState('')
  const [description, setDescription] = useState('')

  // ── Step 2 — image upload ─────────────────────────────────────────────
  const {
    images,
    featuredIndex,
    setFeaturedIndex,
    removeImage,
    uploadError,
    setUploadError,
    getRootProps,
    getInputProps,
    isDragActive,
  } = useImageUpload({ maxFiles: MAX_IMAGES })

  // ── Refs ──────────────────────────────────────────────────────────
  const dateInputRef = useRef<HTMLInputElement>(null)

  // ── Submission state ──────────────────────────────────────────────────
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const today = new Date().toISOString().split('T')[0]
  const minDate = (() => { const d = new Date(); d.setFullYear(d.getFullYear() - 21); return d.toISOString().split('T')[0] })()
  const dateInRange = !dateOfBirth || (dateOfBirth >= minDate && dateOfBirth <= today)
  const step1Valid = name.trim() !== '' && petTypeValue !== 0 && breedValue !== 0 &&
    (dobUnknown ? age !== '' : dateOfBirth !== '' && dateInRange) && description.trim() !== ''
  const step2Valid = images.length > 0

  // ── Handlers ──────────────────────────────────────────────────────────
  function handlePetTypeChange(value: number) {
    setPetTypeValue(value)
    setBreedValue(0)
  }

  async function handleSubmit() {
    flushSync(() => {
      setError(null)
      setIsSubmitting(true)
    })

    try {
      const petData: CreatePetRequest = {
        name: name.trim(),
        petTypeValue,
        breedValue,
        dateOfBirth: dobUnknown ? ageToDateOfBirth(Number(age)) : dateOfBirth,
        ...(description.trim() && { description: description.trim() }),
      }

      const petId = await createPet(petData)

      const uploadResult = await addPetImages(petId, images.map(img => img.file))

      // Set featured image — prefer the user's selection, fall back to first success
      const successfulResults = uploadResult.results.filter(r => r.success && r.imageId)
      if (successfulResults.length > 0) {
        const preferred = uploadResult.results[featuredIndex]
        const target    = preferred?.success && preferred.imageId
          ? preferred
          : successfulResults[0]

        if (target.imageId) {
          await setFeaturedImage(petId, target.imageId)
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

  // ── Dialog ────────────────────────────────────────────────────────────
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
      <div className="relative w-full max-w-lg bg-card rounded-3xl shadow-2xl flex flex-col max-h-[90vh]">

        {/* Header */}
        <div className="flex items-center gap-2 px-6 pt-6 pb-0">
          <BarkfestMark size={22} />
          <span className="font-heading font-bold" style={{ fontSize: '17px' }}>
            {step === 1 ? 'Add your furry friend' : `Time to shine, ${name}!`}
          </span>
          <button
            onClick={onClose}
            disabled={isSubmitting}
            className="absolute top-5 right-5 text-muted-foreground hover:text-foreground transition-colors disabled:opacity-40"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Step progress bar */}
        <div className="flex gap-2 px-6 pt-3 pb-4">
          <div className="flex-1 space-y-1.5">
            <div className="h-1 rounded-full bg-primary" />
            <span className="block text-xs font-medium text-primary">The basics</span>
          </div>
          <div className="flex-1 space-y-1.5">
            <div className={cn('h-1 rounded-full transition-colors', step === 2 ? 'bg-primary' : 'bg-border')} />
            <span className={cn('block text-xs transition-colors', step === 2 ? 'font-medium text-primary' : 'text-muted-foreground')}>Showtime</span>
          </div>
        </div>

        {/* ── Step 1 — Pet details ──────────────────────────────────── */}
        {step === 1 && (
          <div className="px-6 pb-6 overflow-y-auto space-y-4">

            <div className="space-y-1.5">
              <label className="text-sm font-semibold">
                Name <span className="text-destructive">*</span>
              </label>
              <input
                type="text"
                autoFocus
                placeholder="What is your fur baby's name?"
                maxLength={75}
                value={name}
                onChange={e => setName(e.target.value)}
                className="w-full h-11 rounded-xl border border-input bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/50 placeholder:text-muted-foreground"
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <PetTypeBreedFormFields
                petTypeValue={petTypeValue}
                onPetTypeChange={handlePetTypeChange}
                breedValue={breedValue}
                onBreedChange={setBreedValue}
              />
            </div>

            <div className="space-y-1.5">
              <label className="text-sm font-semibold block">
                How old is {name.trim() || 'your pet'}? <span className="text-destructive">*</span>
              </label>
              <div className="grid grid-cols-2 gap-3">

                {/* ── Date of birth column ── */}
                <div className="space-y-1.5">
                  <label className="flex items-center gap-2.5 cursor-pointer select-none">
                    <input
                      type="radio"
                      name="dob-mode"
                      checked={!dobUnknown}
                      onChange={() => { setDobUnknown(false); setAge('') }}
                      className="sr-only"
                    />
                    <div className={cn(
                      'w-4 h-4 rounded-full border-2 flex items-center justify-center transition-colors shrink-0',
                      !dobUnknown ? 'border-primary' : 'border-input'
                    )}>
                      {!dobUnknown && <div className="w-2 h-2 rounded-full bg-primary" />}
                    </div>
                    <span className="text-sm font-semibold">
                      I know exactly!
                    </span>
                  </label>
                  <div className={cn('relative', dobUnknown && 'opacity-40 pointer-events-none')}>
                    <input
                      ref={dateInputRef}
                      type="date"
                      min={minDate}
                      max={today}
                      value={dateOfBirth}
                      onKeyDown={e => e.preventDefault()}
                      onClick={() => dateInputRef.current?.showPicker()}
                      onChange={e => setDateOfBirth(e.target.value)}
                      className={cn(
                        'w-full h-11 rounded-xl border border-input bg-background px-3 pr-10 text-sm focus:outline-none focus:ring-2 focus:ring-primary/50 [&::-webkit-calendar-picker-indicator]:hidden cursor-pointer',
                        !dobUnknown && 'ring-2 ring-primary/50'
                      )}
                    />
                    <button
                      type="button"
                      onClick={() => dateInputRef.current?.showPicker()}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
                    >
                      <CalendarDays className="w-4 h-4" />
                    </button>
                  </div>
                </div>

                {/* ── Approximate age column ── */}
                <div className="space-y-1.5">
                  <label className="flex items-center gap-2.5 cursor-pointer select-none">
                    <input
                      type="radio"
                      name="dob-mode"
                      checked={dobUnknown}
                      onChange={() => { setDobUnknown(true); setDateOfBirth('') }}
                      className="sr-only"
                    />
                    <div className={cn(
                      'w-4 h-4 rounded-full border-2 flex items-center justify-center transition-colors shrink-0',
                      dobUnknown ? 'border-primary' : 'border-input'
                    )}>
                      {dobUnknown && <div className="w-2 h-2 rounded-full bg-primary" />}
                    </div>
                    <span className="text-sm font-semibold">
                      Rescue / Not sure
                    </span>
                  </label>
                  <div className={cn(
                    'flex items-center h-11 rounded-xl border border-input bg-background focus-within:ring-2 focus-within:ring-primary/50',
                    !dobUnknown && 'opacity-40 pointer-events-none',
                    dobUnknown && 'ring-2 ring-primary/50'
                  )}>
                    <button
                      type="button"
                      onClick={() => setAge(String(Math.max(1, Number(age) - 1)))}
                      disabled={!age || Number(age) <= 1}
                      className="flex items-center justify-center w-12 h-full text-muted-foreground hover:text-foreground disabled:opacity-30 transition-colors"
                    >
                      <Minus className="w-4 h-4" />
                    </button>
                    <span className={cn('flex-1 text-center text-sm select-none', !age && 'text-muted-foreground')}>
                      {age ? `${age} ${Number(age) === 1 ? 'year' : 'years'} old` : 'Tap + to set age'}
                    </span>
                    <button
                      type="button"
                      onClick={() => setAge(String(Math.min(21, Number(age || '0') + 1)))}
                      disabled={Number(age) >= 21}
                      className="flex items-center justify-center w-12 h-full text-muted-foreground hover:text-foreground disabled:opacity-30 transition-colors"
                    >
                      <Plus className="w-4 h-4" />
                    </button>
                  </div>
                </div>

              </div>
              {dateOfBirth && !dateInRange && (
                <p className="text-xs text-destructive">Date must be within the last 21 years and not in the future.</p>
              )}
            </div>

            <div className="space-y-1.5">
              <label className="text-sm font-semibold">
                Description <span className="text-destructive">*</span>
              </label>
              <textarea
                placeholder="Lazy couch potato or zoomies champion? Tell us!"
                rows={3}
                value={description}
                onChange={e => setDescription(e.target.value)}
                className="w-full rounded-xl border border-input bg-background px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/50 placeholder:text-muted-foreground resize-none"
              />
            </div>

            <div className="flex gap-3 pt-2">
              <button
                type="button"
                onClick={onClose}
                className="flex-1 h-11 rounded-xl text-sm font-medium transition-colors hover:bg-secondary"
                style={{ border: '1.5px solid var(--border)', background: 'transparent', color: 'var(--muted-foreground)' }}
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={() => setStep(2)}
                disabled={!step1Valid}
                className="flex-1 h-11 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-50 flex items-center justify-center gap-1.5"
              >
                Next
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}

        {/* ── Step 2 — Photos ───────────────────────────────────────── */}
        {step === 2 && (
          <div className="px-6 pb-6 overflow-y-auto space-y-4">

            <p className="text-sm text-muted-foreground">
              Add photos to give <span className="font-bold text-primary">{name}</span> their moment in the spotlight!
            </p>
            {images.length > 0 && (
              <p className="flex items-center gap-1.5 text-sm text-muted-foreground">
                <Star className="w-3.5 h-3.5 text-primary fill-current shrink-0" /> Tap a photo to feature it.
              </p>
            )}

            {/* Upload zone — hidden once the limit is reached */}
            {images.length < MAX_IMAGES && (
              <DropZone
                getRootProps={getRootProps}
                getInputProps={getInputProps}
                isDragActive={isDragActive}
                hint={`JPG or PNG · up to ${MAX_IMAGES - images.length} more`}
              />
            )}

            {/* Thumbnail grid */}
            {images.length > 0 && (
              <div className="grid grid-cols-3 gap-2">
                {images.map((img, i) => (
                  <div
                    key={img.previewUrl}
                    onClick={() => setFeaturedIndex(i)}
                    className={cn(
                      'relative aspect-square rounded-xl overflow-hidden cursor-pointer ring-2 transition-all',
                      i === featuredIndex
                        ? 'ring-primary'
                        : 'ring-transparent hover:ring-primary/40'
                    )}
                  >
                    <img
                      src={img.previewUrl}
                      alt={`Upload ${i + 1}`}
                      className="w-full h-full object-cover"
                    />
                    {/* Featured badge */}
                    {i === featuredIndex && (
                      <div className="absolute top-1.5 left-1.5 bg-primary rounded-full p-0.5">
                        <Star className="w-3 h-3 text-primary-foreground fill-current" />
                      </div>
                    )}
                    {/* Remove button */}
                    <button
                      type="button"
                      onClick={e => { e.stopPropagation(); removeImage(i) }}
                      disabled={isSubmitting}
                      className="absolute top-1.5 right-1.5 bg-black/60 rounded-full p-0.5 text-white hover:bg-black/80 transition-colors disabled:opacity-40"
                    >
                      <X className="w-3 h-3" />
                    </button>
                  </div>
                ))}
              </div>
            )}

            {images.length === 0 && (
              <p className="text-sm text-destructive text-center py-1">
                At least 1 photo is required.
              </p>
            )}

            {uploadError && (
              <p className="text-sm text-destructive">{uploadError}</p>
            )}

            {error && (
              <p className="text-sm text-destructive">{error}</p>
            )}

            <div className="flex gap-3 pt-2">
              <button
                type="button"
                onClick={() => { setStep(1); setError(null); setUploadError(null) }}
                disabled={isSubmitting}
                className="flex-1 h-11 rounded-xl text-sm font-medium transition-colors hover:bg-secondary disabled:opacity-50"
                style={{ border: '1.5px solid var(--border)', background: 'transparent', color: 'var(--muted-foreground)' }}
              >
                Back
              </button>
              <button
                type="button"
                onClick={handleSubmit}
                disabled={!step2Valid || isSubmitting}
                className="flex-1 h-11 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-50 flex items-center justify-center gap-2"
              >
                {isSubmitting && <Loader2 className="w-4 h-4 animate-spin" />}
                {isSubmitting ? 'Making it official...' : 'Make it official!'}
              </button>
            </div>
          </div>
        )}

      </div>
    </div>
  )
}
