import { useState } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, ChevronLeft, ChevronRight, Heart, Loader2, MoreHorizontal, Pencil, Trash2, UserCircle, X } from 'lucide-react'
import { deletePet, getOwnerById, getPetDetail, likePet, unlikePet } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { useAuth } from '@/hooks/useAuth'
import { BarkfestMark } from '@/components/BarkfestMark'
import { Navbar } from '@/components/Navbar'

function formatAge(dateOfBirth?: string): string | null {
  if (!dateOfBirth) return null
  const dob = new Date(dateOfBirth)
  const today = new Date()
  let months = (today.getFullYear() - dob.getFullYear()) * 12 + (today.getMonth() - dob.getMonth())
  if (today.getDate() < dob.getDate()) months--
  months = Math.max(months, 0)
  if (months < 12) return months === 1 ? '1 month' : `${months} months`
  const years = Math.floor(months / 12)
  return years === 1 ? '1 year' : `${years} years`
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', year: 'numeric' })
}

export function PetDetailPage() {
  const { petId } = useParams<{ petId: string }>()
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { accountId, isAuthenticated, accountType } = useAuth()

  const [liked, setLiked] = useState(false)
  const [likeCount, setLikeCount] = useState<number | null>(null)
  const [kebabOpen, setKebabOpen] = useState(false)
  const [deleteOpen, setDeleteOpen] = useState(false)
  const [lightboxIndex, setLightboxIndex] = useState<number | null>(null)
  const [isDeleting, setIsDeleting] = useState(false)

  const { data: pet, isLoading, isError } = useQuery({
    queryKey: ['pet', petId],
    queryFn: () => getPetDetail(petId!),
    enabled: !!petId,
  })

  // Fetch owner info — only available for authenticated users (endpoint requires JWT)
  const { data: owner } = useQuery({
    queryKey: ['owner', pet?.ownerId],
    queryFn: () => getOwnerById(pet!.ownerId),
    enabled: !!pet?.ownerId && isAuthenticated,
  })

  const isOwner = isAuthenticated && accountType === 'owner' && accountId === pet?.ownerId
  const displayLikes = likeCount !== null ? likeCount : (pet?.likes ?? 0)
  const fromManage = searchParams.get('from') === 'manage'

  async function handleLike() {
    if (!pet) return
    const next = !liked
    const prev = likeCount !== null ? likeCount : pet.likes
    setLiked(next)
    setLikeCount(next ? prev + 1 : Math.max(0, prev - 1))
    try {
      if (next) await likePet(pet.petId)
      else await unlikePet(pet.petId)
    } catch {
      setLiked(!next)
      setLikeCount(prev)
    }
  }

  async function handleDelete() {
    if (!pet) return
    setIsDeleting(true)
    try {
      await deletePet(pet.petId)
      queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
      queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
      navigate('/')
    } catch {
      setIsDeleting(false)
    }
  }

  function handleLightboxNav(dir: 1 | -1) {
    if (lightboxIndex === null || !pet) return
    const count = pet.images.length
    setLightboxIndex((lightboxIndex + dir + count) % count)
  }

  if (isLoading) {
    return (
      <div className="min-h-screen" style={{ background: 'var(--background)' }}>
        <Navbar />
        <div className="flex items-center justify-center pt-32">
          <Loader2 className="w-8 h-8 animate-spin text-primary" />
        </div>
      </div>
    )
  }

  if (isError || !pet) {
    return (
      <div className="min-h-screen" style={{ background: 'var(--background)' }}>
        <Navbar />
        <div className="flex flex-col items-center justify-center pt-32 gap-4">
          <p className="text-muted-foreground">Pet not found.</p>
          <Link to="/" className="text-sm font-medium text-primary hover:underline">← Back to Barkfest</Link>
        </div>
      </div>
    )
  }

  const heroImage = pet.images.find(i => i.isFeaturedImage) ?? pet.images[0]
  const age = formatAge(pet.dateOfBirth)

  return (
    <div className="min-h-screen" style={{ background: 'var(--background)' }}>
      {/* Custom navbar for pet detail — back link */}
      <div className="sticky top-0 z-50 px-4 pt-[10px] pb-[10px]">
        <nav
          className="flex items-center justify-between h-[52px] px-[22px] rounded-full"
          style={{ background: 'var(--primary)', boxShadow: '0 4px 24px rgba(0,0,0,0.18)' }}
        >
          <Link
            to={fromManage ? '/manage' : '/'}
            className="flex items-center gap-1.5 text-sm font-medium text-white hover:opacity-80 transition-opacity"
          >
            <ArrowLeft className="w-4 h-4" />
            {fromManage ? 'My Pets' : 'Back to Barkfest'}
          </Link>
        </nav>
      </div>

      {/* Hero image — full bleed */}
      <div
        className="relative overflow-hidden"
        style={{ height: 'clamp(320px, 45vw, 520px)' }}
      >
        {heroImage ? (
          <img
            src={getBlobImageUrl(heroImage.blobName)}
            alt={pet.name}
            className="w-full h-full object-cover"
          />
        ) : (
          <div className="w-full h-full bg-secondary flex items-center justify-center">
            <span className="text-6xl">🐾</span>
          </div>
        )}

        {/* Gradient overlay */}
        <div
          className="absolute inset-0"
          style={{ background: 'linear-gradient(to top, rgba(0,0,0,0.75) 0%, rgba(0,0,0,0.1) 50%, transparent 100%)' }}
        />

        {/* Name + badges pinned bottom-left */}
        <div className="absolute bottom-0 left-0 right-0 px-6 pb-8">
          <h1
            className="font-heading font-bold text-white drop-shadow mb-2"
            style={{ fontSize: 'clamp(32px, 4vw, 56px)' }}
          >
            {pet.name}
          </h1>
          <div className="flex flex-wrap gap-2">
            {age && (
              <span
                className="px-3 py-1 rounded-full text-sm text-white font-medium"
                style={{ background: 'rgba(255,255,255,0.2)', border: '1px solid rgba(255,255,255,0.3)' }}
              >
                {age}
              </span>
            )}
            {pet.breed && (
              <span
                className="px-3 py-1 rounded-full text-sm text-white font-medium"
                style={{ background: 'rgba(255,255,255,0.2)', border: '1px solid rgba(255,255,255,0.3)' }}
              >
                {pet.breed}
              </span>
            )}
          </div>
        </div>

        {/* Owner kebab — top right */}
        {isOwner && (
          <div className="absolute top-4 right-4">
            <button
              onClick={() => setKebabOpen(o => !o)}
              className="w-[38px] h-[38px] rounded-full flex items-center justify-center text-white transition-opacity hover:opacity-80"
              style={{ background: 'rgba(0,0,0,0.35)', backdropFilter: 'blur(6px)' }}
            >
              <MoreHorizontal className="w-5 h-5" />
            </button>

            {kebabOpen && (
              <div
                className="absolute right-0 top-[calc(100%+8px)] w-40 rounded-2xl overflow-hidden"
                style={{ background: 'var(--card)', border: '1px solid var(--border)', boxShadow: '0 16px 48px rgba(0,0,0,0.12)' }}
              >
                <button
                  onClick={() => { setKebabOpen(false) /* TODO: open EditPetModal */ }}
                  className="w-full flex items-center gap-3 px-4 py-3 text-sm font-medium hover:bg-secondary transition-colors"
                  style={{ color: 'var(--foreground)' }}
                >
                  <Pencil className="w-4 h-4 text-primary shrink-0" />
                  Edit pet
                </button>
                <button
                  onClick={() => { setKebabOpen(false); setDeleteOpen(true) }}
                  className="w-full flex items-center gap-3 px-4 py-3 text-sm font-medium hover:bg-secondary transition-colors"
                  style={{ color: 'var(--destructive)' }}
                >
                  <Trash2 className="w-4 h-4 shrink-0" />
                  Delete pet
                </button>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Floating info card */}
      <div className="max-w-2xl mx-auto px-4">
        <div
          className="relative rounded-[20px] p-7 sm:p-8"
          style={{
            background: 'var(--card)',
            boxShadow: '0 4px 24px rgba(0,0,0,0.08)',
            marginTop: '-40px',
          }}
        >
          {/* Description */}
          {pet.description && (
            <p className="text-sm leading-relaxed mb-5" style={{ color: 'var(--muted-foreground)' }}>
              {pet.description}
            </p>
          )}

          <div className="border-t mb-5" style={{ borderColor: 'var(--border)' }} />

          {/* Owner row + Like button */}
          <div className="flex items-center justify-between gap-4">
            <div className="flex items-center gap-3 min-w-0">
              {owner?.profileImage?.blobName ? (
                <img
                  src={getBlobImageUrl(owner.profileImage.blobName, 'owner-profile-images')}
                  alt={owner.displayName ?? owner.username}
                  className="w-9 h-9 rounded-full object-cover shrink-0"
                />
              ) : (
                <div className="w-9 h-9 rounded-full bg-secondary flex items-center justify-center shrink-0">
                  <UserCircle className="w-6 h-6 text-muted-foreground" />
                </div>
              )}
              <div className="min-w-0">
                {owner?.displayName && (
                  <p className="text-sm font-semibold leading-tight truncate">{owner.displayName}</p>
                )}
                <p className="text-xs truncate" style={{ color: 'var(--muted-foreground)' }}>
                  {owner ? `@${owner.username} · ` : ''}{formatDate(pet.createdAt)}
                </p>
              </div>
            </div>

            {/* Like button */}
            <button
              onClick={isOwner ? undefined : handleLike}
              disabled={isOwner}
              title={isOwner ? "Can't like your own pet" : undefined}
              className="flex items-center gap-2 px-4 h-[38px] rounded-[20px] text-sm font-medium transition-colors shrink-0"
              style={{
                border: `1.5px solid ${liked ? '#e5484d' : 'var(--border)'}`,
                background: liked ? 'rgba(229,72,77,0.08)' : 'transparent',
                color: liked ? '#e5484d' : 'var(--muted-foreground)',
                opacity: isOwner ? 0.45 : 1,
                cursor: isOwner ? 'not-allowed' : 'pointer',
              }}
            >
              <Heart className={`w-4 h-4 ${liked ? 'fill-current' : ''}`} />
              {displayLikes}
            </button>
          </div>
        </div>
      </div>

      {/* Photo grid */}
      {pet.images.length > 0 && (
        <div className="max-w-2xl mx-auto px-4 mt-6 pb-20">
          <h2 className="font-heading font-semibold text-lg mb-4">Photos</h2>
          <div className="grid grid-cols-3 gap-[10px]">
            {pet.images.map((img, idx) => (
              <button
                key={img.petImageId}
                onClick={() => setLightboxIndex(idx)}
                className="aspect-square rounded-[10px] overflow-hidden group"
              >
                <img
                  src={getBlobImageUrl(img.blobName)}
                  alt={`${pet.name} photo ${idx + 1}`}
                  className="w-full h-full object-cover group-hover:scale-[1.06] transition-transform duration-300"
                />
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Lightbox */}
      {lightboxIndex !== null && (
        <div
          className="fixed inset-0 z-[100] flex items-center justify-center animate-backdrop-in"
          style={{ background: 'rgba(0,0,0,0.92)' }}
          onClick={() => setLightboxIndex(null)}
        >
          <button
            className="absolute top-4 right-4 w-10 h-10 rounded-full bg-white/10 flex items-center justify-center text-white hover:bg-white/20 transition-colors"
            onClick={() => setLightboxIndex(null)}
          >
            <X className="w-5 h-5" />
          </button>

          <button
            className="absolute left-4 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/10 flex items-center justify-center text-white hover:bg-white/20 transition-colors"
            onClick={e => { e.stopPropagation(); handleLightboxNav(-1) }}
          >
            <ChevronLeft className="w-5 h-5" />
          </button>

          <img
            src={getBlobImageUrl(pet.images[lightboxIndex].blobName)}
            alt={`${pet.name} photo ${lightboxIndex + 1}`}
            className="max-w-[90vw] max-h-[90vh] object-contain rounded-2xl"
            onClick={e => e.stopPropagation()}
          />

          <button
            className="absolute right-4 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/10 flex items-center justify-center text-white hover:bg-white/20 transition-colors"
            onClick={e => { e.stopPropagation(); handleLightboxNav(1) }}
          >
            <ChevronRight className="w-5 h-5" />
          </button>

          <span className="absolute bottom-4 left-1/2 -translate-x-1/2 text-white/60 text-sm">
            {lightboxIndex + 1} / {pet.images.length}
          </span>
        </div>
      )}

      {/* Delete confirmation modal */}
      {deleteOpen && (
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
            <h3 className="font-heading font-bold text-lg mb-2">Delete pet?</h3>
            <p className="text-sm mb-1" style={{ color: 'var(--muted-foreground)' }}>
              You're about to permanently delete <strong>{pet.name}</strong>.
            </p>
            <p className="text-sm mb-6" style={{ color: 'var(--muted-foreground)' }}>
              This will remove all photos and cannot be undone.
            </p>
            <div className="flex gap-3">
              <button
                onClick={() => setDeleteOpen(false)}
                className="flex-1 h-11 rounded-xl text-sm font-medium transition-colors hover:bg-secondary"
                style={{
                  border: '1.5px solid var(--border)',
                  background: 'transparent',
                  color: 'var(--muted-foreground)',
                }}
              >
                Cancel
              </button>
              <button
                onClick={handleDelete}
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
      )}
    </div>
  )
}
