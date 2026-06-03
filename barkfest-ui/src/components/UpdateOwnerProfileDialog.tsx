import { useCallback, useEffect, useRef, useState, type ChangeEvent } from 'react'
import { flushSync } from 'react-dom'
import isEmail from 'validator/lib/isEmail'
import { useQueryClient } from '@tanstack/react-query'
import { ChevronRight, Loader2, Upload, UserCircle, X } from 'lucide-react'
import { BarkfestMark } from '@/components/BarkfestMark'
import { ChangePasswordDialog } from '@/components/ChangePasswordDialog'
import { useDropzone } from 'react-dropzone'
import { cn } from '@/lib/utils'
import { useAuth } from '@/hooks/useAuth'
import {
  getOwnerById,
  updateOwner,
  uploadOwnerProfileImage,
  removeOwnerProfileImage,
  checkDisplayName,
  ApiError,
} from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'

interface UpdateOwnerProfileDialogProps {
  onClose: () => void
}

// Matches PetImage.MaxImageSizeBytes in Barkfest.Domain
const MAX_SIZE_BYTES = 10 * 1024 * 1024

export function UpdateOwnerProfileDialog({ onClose }: UpdateOwnerProfileDialogProps) {
  const { accountId, setProfileImage } = useAuth()
  const queryClient = useQueryClient()

  // ── Navigation ────────────────────────────────────────────────────────
  const [step, setStep] = useState<1 | 2>(1)

  // ── Loading state ─────────────────────────────────────────────────────
  const [isFetching, setIsFetching] = useState(true)
  const [fetchError, setFetchError] = useState<string | null>(null)

  // ── Step 1 fields ─────────────────────────────────────────────────────
  const [username, setUsername] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [displayName, setDisplayName] = useState('')

  // Saved display name from the server — availability check skips this value
  const [savedDisplayName, setSavedDisplayName] = useState('')

  // Display name availability check
  const [displayNameAvailable, setDisplayNameAvailable] = useState<boolean | null>(null)
  const [displayNameChecking, setDisplayNameChecking] = useState(false)
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  // ── Step 2 — profile image ────────────────────────────────────────────
  const [existingBlobName, setExistingBlobName] = useState<string | null>(null)
  const [newImageFile, setNewImageFile] = useState<File | null>(null)
  const [newImagePreviewUrl, setNewImagePreviewUrl] = useState<string | null>(null)
  const [imageCleared, setImageCleared] = useState(false)
  const [imageError, setImageError] = useState<string | null>(null)
  const previewUrlRef = useRef<string | null>(null)

  // ── Submission ────────────────────────────────────────────────────────
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [changePasswordOpen, setChangePasswordOpen] = useState(false)

  // ── Derived state ─────────────────────────────────────────────────────
  const displayNameStripped = displayName.replace(/\s/g, '')
  const displayNameTooShort = displayNameStripped.length > 0 && displayNameStripped.length < 4
  const displayNameChanged = displayName.trim() !== savedDisplayName.trim()

  const emailInvalid = email.trim() !== '' && !isEmail(email.trim())

  const step1Valid =
    firstName.trim() !== '' &&
    lastName.trim() !== '' &&
    email.trim() !== '' &&
    !emailInvalid &&
    !displayNameTooShort &&
    !displayNameChecking

  const showExistingImage = !newImageFile && !imageCleared && existingBlobName !== null
  const showNewImage = newImageFile !== null

  // ── Fetch owner on open ───────────────────────────────────────────────
  useEffect(() => {
    if (!accountId) return

    async function fetchOwner() {
      setIsFetching(true)
      setFetchError(null)
      try {
        const owner = await getOwnerById(accountId!)
        setUsername(owner.username)
        setFirstName(owner.firstName)
        setLastName(owner.lastName)
        setEmail(owner.email)
        setDisplayName(owner.displayName ?? '')
        setSavedDisplayName(owner.displayName ?? '')
        setExistingBlobName(owner.profileImage?.blobName ?? null)
      } catch {
        setFetchError('Failed to load your profile. Please try again.')
      } finally {
        setIsFetching(false)
      }
    }

    fetchOwner()
  }, [accountId])

  // ── Display name availability check ──────────────────────────────────
  const checkDN = useCallback((value: string) => {
    if (debounceRef.current) clearTimeout(debounceRef.current)
    const trimmed = value.trim()
    if (!trimmed || trimmed.replace(/\s/g, '').length < 4 || trimmed === savedDisplayName.trim()) {
      setDisplayNameAvailable(null)
      setDisplayNameChecking(false)
      return
    }
    setDisplayNameAvailable(null)
    debounceRef.current = setTimeout(async () => {
      setDisplayNameChecking(true)
      try {
        const available = await checkDisplayName(trimmed)
        setDisplayNameAvailable(available)
      } catch {
        setDisplayNameAvailable(null)
      } finally {
        setDisplayNameChecking(false)
      }
    }, 500)
  }, [savedDisplayName])

  useEffect(() => {
    checkDN(displayName)
  }, [displayName, checkDN])

  // ── Revoke object URL on unmount ──────────────────────────────────────
  useEffect(() => {
    return () => {
      if (previewUrlRef.current) URL.revokeObjectURL(previewUrlRef.current)
    }
  }, [])

  // ── Dropzone ──────────────────────────────────────────────────────────
  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop(accepted, rejected) {
      setImageError(null)
      if (rejected.length > 0) {
        const tooLarge = rejected.some(r => r.errors.some(e => e.code === 'file-too-large'))
        const badType  = rejected.some(r => r.errors.some(e => e.code === 'file-invalid-type'))
        if (tooLarge) setImageError('Photo must be 10 MB or smaller.')
        else if (badType) setImageError('Only JPG and PNG photos are accepted.')
        return
      }
      if (accepted.length === 0) return

      if (previewUrlRef.current) URL.revokeObjectURL(previewUrlRef.current)
      const url = URL.createObjectURL(accepted[0])
      previewUrlRef.current = url
      setNewImageFile(accepted[0])
      setNewImagePreviewUrl(url)
      setImageCleared(false)
    },
    accept: { 'image/jpeg': ['.jpg', '.jpeg'], 'image/png': ['.png'] },
    maxSize: MAX_SIZE_BYTES,
    multiple: false,
    disabled: isSubmitting || showNewImage || showExistingImage,
  })

  function handleClearImage() {
    if (newImageFile) {
      if (previewUrlRef.current) {
        URL.revokeObjectURL(previewUrlRef.current)
        previewUrlRef.current = null
      }
      setNewImageFile(null)
      setNewImagePreviewUrl(null)
    } else {
      setImageCleared(true)
    }
  }

  // ── Submit ────────────────────────────────────────────────────────────
  async function handleSave() {
    if (!accountId) return

    // flushSync forces the spinner to render before the first await — without
    // it, React 18's automatic batching can delay the re-render until after
    // fast localhost responses have already returned.
    flushSync(() => {
      setIsSubmitting(true)
      setSubmitError(null)
    })

    try {
      // 1. Always update personal info
      await updateOwner(accountId, {
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        email: email.trim(),
        displayName: displayName.trim() || null,
      })

      // 2. Handle profile image changes
      if (newImageFile) {
        await uploadOwnerProfileImage(accountId, newImageFile)
        // Re-fetch to get the server-assigned blob name
        const updated = await getOwnerById(accountId)
        const newBlobName = updated.profileImage?.blobName ?? null
        setProfileImage(newBlobName)
        queryClient.setQueryData(['owner', accountId, 'profile-image'], newBlobName)
      } else if (imageCleared && existingBlobName) {
        await removeOwnerProfileImage(accountId)
        setProfileImage(null)
        queryClient.setQueryData(['owner', accountId, 'profile-image'], null)
      }
      // No image change — context unchanged

      // Only invalidate browse cache if the display name changed — it's the
      // only owner field visible on pet tiles.
      if (displayNameChanged) {
        queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
        queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
      }

      onClose()
    } catch (err) {
      if (err instanceof ApiError && err.status < 500) {
        setSubmitError(err.message)
      } else {
        setSubmitError('Something went wrong. Please try again.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  // ── Render ────────────────────────────────────────────────────────────
  return (
    <>
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
      <div className="relative w-full max-w-sm bg-card rounded-3xl shadow-2xl p-8">

        {/* Close button */}
        <button
          onClick={onClose}
          disabled={isSubmitting}
          aria-label="Close"
          className="absolute top-4 right-4 text-muted-foreground hover:text-foreground transition-colors disabled:opacity-50"
        >
          <X className="w-5 h-5" />
        </button>

        {/* Header */}
        <div className="mb-6">
          <div className="flex items-center gap-2 mb-4">
            <BarkfestMark size={22} />
            <span className="font-heading font-bold" style={{ fontSize: '17px' }}>Barkfest</span>
          </div>
          <h2 className="text-2xl font-bold">
            {step === 1 ? 'Your Barkfest Profile' : 'Put a face to the name.'}
          </h2>
          <p className="text-sm text-muted-foreground mt-1">
            {step === 1 ? 'Keep your details up to date.' : 'Add or update your profile photo.'}
          </p>
        </div>

        {/* Progress bar */}
        <div className="flex gap-1.5 mb-6">
          {[1, 2].map(s => (
            <div
              key={s}
              className={cn(
                'h-1 flex-1 rounded-full transition-colors',
                s <= step ? 'bg-primary' : 'bg-border'
              )}
            />
          ))}
        </div>

        {/* ── Loading / Error state ── */}
        {isFetching ? (
          <div className="flex items-center justify-center py-12">
            <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
          </div>
        ) : fetchError ? (
          <div className="py-8 text-center">
            <p className="text-sm text-destructive">{fetchError}</p>
          </div>
        ) : step === 1 ? (
          /* ── Step 1: Personal Info ── */
          <div className="space-y-4">

            {/* Username — read-only info line */}
            <div className="space-y-1.5">
              <p className="text-sm font-medium">Username</p>
              <p className="text-sm text-muted-foreground">{username}</p>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <ProfileField
                label="First name"
                id="upd-firstName"
                required
                maxLength={50}
                value={firstName}
                onChange={e => setFirstName(e.target.value)}
              />
              <ProfileField
                label="Last name"
                id="upd-lastName"
                required
                maxLength={100}
                value={lastName}
                onChange={e => setLastName(e.target.value)}
              />
            </div>

            <div className="space-y-1">
              <ProfileField
                label="Email"
                id="upd-email"
                type="email"
                required
                maxLength={75}
                value={email}
                onChange={e => setEmail(e.target.value)}
              />
              {emailInvalid && (
                <p className="text-xs text-destructive">Must be a valid email address.</p>
              )}
            </div>

            <div className="space-y-1">
              <ProfileField
                label="Display name"
                id="upd-displayName"
                maxLength={25}
                placeholder="Shown on pet cards"
                value={displayName}
                onChange={e => setDisplayName(e.target.value)}
              />
              {displayName.trim() && (
                displayNameTooShort ? (
                  <p className="text-xs text-destructive">At least 4 characters required</p>
                ) : displayNameChanged && (displayNameChecking || displayNameAvailable !== null) ? (
                  <p className={cn('text-xs flex items-center gap-1',
                    displayNameChecking ? 'text-muted-foreground'
                    : displayNameAvailable ? 'text-green-500'
                    : 'text-destructive'
                  )}>
                    {displayNameChecking && <Loader2 className="w-3 h-3 animate-spin" />}
                    {displayNameChecking ? 'Checking…'
                      : displayNameAvailable ? '✓ Available'
                      : 'Already taken'}
                  </p>
                ) : null
              )}
            </div>

            {/* Change password link */}
            <button
              type="button"
              onClick={() => setChangePasswordOpen(true)}
              className="text-sm font-medium hover:underline transition-opacity hover:opacity-80"
              style={{ color: 'var(--primary)' }}
            >
              Change password →
            </button>

            <div className="flex gap-3 pt-2">
              <button
                type="button"
                onClick={onClose}
                className="flex-1 h-11 rounded-xl border-[1.5px] border-border bg-transparent text-muted-foreground text-sm font-medium hover:bg-secondary transition-colors"
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
        ) : (
          /* ── Step 2: Profile Image ── */
          <div className="space-y-5">

            {/* Image preview / dropzone */}
            {showExistingImage || showNewImage ? (
              <div className="relative w-full flex flex-col items-center gap-3">
                <div className="relative">
                  <img
                    src={showNewImage ? newImagePreviewUrl! : getBlobImageUrl(existingBlobName!, 'owner-profile-images')}
                    alt="Profile"
                    className="w-32 h-32 rounded-full object-cover ring-2 ring-border"
                  />
                  <button
                    type="button"
                    onClick={handleClearImage}
                    disabled={isSubmitting}
                    aria-label="Remove photo"
                    className="absolute -top-1 -right-1 w-6 h-6 rounded-full bg-primary/20 text-primary hover:bg-primary/30 transition-colors flex items-center justify-center shadow-sm disabled:opacity-50 disabled:pointer-events-none"
                  >
                    <X className="w-3.5 h-3.5" />
                  </button>
                </div>
                <p className="text-xs text-muted-foreground">
                  {showNewImage ? 'New photo selected' : 'Looking good!'}
                </p>
              </div>
            ) : (
              <div
                {...getRootProps()}
                className={cn(
                  'flex flex-col items-center justify-center gap-3 rounded-2xl border-2 border-dashed p-8 text-center transition-colors cursor-pointer',
                  isDragActive ? 'border-primary bg-primary/5' : 'border-border hover:border-primary/50 hover:bg-accent/30'
                )}
              >
                <input {...getInputProps()} />
                <div className="w-14 h-14 rounded-full bg-secondary flex items-center justify-center">
                  {isDragActive
                    ? <Upload className="w-7 h-7 text-primary" />
                    : <UserCircle className="w-10 h-10 text-primary" />
                  }
                </div>
                <div>
                  <p className="text-sm font-medium">
                    {isDragActive ? 'Drop your photo here' : 'Upload a profile photo'}
                  </p>
                  <p className="text-xs text-muted-foreground mt-0.5">JPG or PNG · max 10 MB · optional</p>
                </div>
              </div>
            )}

            {imageError && (
              <p className="text-xs text-destructive text-center">{imageError}</p>
            )}

            {submitError && (
              <p className="text-sm text-destructive text-center">{submitError}</p>
            )}

            <div className="flex gap-3 pt-2">
              <button
                type="button"
                onClick={() => { setStep(1); setSubmitError(null); setImageError(null) }}
                disabled={isSubmitting}
                className="flex-1 h-11 rounded-xl border-[1.5px] border-border bg-transparent text-muted-foreground text-sm font-medium hover:bg-secondary transition-colors disabled:opacity-50"
              >
                Back
              </button>
              <button
                type="button"
                onClick={handleSave}
                disabled={isSubmitting}
                className="flex-1 h-11 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-50 flex items-center justify-center gap-2"
              >
                {isSubmitting ? (
                  <>
                    <Loader2 className="w-4 h-4 animate-spin" />
                    Saving…
                  </>
                ) : 'Good to go!'}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>

    {changePasswordOpen && (
      <ChangePasswordDialog onClose={() => setChangePasswordOpen(false)} />
    )}
  </>
  )
}

interface ProfileFieldProps {
  label: string
  id: string
  type?: string
  required?: boolean
  maxLength?: number
  placeholder?: string
  value: string
  onChange: (e: ChangeEvent<HTMLInputElement>) => void
}

function ProfileField({ label, id, type = 'text', required, maxLength, placeholder, value, onChange }: ProfileFieldProps) {
  return (
    <div className="space-y-1.5">
      <label className="text-sm font-medium" htmlFor={id}>
        {label} {required && <span className="text-destructive">*</span>}
      </label>
      <input
        id={id}
        type={type}
        required={required}
        maxLength={maxLength}
        placeholder={placeholder}
        value={value}
        onChange={onChange}
        className="w-full h-11 rounded-xl border-[1.5px] border-border bg-background text-foreground px-3 text-sm outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary placeholder:text-muted-foreground transition"
      />
    </div>
  )
}
