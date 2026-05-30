import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, ChevronLeft, ChevronRight, Heart, Loader2, MoreHorizontal, Pencil, Trash2, UserCircle, X } from 'lucide-react'
import { deletePet, getOwnerById, getPetDetail, likePet, unlikePet } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'
import { useAuth } from '@/hooks/useAuth'
import { Navbar } from '@/components/Navbar'
import { EditPetModal } from '@/components/EditPetModal'

// ── Helpers ───────────────────────────────────────────────────────────

function formatAge(dateOfBirth?: string): string | null {
  if (!dateOfBirth) return null
  const dob = new Date(dateOfBirth)
  const today = new Date()
  let months = (today.getFullYear() - dob.getFullYear()) * 12 + (today.getMonth() - dob.getMonth())
  if (today.getDate() < dob.getDate()) months--
  months = Math.max(months, 0)
  if (months < 12) return months === 1 ? '1 month old' : `${months} months old`
  const years = Math.floor(months / 12)
  return years === 1 ? '1 year old' : `${years} years old`
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', year: 'numeric' })
}

// ── PetDetailPage ─────────────────────────────────────────────────────

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
  const [editOpen, setEditOpen] = useState(false)
  const [lightboxIndex, setLightboxIndex] = useState<number | null>(null)
  const [isDeleting, setIsDeleting] = useState(false)

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

  const isOwner = isAuthenticated && accountType === 'owner' && accountId === pet?.ownerId
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

  // ── Loading / error states ────────────────────────────────────────
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
  const age = formatAge(pet.dateOfBirth)

  return (
    <div className="min-h-screen bg-background">
      <Navbar />

      {/* Back nav */}
      <div className="max-w-[860px] mx-auto px-6 pt-6 pb-2">
        <Link
          to={fromManage ? '/manage' : '/'}
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: 6,
            fontSize: 13,
            color: 'var(--muted-foreground)',
            textDecoration: 'none',
            transition: 'color 0.15s',
          }}
          onMouseEnter={e => (e.currentTarget.style.color = 'var(--foreground)')}
          onMouseLeave={e => (e.currentTarget.style.color = 'var(--muted-foreground)')}
        >
          <ArrowLeft className="w-4 h-4" />
          {fromManage ? 'My Pets' : 'Back to Barkfest'}
        </Link>
      </div>

      {/* ── Hero — full-bleed, overflow visible so kebab dropdown can escape ── */}
      <div style={{ position: 'relative', height: 'clamp(320px, 45vw, 520px)' }}>

        {/* Image + gradient — clipped independently */}
        <div style={{ position: 'absolute', inset: 0, overflow: 'hidden' }}>
          {heroImage ? (
            <img
              src={getBlobImageUrl(heroImage.blobName)}
              alt={pet.name}
              style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }}
            />
          ) : (
            <div style={{ width: '100%', height: '100%', background: 'var(--secondary)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <span style={{ fontSize: 64 }}>🐾</span>
            </div>
          )}
          <div style={{ position: 'absolute', inset: 0, background: 'linear-gradient(to top, rgba(0,0,0,0.75) 0%, rgba(0,0,0,0.1) 50%, transparent 100%)' }} />
        </div>

        {/* Name + badges — bottom-left */}
        <div
          style={{
            position: 'absolute',
            bottom: 0,
            left: 0,
            right: 0,
            padding: '0 32px 64px',
            display: 'flex',
            alignItems: 'flex-end',
            justifyContent: 'space-between',
          }}
        >
          <div>
            <h1
              className="font-heading"
              style={{
                fontSize: 'clamp(32px, 4vw, 56px)',
                fontWeight: 700,
                color: '#fff',
                lineHeight: 1.1,
                marginBottom: 12,
                textShadow: '0 2px 8px rgba(0,0,0,0.3)',
              }}
            >
              {pet.name}
            </h1>
            {/* Age + breed badges */}
            <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
              {age && <DarkChip>{age}</DarkChip>}
              {pet.breed && <DarkChip>{pet.breed}</DarkChip>}
            </div>
          </div>
        </div>

        {/* Owner kebab — top-right, z-index above hero but not clipped */}
        {isOwner && (
          <div style={{ position: 'absolute', top: 16, right: 16, zIndex: 10 }}>
            <div style={{ position: 'relative' }}>
              {kebabOpen && (
                <div onClick={() => setKebabOpen(false)} style={{ position: 'fixed', inset: 0, zIndex: 90 }} />
              )}
              <button
                onClick={() => setKebabOpen(o => !o)}
                style={{
                  width: 38, height: 38, borderRadius: '50%',
                  border: 'none', cursor: 'pointer',
                  background: 'rgba(0,0,0,0.35)',
                  backdropFilter: 'blur(6px)',
                  color: '#fff',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                }}
              >
                <MoreHorizontal className="w-5 h-5" />
              </button>

              {kebabOpen && (
                <div
                  style={{
                    position: 'absolute', top: 'calc(100% + 8px)', right: 0,
                    width: 160,
                    background: 'var(--card)',
                    border: '1px solid var(--border)',
                    borderRadius: 12,
                    boxShadow: '0 8px 32px rgba(0,0,0,0.12)',
                    overflow: 'hidden',
                    zIndex: 100,
                  }}
                >
                  <KebabItem
                    icon={<Pencil className="w-3.5 h-3.5" />}
                    label="Edit pet"
                    color="var(--foreground)"
                    onClick={() => { setKebabOpen(false); setEditOpen(true) }}
                  />
                  <KebabItem
                    icon={<Trash2 className="w-3.5 h-3.5" />}
                    label="Delete pet"
                    color="var(--destructive)"
                    onClick={() => { setKebabOpen(false); setDeleteOpen(true) }}
                  />
                </div>
              )}
            </div>
          </div>
        )}
      </div>

      {/* ── Floating info card ─────────────────────────────────────── */}
      <div style={{ maxWidth: 860, margin: '0 auto', padding: '0 24px' }}>
        <div
          style={{
            background: 'var(--card)',
            border: '1px solid var(--border)',
            borderRadius: 20,
            padding: '28px 32px',
            marginTop: -40,
            position: 'relative',
            boxShadow: '0 4px 24px rgba(0,0,0,0.08)',
          }}
        >
          {/* Description */}
          {pet.description && (
            <p style={{ fontSize: 15, color: 'var(--muted-foreground)', lineHeight: 1.75, marginBottom: 20 }}>
              {pet.description}
            </p>
          )}

          <div style={{ height: 1, background: 'var(--border)', marginBottom: 20 }} />

          {/* Owner row + like button */}
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 12 }}>

            {/* Owner */}
            <div style={{ display: 'flex', alignItems: 'center', gap: 10, minWidth: 0 }}>
              {owner?.profileImage?.blobName ? (
                <img
                  src={getBlobImageUrl(owner.profileImage.blobName, 'owner-profile-images')}
                  alt={owner.displayName ?? owner.username}
                  style={{ width: 34, height: 34, borderRadius: '50%', objectFit: 'cover', border: '2px solid var(--border)', flexShrink: 0 }}
                />
              ) : (
                <div style={{ width: 34, height: 34, borderRadius: '50%', background: 'var(--secondary)', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
                  <UserCircle className="w-5 h-5 text-muted-foreground" />
                </div>
              )}
              <div style={{ minWidth: 0 }}>
                {owner?.displayName && (
                  <p style={{ margin: 0, fontSize: 13, fontWeight: 600, color: 'var(--foreground)', lineHeight: 1.2 }}>
                    {owner.displayName}
                  </p>
                )}
                <p style={{ margin: 0, fontSize: 11, color: 'var(--muted-foreground)' }}>
                  {owner ? `@${owner.username} · ` : ''}{formatDate(pet.createdAt)}
                </p>
              </div>
            </div>

            {/* Like button */}
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: 4 }}>
              <button
                onClick={handleLike}
                disabled={isOwner}
                title={isOwner ? "You can't like your own pet" : undefined}
                style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: 7,
                  height: 38,
                  padding: '0 16px',
                  borderRadius: 20,
                  border: `1.5px solid ${liked ? '#e5484d' : 'var(--border)'}`,
                  background: liked ? 'rgba(229,72,77,0.08)' : 'transparent',
                  color: isOwner ? 'var(--border)' : liked ? '#e5484d' : 'var(--muted-foreground)',
                  fontFamily: "'DM Sans', sans-serif",
                  fontSize: 14,
                  fontWeight: liked ? 600 : 400,
                  cursor: isOwner ? 'not-allowed' : 'pointer',
                  opacity: isOwner ? 0.45 : 1,
                  transition: 'all 0.15s',
                }}
              >
                <Heart
                  className="w-4 h-4"
                  fill={liked ? '#e5484d' : 'none'}
                  stroke={liked ? '#e5484d' : 'currentColor'}
                />
                {displayLikes}
              </button>
              {isOwner && (
                <p style={{ fontSize: 11, color: 'var(--muted-foreground)', margin: 0 }}>
                  Can't like your own pet
                </p>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* ── Photos ────────────────────────────────────────────────── */}
      {pet.images.length > 0 && (
        <div style={{ maxWidth: 860, margin: '40px auto 0', padding: '0 24px 56px' }}>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 8, marginBottom: 14 }}>
            <h2
              className="font-heading"
              style={{ fontSize: 20, fontWeight: 700, margin: 0 }}
            >
              Photos
            </h2>
            <span style={{ fontSize: 13, color: 'var(--muted-foreground)' }}>
              {pet.images.length}
            </span>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 10 }}>
            {pet.images.map((img, idx) => (
              <button
                key={img.petImageId}
                onClick={() => setLightboxIndex(idx)}
                style={{ aspectRatio: '1', borderRadius: 10, overflow: 'hidden', cursor: 'pointer', border: 'none', padding: 0, position: 'relative', display: 'block' }}
              >
                <img
                  src={getBlobImageUrl(img.blobName)}
                  alt={`${pet.name} photo ${idx + 1}`}
                  style={{ width: '100%', height: '100%', objectFit: 'cover', transition: 'transform 0.5s ease', display: 'block' }}
                  onMouseEnter={e => (e.currentTarget.style.transform = 'scale(1.06)')}
                  onMouseLeave={e => (e.currentTarget.style.transform = 'scale(1)')}
                />
              </button>
            ))}
          </div>
        </div>
      )}

      {/* ── Lightbox ───────────────────────────────────────────────── */}
      {lightboxIndex !== null && (
        <div
          onClick={() => setLightboxIndex(null)}
          style={{ position: 'fixed', inset: 0, zIndex: 500, background: 'rgba(0,0,0,0.92)', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: 24, animation: 'fade-in 0.2s ease' }}
        >
          {/* Close */}
          <button
            onClick={() => setLightboxIndex(null)}
            style={{ position: 'absolute', top: 20, right: 20, width: 40, height: 40, borderRadius: '50%', background: 'rgba(255,255,255,0.1)', border: 'none', cursor: 'pointer', color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
          >
            <X className="w-5 h-5" />
          </button>

          {/* Image */}
          <div
            onClick={e => e.stopPropagation()}
            style={{ position: 'relative', maxWidth: 900, maxHeight: '85vh', width: '100%' }}
          >
            <img
              src={getBlobImageUrl(pet.images[lightboxIndex].blobName)}
              alt={`${pet.name} photo ${lightboxIndex + 1}`}
              style={{ width: '100%', maxHeight: '80vh', objectFit: 'contain', borderRadius: 12, display: 'block' }}
            />
            {/* Counter */}
            <div
              style={{ position: 'absolute', bottom: -36, left: '50%', transform: 'translateX(-50%)', fontSize: 13, color: 'rgba(255,255,255,0.6)', whiteSpace: 'nowrap' }}
            >
              {lightboxIndex + 1} / {pet.images.length}
            </div>
          </div>

          {/* Arrows */}
          {pet.images.length > 1 && (
            <>
              <button
                onClick={e => { e.stopPropagation(); setLightboxIndex(i => i === null ? 0 : (i - 1 + pet.images.length) % pet.images.length) }}
                style={{ position: 'absolute', left: 16, top: '50%', transform: 'translateY(-50%)', width: 44, height: 44, borderRadius: '50%', background: 'rgba(255,255,255,0.12)', border: 'none', cursor: 'pointer', color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
              >
                <ChevronLeft className="w-5 h-5" />
              </button>
              <button
                onClick={e => { e.stopPropagation(); setLightboxIndex(i => i === null ? 0 : (i + 1) % pet.images.length) }}
                style={{ position: 'absolute', right: 16, top: '50%', transform: 'translateY(-50%)', width: 44, height: 44, borderRadius: '50%', background: 'rgba(255,255,255,0.12)', border: 'none', cursor: 'pointer', color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
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

      {/* ── Delete confirmation ────────────────────────────────────── */}
      {deleteOpen && (
        <div
          style={{ position: 'fixed', inset: 0, zIndex: 500, background: 'rgba(0,0,0,0.45)', backdropFilter: 'blur(3px)', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: 24 }}
        >
          <div
            style={{ background: 'var(--card)', borderRadius: 20, padding: 28, maxWidth: 360, width: '100%', boxShadow: '0 24px 64px rgba(0,0,0,0.15)' }}
          >
            <h3 className="font-heading" style={{ fontSize: 20, fontWeight: 700, marginBottom: 10 }}>
              Delete {pet.name}?
            </h3>
            <p style={{ fontSize: 14, color: 'var(--muted-foreground)', lineHeight: 1.6, marginBottom: 22 }}>
              This will permanently remove {pet.name} and all their photos. This can't be undone.
            </p>
            <div style={{ display: 'flex', gap: 10 }}>
              <button
                onClick={() => setDeleteOpen(false)}
                style={{ flex: 1, height: 42, borderRadius: 10, border: '1.5px solid var(--border)', background: 'transparent', fontFamily: "'DM Sans',sans-serif", fontSize: 14, fontWeight: 500, cursor: 'pointer', color: 'var(--foreground)' }}
              >
                Cancel
              </button>
              <button
                onClick={handleDelete}
                disabled={isDeleting}
                style={{ flex: 1, height: 42, borderRadius: 10, border: 'none', background: 'var(--destructive)', color: '#fff', fontFamily: "'DM Sans',sans-serif", fontSize: 14, fontWeight: 600, cursor: isDeleting ? 'not-allowed' : 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 8, opacity: isDeleting ? 0.6 : 1 }}
              >
                {isDeleting && <Loader2 className="w-4 h-4 animate-spin" />}
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

// ── Shared small components ───────────────────────────────────────────

function DarkChip({ children }: { children: React.ReactNode }) {
  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        height: 26,
        padding: '0 12px',
        borderRadius: 999,
        fontSize: 12,
        fontWeight: 500,
        background: 'rgba(255,255,255,0.2)',
        color: '#fff',
        border: '1px solid rgba(255,255,255,0.3)',
      }}
    >
      {children}
    </span>
  )
}

function KebabItem({
  icon,
  label,
  color,
  onClick,
}: {
  icon: React.ReactNode
  label: string
  color: string
  onClick: () => void
}) {
  return (
    <button
      onClick={onClick}
      style={{ width: '100%', height: 40, padding: '0 14px', display: 'flex', alignItems: 'center', gap: 9, background: 'transparent', border: 'none', cursor: 'pointer', color, fontFamily: "'DM Sans',sans-serif", fontSize: 13, fontWeight: 500, textAlign: 'left' }}
      onMouseEnter={e => (e.currentTarget.style.background = 'var(--secondary)')}
      onMouseLeave={e => (e.currentTarget.style.background = 'transparent')}
    >
      {icon}{label}
    </button>
  )
}
