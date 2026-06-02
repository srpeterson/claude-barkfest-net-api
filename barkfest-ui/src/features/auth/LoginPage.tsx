import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { Eye, EyeOff, Loader2 } from 'lucide-react'
import { useAuth } from '@/hooks/useAuth'
import { getOwnerById, login, setAuthToken } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'

// Brand panel mosaic — local pet photos served from public/pets/
// TODO (Roadmap #24): replace with live browse API images
const PET_IMAGES = [
  { src: '/pets/pet-1.jpg', tall: false },
  { src: '/pets/pet-2.jpg', tall: true  },
  { src: '/pets/pet-3.jpg', tall: true  },
  { src: '/pets/pet-4.jpg', tall: false },
]

const inputCls = [
  'w-full h-12 rounded-xl border-[1.5px] border-border',
  'bg-card text-foreground px-3.5 text-sm',
  'outline-none box-border transition',
  'focus:border-primary focus:ring-2 focus:ring-primary/30',
].join(' ')

export function LoginPage() {
  const navigate = useNavigate()
  const { signIn } = useAuth()
  const [searchParams] = useSearchParams()
  const signedOut = searchParams.get('signed-out') === 'true'

  const [username, setUsername]   = useState('')
  const [password, setPassword]   = useState('')
  const [showPw, setShowPw]       = useState(false)
  const [error, setError]         = useState('')
  const [forgotOpen, setForgotOpen] = useState(false)
  const [loading, setLoading]     = useState(false)

  const allFilled = username.trim() !== '' && password !== ''

  async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    if (!allFilled) return
    setLoading(true)
    setError('')
    try {
      const result = await login(username, password)
      let profileImageBlobName: string | null = null
      try {
        setAuthToken(result.accessToken)
        const owner = await getOwnerById(result.accountId)
        profileImageBlobName = owner.profileImage?.blobName ?? null
      } catch {
        // Non-fatal — proceed with null profile image
      }
      signIn(result.accountId, 'owner', result.accessToken, profileImageBlobName)
      navigate('/')
    } catch {
      setError('Invalid username or password.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen">

      {/* ── Brand panel — 42%, hidden ≤680px ── */}
      <div
        className="brand-panel w-[42%] min-h-screen bg-primary flex flex-col p-[40px_48px] sticky top-0 self-start"
      >
        {/* Logo */}
        <div className="flex items-center gap-2.5">
          <BarkfestMark size={32} inverted />
          <span className="font-heading text-xl font-bold text-white tracking-[-0.02em]">Barkfest</span>
        </div>

        {/* Headline */}
        <div className="mt-[52px]">
          <h1 className="font-heading font-bold text-white leading-[1.25] mb-3.5 text-[clamp(26px,2.8vw,38px)]">
            {signedOut ? 'Gone but not fur-gotten.' : <>Your pets deserve<br />the spotlight.</>}
          </h1>
          <p className="text-[15px] text-white/70 leading-relaxed max-w-[280px] m-0">
            {signedOut
              ? 'Your pets are still adorable. Sign back in to see for yourself!'
              : 'Sign in and give your pets their moment to shine!'}
          </p>
        </div>

        {/* 2×2 photo mosaic */}
        <div className="grid grid-cols-2 gap-2.5 mt-10 flex-1 max-h-[340px]">
          {PET_IMAGES.map((p, i) => (
            <div key={i} className="rounded-2xl overflow-hidden" style={{ height: p.tall ? 180 : 140 }}>
              <img src={p.src} alt="" className="w-full h-full object-cover block" />
            </div>
          ))}
        </div>

        {/* Copyright */}
        <p className="mt-auto pt-16 text-xs text-white/50">
          © {new Date().getFullYear()} Barkfest · Privacy · Terms
        </p>
      </div>

      {/* ── Form panel ── */}
      <div className="flex-1 flex flex-col items-center justify-start px-10 pt-20 pb-12 min-h-screen">

        {/* Back link */}
        <div className="w-full max-w-[380px]">
          <Link
            to="/"
            className="inline-flex items-center gap-1.5 text-[13px] text-muted-foreground no-underline mb-8 transition-colors hover:text-foreground"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="m15 18-6-6 6-6"/>
            </svg>
            Back to Barkfest
          </Link>
        </div>

        <div className="w-full max-w-[380px]">
          {/* Mobile logo */}
          <div className="mobile-auth-header hidden items-center gap-2.5 mb-8">
            <BarkfestMark size={28} />
            <span className="font-heading text-[18px] font-bold">Barkfest</span>
          </div>

          {/* Heading */}
          <div className="mb-9">
            <h2 className="font-heading font-bold mb-1.5 text-[clamp(22px,2.5vw,30px)]">
              {signedOut ? 'Pawsing for a break?' : 'Barkfest awaits!'}
            </h2>
            <p className="text-sm text-muted-foreground m-0">
              {signedOut ? (
                'Miss your pets already? Sign back in anytime.'
              ) : (
                <>
                  New to Barkfest?{' '}
                  <Link to="/register" className="text-primary font-semibold no-underline">
                    Create an account
                  </Link>
                </>
              )}
            </p>
          </div>

          <form onSubmit={handleSubmit} noValidate>

            {/* Username */}
            <div className="mb-5">
              <label htmlFor="si-un" className="block text-[13px] font-semibold mb-1.5 text-foreground">
                Username <span className="text-destructive">*</span>
              </label>
              <input
                id="si-un"
                type="text"
                autoComplete="username"
                maxLength={25}
                autoFocus
                placeholder="Your username"
                value={username}
                onChange={e => setUsername(e.target.value)}
                className={inputCls}
              />
            </div>

            {/* Password */}
            <div className="mb-5">
              <div className="flex justify-between items-baseline mb-1.5">
                <label htmlFor="si-pw" className="text-[13px] font-semibold text-foreground m-0">
                  Password <span className="text-destructive">*</span>
                </label>
                <button
                  type="button"
                  onClick={() => setForgotOpen(true)}
                  className="text-xs text-primary font-medium bg-transparent border-0 cursor-pointer p-0"
                >
                  Forgot password?
                </button>
              </div>
              <div className="relative">
                <input
                  id="si-pw"
                  type={showPw ? 'text' : 'password'}
                  autoComplete="current-password"
                  maxLength={72}
                  value={password}
                  onChange={e => setPassword(e.target.value)}
                  className={inputCls + ' pr-12'}
                />
                <button
                  type="button"
                  onClick={() => setShowPw(v => !v)}
                  aria-label={showPw ? 'Hide password' : 'Show password'}
                  className="absolute right-0 top-0 h-12 w-12 flex items-center justify-center bg-transparent border-0 cursor-pointer text-muted-foreground hover:text-foreground transition-colors"
                >
                  {showPw ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
            </div>

            {error && (
              <p className="text-[13px] text-destructive text-center mb-3">{error}</p>
            )}

            <button
              type="submit"
              disabled={!allFilled || loading}
              className="w-full h-[52px] rounded-[14px] border-0 bg-primary text-white text-[15px] font-semibold cursor-pointer flex items-center justify-center gap-2 mt-2 disabled:cursor-not-allowed disabled:opacity-[0.45] transition-opacity"
            >
              {loading && <Loader2 className="w-4 h-4 animate-spin" />}
              {loading ? 'Signing in…' : 'Sign in'}
            </button>
          </form>

          {/* TODO (Roadmap #25): "or continue with" divider + Google/Apple buttons hidden until
              third-party OAuth providers are implemented. Restore this block when ready. */}

          <p className="text-center text-xs text-muted-foreground mt-6 leading-relaxed">
            Privacy Policy · Terms of Use
          </p>
        </div>
      </div>

      {/* ── Forgot password modal ── */}
      {forgotOpen && (
        <div
          className="animate-backdrop-in fixed inset-0 z-[100] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4"
          onClick={() => setForgotOpen(false)}
        >
          <div
            className="animate-dialog-appear w-full max-w-[360px] bg-card rounded-[20px] p-7 shadow-[0_24px_64px_rgba(0,0,0,0.18)] relative"
            onClick={e => e.stopPropagation()}
          >
            <div className="flex items-center gap-2 mb-4">
              <BarkfestMark size={22} />
              <span className="font-heading text-[17px] font-bold">Barkfest</span>
            </div>
            <h3 className="font-heading text-xl font-bold mb-2">Forgot your password?</h3>
            <p className="text-sm text-muted-foreground leading-relaxed mb-1.5">
              Woof! Automated reset is on its way. Until then, shoot us an email and we'll get your paws back on the keys. Don't forget to include your username:
            </p>
            <a
              href="mailto:srpeterson@outlook.com"
              className="text-sm font-semibold text-primary no-underline inline-block mb-[22px]"
            >
              srpeterson@outlook.com
            </a>
            <button
              onClick={() => setForgotOpen(false)}
              className="block w-full h-[42px] rounded-[10px] border-[1.5px] border-border bg-transparent text-muted-foreground text-sm font-medium cursor-pointer"
            >
              Close
            </button>
          </div>
        </div>
      )}

      <style>{`
        @media (max-width: 680px) {
          .brand-panel { display: none !important; }
          .mobile-auth-header { display: flex !important; }
        }
      `}</style>
    </div>
  )
}
