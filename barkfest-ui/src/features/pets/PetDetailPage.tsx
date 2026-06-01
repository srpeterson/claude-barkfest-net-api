import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, ChevronLeft, ChevronRight, Heart, Loader2, MoreVertical, Pencil, Trash2, UserCircle, X } from 'lucide-react'
import { cn } from '@/lib/utils'
import { deletePet, getOwnerById, getPetDetail, likePet, unlikePet } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { formatAge } from '@/lib/formatAge'
import { useAuth } from '@/hooks/useAuth'
import { Navbar } from '@/components/Navbar'
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

  const [liked, setLiked]                   = useState(false)
  const [likeCount, setLikeCount]           = useState<number | null>(null)
  const [kebabOpen, setKebabOpen]           = useState(false)
  const [deleteOpen, setDeleteOpen]         = useState(false)
  const [editOpen, setEditOpen]             = useState(false)
  const [lightboxIndex, setLightboxIndex]   = useState<number | null>(null)
  const [isDeleting, setIsDeleting]         = useState(false)

  const { data: pet, isLoading, isError } = useQuery({
    queryKey: ['pet', petId],
    queryFn: () => getPetDetail(petId!),
    enabled: !!petId,
  })

  const { data: owner } = useQuery({
    queryKey: ['owner', pet?.ownerId],
    queryFn: () => getOwnerById(pet!.ownerId),
    enabled: !!pet?.ownerId && isAuthenticated,
  })

  const isOwner    = isAuthenticated && accountType === 'owner' && accountId === pet?.ownerId
  const displayLikes = likeCount !== null ? likeCount : (pet?.likes ?? 0)
  const fromManage = searchParams.get('from') === 'manage'

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

  async function handleLike() {
    if (!pet || isOwner) return
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

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background">
        <Navbar />
        <div className="flex items-center justify-center pt-32">
          <Loader2 className="w-8 h-8 animate-spin text-primary" />
        </div>
      </div>
    )
  }

  if (isError || !pet) {
    return (
      <div className="min-h-screen bg-background">
        <Navbar />
        <div className="flex flex-col items-center justify-center pt-32 gap-4">
          <p className="text-muted-foreground">Pet not found.</p>
          <Link to="/" className="text-sm font-medium text-primary hover:underline">← Back to Barkfest</Link>
        </div>
      </div>
    )
  }

  const heroImage = pet.images.find(i => i.isFeaturedImage) ?? pet.images[0]
  const age = formatAge(pet.dateOfBirth, 'long')

  return (
    <div className="min-h-screen bg-background">
      <Navbar />

      {/* Back nav */}
      <div className="max-w-[860px] mx-auto px-6 pt-6 pb-2">
        <button
          onClick={() => fromManage ? navigate('/manage') : navigate(-1)}
          className="inline-flex items-center gap-1.5 text-[13px] text-muted-foreground bg-transparent border-0 cursor-pointer p-0 transition-colors hover:text-foreground"
        >
          <ArrowLeft className="w-4 h-4" />
          {fromManage ? 'My Pets' : 'Back to Barkfest'}
        </button>
      </div>

      {/* ── Hero ── */}
      <div className="relative" style={{ height: 'clamp(320px, 45vw, 520px)' }}>

        {/* Image + gradient */}
        <div className="absolute inset-0 overflow-hidden">
          {heroImage ? (
            <img
              src={getBlobImageUrl(heroImage.blobName)}
              alt={pet.name}
              className="w-full h-full object-cover block"
            />
          ) : (
            <div className="w-full h-full bg-secondary flex items-center justify-center">
              <span className="text-[64px]">🐾</span>
            </div>
          )}
          <div className="absolute inset-0 bg-gradient-to-t from-black/75 via-black/10 to-transparent" />
        </div>

        {/* Name + badges */}
        <div className="absolute bottom-0 left-0 right-0 px-8 pb-16 flex items-end justify-between">
          <div>
            <h1
              className="font-heading font-bold text-white leading-[1.1] mb-3 drop-shadow-[0_2px_8px_rgba(0,0,0,0.3)]"
              style={{ fontSize: 'clamp(32px, 4vw, 56px)' }}
            >
              {pet.name}
            </h1>
            <div className="flex gap-1.5 flex-wrap">
              {age && <DarkChip>{age}</DarkChip>}
              {pet.breed && <DarkChip>{pet.breed}</DarkChip>}
            </div>
          </div>
        </div>

        {/* Owner kebab */}
        {isOwner && (
          <div className="absolute top-4 right-4 z-10">
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
      </div>

      {/* ── Floating info card ── */}
      <div className="max-w-[860px] mx-auto px-6">
        <div className="bg-card border border-border rounded-[20px] px-8 py-7 -mt-10 relative shadow-[0_4px_24px_rgba(0,0,0,0.08)]">

          {pet.description && (
            <p className="text-[15px] text-muted-foreground leading-[1.75] mb-5">{pet.description}</p>
          )}

          <div className="h-px bg-border mb-5" />

          {/* Owner row + like button */}
          <div className="flex items-center justify-between flex-wrap gap-3">

            {/* Owner */}
            <div className="flex items-center gap-2.5 min-w-0">
              {owner?.profileImage?.blobName ? (
                <img
                  src={getBlobImageUrl(owner.profileImage.blobName, 'owner-profile-images')}
                  alt={owner.displayName ?? owner.username}
                  className="w-[34px] h-[34px] rounded-full object-cover border-2 border-border shrink-0"
                />
              ) : (
                <div className="w-[34px] h-[34px] rounded-full bg-secondary flex items-center justify-center shrink-0">
                  <UserCircle className="w-5 h-5 text-muted-foreground" />
                </div>
              )}
              <div className="min-w-0">
                {owner?.displayName && (
                  <p className="m-0 text-[13px] font-semibold text-foreground leading-[1.2]">
                    {owner.displayName}
                  </p>
                )}
                <p className="m-0 text-[11px] text-muted-foreground">
                  {owner ? `@${owner.username} · ` : ''}{formatDate(pet.createdAt)}
                </p>
              </div>
            </div>

            {/* Like button */}
            <div className="flex flex-col items-end gap-1">
              <button
                onClick={handleLike}
                disabled={isOwner}
                title={isOwner ? "You can't like your own pet" : undefined}
                className={cn(
                  'inline-flex items-center gap-1.5 h-[38px] px-4 rounded-full border-[1.5px] text-sm cursor-pointer transition-all',
                  liked
                    ? 'border-[#e5484d] bg-[#e5484d]/[0.08] text-[#e5484d] font-semibold'
                    : 'border-border bg-transparent text-muted-foreground font-normal',
                  isOwner && 'border-border text-border cursor-not-allowed opacity-45'
                )}
              >
                <Heart
                  className="w-4 h-4"
                  fill={liked ? '#e5484d' : 'none'}
                  stroke={liked ? '#e5484d' : 'currentColor'}
                />
                {displayLikes}
              </button>
              {isOwner && (
                <p className="text-[11px] text-muted-foreground m-0">Can't like your own pet</p>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* ── Photos ── */}
      {pet.images.length > 0 && (
        <div className="max-w-[860px] mx-auto mt-10 px-6 pb-14">
          <div className="flex items-baseline gap-2 mb-3.5">
            <h2 className="font-heading text-xl font-bold m-0">Photos</h2>
            <span className="text-[13px] text-muted-foreground">{pet.images.length}</span>
          </div>

          <div className="grid grid-cols-3 gap-2.5">
            {pet.images.map((img, idx) => (
              <button
                key={img.petImageId}
                onClick={() => setLightboxIndex(idx)}
                className="aspect-square rounded-[10px] overflow-hidden cursor-pointer border-0 p-0 relative block group"
              >
                <img
                  src={getBlobImageUrl(img.blobName)}
                  alt={`${pet.name} photo ${idx + 1}`}
                  className="w-full h-full object-cover block transition-transform duration-500 group-hover:scale-[1.06]"
                />
              </button>
            ))}
          </div>
        </div>
      )}

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
              src={getBlobImageUrl(pet.images[lightboxIndex].blobName)}
              alt={`${pet.name} photo ${lightboxIndex + 1}`}
              className="w-full max-h-[80vh] object-contain rounded-xl block"
            />
            <div className="absolute -bottom-9 left-1/2 -translate-x-1/2 text-[13px] text-white/60 whitespace-nowrap">
              {lightboxIndex + 1} / {pet.images.length}
            </div>
          </div>

          {pet.images.length > 1 && (
            <>
              <button
                onClick={e => { e.stopPropagation(); setLightboxIndex(i => i === null ? 0 : (i - 1 + pet.images.length) % pet.images.length) }}
                aria-label="Previous photo"
                className="absolute left-4 top-1/2 -translate-y-1/2 w-11 h-11 rounded-full bg-white/[0.12] border-0 cursor-pointer text-white flex items-center justify-center hover:bg-white/20 transition-colors"
              >
                <ChevronLeft className="w-5 h-5" />
              </button>
              <button
                onClick={e => { e.stopPropagation(); setLightboxIndex(i => i === null ? 0 : (i + 1) % pet.images.length) }}
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
            queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
            queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
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

      <style>{`@keyframes fade-in { from { opacity:0 } to { opacity:1 } }`}</style>
    </div>
  )
}

function DarkChip({ children }: { children: React.ReactNode }) {
  return (
    <span className="inline-flex items-center h-[26px] px-3 rounded-full text-xs font-medium bg-white/20 text-white border border-white/30">
      {children}
    </span>
  )
}

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
