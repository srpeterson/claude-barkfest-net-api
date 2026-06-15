import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, ChevronLeft, ChevronRight, Heart, Loader2, MoreVertical, Pencil, Trash2, UserCircle, X } from 'lucide-react'
import { cn } from '@/lib/utils'
import { deletePet, getOwnerById, getPetDetail, likePet, unlikePet } from '@/lib/api'
import type { PetDto } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { invalidateBrowse } from '@/lib/browseCache'
import { formatAge } from '@/lib/formatAge'
import { useAuth } from '@/hooks/useAuth'
import { Navbar } from '@/components/Navbar'
import { Footer } from '@/components/Footer'
import { EditPetModal } from '@/components/EditPetModal'

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', year: 'numeric' })
}

export function PetDetailPage() {
  const { petId } = useParams<{ petId: string }>()
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { accountId, isAuthenticated, accountType } = useAuth()

  const [active, setActive]               = useState(0)
  const [liked, setLiked]                 = useState(false)
  const [likeCount, setLikeCount]         = useState<number | null>(null)
  const [kebabOpen, setKebabOpen]         = useState(false)
  const [deleteOpen, setDeleteOpen]       = useState(false)
  const [editOpen, setEditOpen]           = useState(false)
  const [lightboxIndex, setLightboxIndex] = useState<number | null>(null)
  const [isDeleting, setIsDeleting]       = useState(false)

  const { data: pet, isLoading, isError } = useQuery({
    queryKey: ['pet', petId],
    queryFn: () => getPetDetail(petId!),
    enabled: !!petId,
    retry: false,
  })

  useEffect(() => {
    if (isError) {
      invalidateBrowse(queryClient)
    }
  }, [isError, queryClient])

  const { data: owner } = useQuery({
    queryKey: ['owner', pet?.ownerId],
    queryFn: () => getOwnerById(pet!.ownerId),
    enabled: !!pet?.ownerId && isAuthenticated,
  })

  const isOwner      = isAuthenticated && accountType === 'owner' && accountId === pet?.ownerId
  const multi        = (pet?.images.length ?? 0) > 1
  const displayLikes = likeCount !== null ? likeCount : (pet?.likes ?? 0)
  const fromManage   = searchParams.get('from') === 'manage'

  // Keyboard nav for lightbox
  useEffect(() => {
    if (lightboxIndex === null || !pet) return
    const fn = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setLightboxIndex(null)
      if (e.key === 'ArrowLeft') setLightboxIndex(i => i === null ? null : (i - 1 + pet.images.length) % pet.images.length)
      if (e.key === 'ArrowRight') setLightboxIndex(i => i === null ? null : (i + 1) % pet.images.length)
    }
    window.addEventListener('keydown', fn)
    return () => window.removeEventListener('keydown', fn)
  }, [lightboxIndex, pet])

  // Arrow key carousel navigation — only when lightbox is closed
  useEffect(() => {
    if (!pet || !multi) return
    const fn = (e: KeyboardEvent) => {
      if (lightboxIndex !== null) return
      if (e.key === 'ArrowLeft') setActive(i => (i - 1 + pet.images.length) % pet.images.length)
      if (e.key === 'ArrowRight') setActive(i => (i + 1) % pet.images.length)
    }
    window.addEventListener('keydown', fn)
    return () => window.removeEventListener('keydown', fn)
  }, [pet, multi, lightboxIndex])

  async function handleLike() {
    if (!pet || isOwner || !isAuthenticated) return
    const next = !liked
    const prev = likeCount !== null ? likeCount : pet.likes
    const newCount = next ? prev + 1 : Math.max(0, prev - 1)
    setLiked(next)
    setLikeCount(newCount)
    try {
      if (next) await likePet(pet.petId)
      else await unlikePet(pet.petId)
      queryClient.setQueryData<PetDto>(['pet', petId], old =>
        old ? { ...old, likes: newCount } : old
      )
      invalidateBrowse(queryClient)
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
      invalidateBrowse(queryClient)
      queryClient.invalidateQueries({ queryKey: ['owner', 'pets', accountId] })
      navigate('/')
    } catch {
      setIsDeleting(false)
    }
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background">
        <Navbar maxWidth="max-w-[860px]" />
        <div className="flex items-center justify-center pt-32">
          <Loader2 className="w-8 h-8 animate-spin text-primary" />
        </div>
      </div>
    )
  }

  if (isError || !pet) {
    return (
      <div className="min-h-screen bg-background">
        <Navbar maxWidth="max-w-[860px]" />
        <div className="flex flex-col items-center justify-center pt-32 gap-4">
          <p className="text-muted-foreground">Pet not found.</p>
          <Link to="/" className="text-sm font-medium text-primary hover:underline">← Back to Barkfest</Link>
        </div>
      </div>
    )
  }

  const age          = formatAge(pet.dateOfBirth, 'long')
  // Featured image sorted to front so it always appears first in the rail
  const sortedImages = (() => {
    const idx = pet.images.findIndex(i => i.isFeaturedImage)
    return idx > 0
      ? [pet.images[idx], ...pet.images.filter((_, i) => i !== idx)]
      : pet.images
  })()
  const safeActive   = Math.min(active, sortedImages.length - 1)

  return (
    <div className="min-h-screen bg-background flex flex-col">
      <Navbar />

      {/* Back nav */}
      <div className="max-w-[860px] mx-auto px-6 pt-6 pb-4 w-full">
        <button
          onClick={() => fromManage ? navigate('/manage') : navigate(-1)}
          className="inline-flex items-center gap-1.5 text-[13px] text-muted-foreground bg-transparent border-0 cursor-pointer p-0 transition-colors hover:text-foreground"
        >
          <ArrowLeft className="w-4 h-4" />
          {fromManage ? 'My Pets' : 'Back to Barkfest'}
        </button>
      </div>

      {/* ── Stage ── */}
      <div className="max-w-[860px] mx-auto px-6 w-full">
        <div
          className="relative overflow-hidden rounded-[18px] cursor-zoom-in"
          style={{ height: 'clamp(380px, 56vw, 600px)', background: 'var(--primary)' }}
          onClick={() => setLightboxIndex(safeActive)}
        >
          {/* Inner frame — clips blurred fill so orange border stays clean */}
          <div
            className="absolute overflow-hidden rounded-xl"
            style={{ top: 10, left: 10, right: 10, bottom: 10 }}
          >
            {/* Blurred ambient fill — Ken Burns drift */}
            <img
              src={getBlobImageUrl(sortedImages[safeActive].blobName)}
              alt=""
              aria-hidden="true"
              className="absolute inset-0 w-full h-full object-cover kenburns"
              style={{ filter: 'blur(30px) saturate(1.15) brightness(0.62)', transform: 'scale(1.18)' }}
            />

            {/* Dark overlay */}
            <div className="absolute inset-0" style={{ background: 'rgba(8,6,5,0.5)' }} />

            {/* Framed photo */}
            <img
              key={safeActive}
              src={getBlobImageUrl(sortedImages[safeActive].blobName)}
              alt={pet.name}
              className="absolute inset-0 w-full h-full object-contain"
              style={{ zIndex: 1, animation: 'fade-in 0.45s ease' }}
            />

            {/* Pet name — bottom-left overlay */}
            <div className="absolute bottom-3 left-3" style={{ zIndex: 2 }}>
              <h1
                className="font-heading font-bold text-white leading-tight drop-shadow-[0_2px_8px_rgba(0,0,0,0.5)]"
                style={{ fontSize: 'clamp(22px, 3vw, 32px)' }}
              >
                {pet.name}
              </h1>
            </div>

            {/* Age + breed chips + like — bottom-right overlay */}
            <div className="absolute bottom-3 right-3 flex gap-1.5 flex-wrap justify-end items-center" style={{ zIndex: 2 }}>
              {age && (
                <span className="inline-flex items-center h-[26px] px-3 rounded-full text-xs font-medium bg-white/20 text-white border border-white/30 backdrop-blur-sm">
                  {age}
                </span>
              )}
              {pet.breed && (
                <span className="inline-flex items-center h-[26px] px-3 rounded-full text-xs font-medium bg-white/20 text-white border border-white/30 backdrop-blur-sm">
                  {pet.breed}
                </span>
              )}
              {isOwner || !isAuthenticated ? (
                <span className="inline-flex items-center gap-1.5 h-[26px] px-3 rounded-full text-xs font-medium bg-white/20 text-white border border-white/30 backdrop-blur-sm">
                  <Heart className="w-[11px] h-[11px]" fill="#e5484d" stroke="#e5484d" />
                  {displayLikes}
                </span>
              ) : (
                <button
                  onClick={e => { e.stopPropagation(); handleLike() }}
                  className="inline-flex items-center gap-1.5 h-[26px] px-3 rounded-full text-xs font-medium bg-white/20 text-white border border-white/30 backdrop-blur-sm cursor-pointer hover:bg-white/30 transition-colors"
                >
                  <Heart
                    className="w-[11px] h-[11px] transition-transform active:scale-125"
                    fill={liked ? '#e5484d' : 'none'}
                    stroke={liked ? '#e5484d' : 'white'}
                  />
                  {displayLikes}
                </button>
              )}
            </div>
          </div>

          {/* Owner kebab — above carousel chrome */}
          {isOwner && (
            <div
              className="absolute top-6 right-6 z-10"
              onClick={e => e.stopPropagation()}
            >
              <div className="relative">
                {kebabOpen && (
                  <div onClick={() => setKebabOpen(false)} className="fixed inset-0 z-[90]" />
                )}
                <button
                  onClick={() => setKebabOpen(o => !o)}
                  aria-label="Pet options"
                  className="w-[38px] h-[38px] rounded-full border-0 cursor-pointer bg-black/35 backdrop-blur-[6px] text-white flex items-center justify-center hover:bg-black/50 transition-colors"
                >
                  <MoreVertical className="w-5 h-5" />
                </button>

                {kebabOpen && (
                  <div className="absolute top-[calc(100%+8px)] right-0 w-40 bg-card border border-border rounded-xl shadow-[0_8px_32px_rgba(0,0,0,0.12)] overflow-hidden z-[100]">
                    <KebabItem
                      icon={<Pencil className="w-3.5 h-3.5" />}
                      label="Edit pet"
                      className="text-foreground"
                      onClick={() => { setKebabOpen(false); setEditOpen(true) }}
                    />
                    <KebabItem
                      icon={<Trash2 className="w-3.5 h-3.5" />}
                      label="Delete pet"
                      className="text-destructive"
                      onClick={() => { setKebabOpen(false); setDeleteOpen(true) }}
                    />
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Carousel chrome — only when multi */}
          {multi && (
            <>
              <button
                onClick={e => { e.stopPropagation(); setActive(i => (i - 1 + pet.images.length) % pet.images.length) }}
                aria-label="Previous photo"
                className="absolute left-3 top-1/2 -translate-y-1/2 w-11 h-11 rounded-full bg-black/40 backdrop-blur-sm border-0 cursor-pointer text-white flex items-center justify-center hover:bg-black/60 transition-colors"
                style={{ zIndex: 2 }}
              >
                <ChevronLeft className="w-5 h-5" />
              </button>

              <button
                onClick={e => { e.stopPropagation(); setActive(i => (i + 1) % pet.images.length) }}
                aria-label="Next photo"
                className="absolute right-3 top-1/2 -translate-y-1/2 w-11 h-11 rounded-full bg-black/40 backdrop-blur-sm border-0 cursor-pointer text-white flex items-center justify-center hover:bg-black/60 transition-colors"
                style={{ zIndex: 2 }}
              >
                <ChevronRight className="w-5 h-5" />
              </button>

              <div
                className="absolute top-5 left-1/2 -translate-x-1/2 flex gap-1.5"
                style={{ zIndex: 2 }}
                onClick={e => e.stopPropagation()}
              >
                {sortedImages.map((_, i) => (
                  <button
                    key={i}
                    onClick={() => setActive(i)}
                    aria-label={`Photo ${i + 1}`}
                    className="h-[7px] rounded-full border-0 cursor-pointer p-0 transition-all"
                    style={{
                      width: i === safeActive ? 20 : 7,
                      background: i === safeActive ? '#fff' : 'rgba(255,255,255,0.55)',
                    }}
                  />
                ))}
              </div>
            </>
          )}
        </div>
      </div>

      {/* ── Thumbnail rail ── */}
      {multi && (
        <div className="max-w-[860px] mx-auto px-6 mt-5 w-full">
          <div className="flex gap-2 overflow-x-auto p-1">
            {sortedImages.map((img, i) => (
              <button
                key={img.petImageId}
                onClick={() => setActive(i)}
                aria-label={`View photo ${i + 1}`}
                className="shrink-0 w-[60px] h-[60px] rounded-[10px] border-0 cursor-pointer p-0"
                style={{
                  outline: i === safeActive ? '2.5px solid var(--primary)' : '2.5px solid transparent',
                  outlineOffset: 1,
                }}
              >
                <div className="w-full h-full rounded-[10px] overflow-hidden">
                  <img
                    src={getBlobImageUrl(img.blobName)}
                    alt=""
                    className="w-full h-full object-cover block transition-opacity"
                    style={{ opacity: i === safeActive ? 1 : 0.6 }}
                  />
                </div>
              </button>
            ))}
          </div>
        </div>
      )}

      {/* ── Description + owner ── */}
      <div className="max-w-[860px] mx-auto px-6 mt-3 pb-14 w-full flex flex-col gap-2">
        {pet.description && (
          <p className="m-0 text-[15px] text-muted-foreground italic leading-relaxed">
            {pet.description}
          </p>
        )}
        <div className="flex items-center gap-2">
          {owner?.profileImage?.blobName ? (
            <img
              src={getBlobImageUrl(owner.profileImage.blobName, 'owner-profile-images')}
              alt={owner.displayName ?? owner.username}
              className="w-[22px] h-[22px] rounded-full object-cover border border-border shrink-0"
            />
          ) : (
            <div className="w-[22px] h-[22px] rounded-full bg-secondary flex items-center justify-center shrink-0">
              <UserCircle className="w-3.5 h-3.5 text-muted-foreground" />
            </div>
          )}
          <p className="m-0 text-[12px] text-muted-foreground">
            {owner?.displayName && (
              <span className="font-medium text-foreground">{owner.displayName}</span>
            )}
            <span className="mx-1 opacity-40">·</span>
            {formatDate(pet.createdAt)}
          </p>
        </div>
      </div>

      <Footer />

      {/* ── Lightbox ── */}
      {lightboxIndex !== null && (
        <div
          onClick={() => setLightboxIndex(null)}
          className="fixed inset-0 z-[500] bg-black/[0.92] flex items-center justify-center p-6"
          style={{ animation: 'fade-in 0.2s ease' }}
        >
          <button
            onClick={() => setLightboxIndex(null)}
            aria-label="Close"
            className="absolute top-5 right-5 w-10 h-10 rounded-full bg-white/10 border-0 cursor-pointer text-white flex items-center justify-center hover:bg-white/20 transition-colors"
          >
            <X className="w-5 h-5" />
          </button>

          <div
            onClick={e => e.stopPropagation()}
            className="relative max-w-[900px] max-h-[85vh] w-full"
          >
            <img
              src={getBlobImageUrl(sortedImages[lightboxIndex].blobName)}
              alt={`${pet.name} photo ${lightboxIndex + 1}`}
              className="w-full max-h-[80vh] object-contain rounded-xl block"
            />
            <div className="absolute -bottom-9 left-1/2 -translate-x-1/2 text-[13px] text-white/60 whitespace-nowrap">
              {lightboxIndex + 1} / {sortedImages.length}
            </div>
          </div>

          {sortedImages.length > 1 && (
            <>
              <button
                onClick={e => { e.stopPropagation(); setLightboxIndex(i => i === null ? 0 : (i - 1 + sortedImages.length) % sortedImages.length) }}
                aria-label="Previous photo"
                className="absolute left-4 top-1/2 -translate-y-1/2 w-11 h-11 rounded-full bg-white/[0.12] border-0 cursor-pointer text-white flex items-center justify-center hover:bg-white/20 transition-colors"
              >
                <ChevronLeft className="w-5 h-5" />
              </button>
              <button
                onClick={e => { e.stopPropagation(); setLightboxIndex(i => i === null ? 0 : (i + 1) % sortedImages.length) }}
                aria-label="Next photo"
                className="absolute right-4 top-1/2 -translate-y-1/2 w-11 h-11 rounded-full bg-white/[0.12] border-0 cursor-pointer text-white flex items-center justify-center hover:bg-white/20 transition-colors"
              >
                <ChevronRight className="w-5 h-5" />
              </button>
            </>
          )}
        </div>
      )}

      {editOpen && pet && (
        <EditPetModal
          pet={pet}
          onClose={() => setEditOpen(false)}
          onSuccess={() => {
            queryClient.invalidateQueries({ queryKey: ['pet', petId] })
            invalidateBrowse(queryClient)
            setEditOpen(false)
          }}
        />
      )}

      {/* ── Delete confirmation ── */}
      {deleteOpen && (
        <div className="fixed inset-0 z-[500] bg-black/45 backdrop-blur-sm flex items-center justify-center p-6">
          <div className="bg-card rounded-[20px] p-7 max-w-[360px] w-full shadow-[0_24px_64px_rgba(0,0,0,0.15)]">
            <h3 className="font-heading text-xl font-bold mb-2.5">Delete {pet.name}?</h3>
            <p className="text-sm text-muted-foreground leading-relaxed mb-[22px]">
              This will permanently remove {pet.name} and all their photos. This can't be undone.
            </p>
            <div className="flex gap-2.5">
              <button
                onClick={() => setDeleteOpen(false)}
                className="flex-1 h-[42px] rounded-[10px] border-[1.5px] border-border bg-transparent text-foreground text-sm font-medium cursor-pointer"
              >
                Cancel
              </button>
              <button
                onClick={handleDelete}
                disabled={isDeleting}
                className="flex-1 h-[42px] rounded-[10px] border-0 bg-destructive text-white text-sm font-semibold cursor-pointer flex items-center justify-center gap-2 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isDeleting && <Loader2 className="w-3.5 h-3.5 animate-spin" />}
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

// ── Small components ──────────────────────────────────────────────────────────


function KebabItem({
  icon, label, className, onClick,
}: {
  icon: React.ReactNode
  label: string
  className?: string
  onClick: () => void
}) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'w-full h-10 px-3.5 flex items-center gap-[9px] bg-transparent border-0 cursor-pointer text-[13px] font-medium text-left hover:bg-secondary transition-colors',
        className
      )}
    >
      {icon}{label}
    </button>
  )
}
