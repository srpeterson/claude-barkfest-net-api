import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { login } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'

// ── Icon helpers ─────────────────────────────────────────────────────
function ArrowLeft() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="m15 18-6-6 6-6"/>
    </svg>
  )
}

function EyeOpen() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"/>
      <circle cx="12" cy="12" r="3"/>
    </svg>
  )
}

function EyeOff() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M9.88 9.88a3 3 0 1 0 4.24 4.24"/>
      <path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"/>
      <path d="M6.61 6.61A13.526 13.526 0 0 0 2 12s3 7 10 7a9.74 9.74 0 0 0 5.39-1.61"/>
      <line x1="2" x2="22" y1="2" y2="22"/>
    </svg>
  )
}

function Spinner() {
  return (
    <span
      style={{
        width: 16,
        height: 16,
        border: '2px solid rgba(255,255,255,0.4)',
        borderTopColor: '#fff',
        borderRadius: '50%',
        display: 'inline-block',
        flexShrink: 0,
        animation: 'spin 0.7s linear infinite',
      }}
    />
  )
}

// Brand panel mosaic — local pet photos served from public/pets/
// TODO (Roadmap #24): replace with live browse API images
const PET_IMAGES = [
  { src: '/pets/pet-1.jpg', tall: false },
  { src: '/pets/pet-2.jpg', tall: true  },
  { src: '/pets/pet-3.jpg', tall: true  },
  { src: '/pets/pet-4.jpg', tall: false },
]

// ── Shared input style ────────────────────────────────────────────────
const INPUT_BASE: React.CSSProperties = {
  width: '100%',
  height: 48,
  border: '1.5px solid var(--border)',
  borderRadius: 12,
  background: 'var(--card)',
  color: 'var(--foreground)',
  padding: '0 14px',
  fontFamily: "'DM Sans', sans-serif",
  fontSize: 14,
  outline: 'none',
  boxSizing: 'border-box',
  transition: 'border-color 0.15s, box-shadow 0.15s',
}

function focusIn(e: React.FocusEvent<HTMLInputElement>) {
  e.target.style.borderColor = 'var(--primary)'
  e.target.style.boxShadow = '0 0 0 3px var(--primary-10)'
}
function focusOut(e: React.FocusEvent<HTMLInputElement>) {
  e.target.style.borderColor = 'var(--border)'
  e.target.style.boxShadow = 'none'
}


// ── LoginPage ─────────────────────────────────────────────────────────
export function LoginPage() {
  const navigate = useNavigate()
  const { signIn } = useAuth()

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPw,        setShowPw]        = useState(false)
  const [error,         setError]         = useState('')
  const [forgotOpen,    setForgotOpen]    = useState(false)
  const [loading,  setLoading]  = useState(false)

  const allFilled = username.trim() !== '' && password !== ''

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!allFilled) return
    setLoading(true)
    setError('')
    try {
      const result = await login(username, password)
      signIn(result.accountId, 'owner', result.accessToken)
      navigate('/')
    } catch {
      setError('Invalid username or password.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>

      {/* ── Brand panel — 42%, hidden ≤680px ── */}
      <div
        className="brand-panel"
        style={{
          width: '42%',
          background: 'var(--primary)',
          display: 'flex',
          flexDirection: 'column',
          padding: '40px 48px',
          position: 'sticky',
          top: 0,
          alignSelf: 'flex-start',
          minHeight: '100vh',
        }}
      >
        {/* Logo */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <BarkfestMark size={32} inverted />
          <span
            className="font-heading"
            style={{ fontSize: 20, fontWeight: 700, color: '#fff', letterSpacing: '-0.02em' }}
          >
            Barkfest
          </span>
        </div>

        {/* Headline */}
        <div style={{ marginTop: 52 }}>
          <h1
            className="font-heading"
            style={{
              fontSize: 'clamp(26px, 2.8vw, 38px)',
              fontWeight: 700,
              color: '#fff',
              lineHeight: 1.25,
              marginBottom: 14,
            }}
          >
            Good to have<br />you back.
          </h1>
          <p style={{ fontSize: 15, color: 'rgba(255,255,255,0.72)', lineHeight: 1.65, maxWidth: 280, margin: 0 }}>
            Your pet's stories are waiting. Sign in to see what the community has been sharing.
          </p>
        </div>

        {/* 2×2 photo mosaic */}
        <div
          style={{
            display: 'grid',
            gridTemplateColumns: '1fr 1fr',
            gap: 10,
            marginTop: 40,
            flex: 1,
            maxHeight: 340,
          }}
        >
          {PET_IMAGES.map((p, i) => (
            <div
              key={i}
              style={{ borderRadius: 16, overflow: 'hidden', height: p.tall ? 180 : 140 }}
            >
              <img src={p.src} alt="" style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }} />
            </div>
          ))}
        </div>

        {/* Copyright */}
        <p style={{ marginTop: 'auto', paddingTop: 64, fontSize: 12, color: 'rgba(255,255,255,0.5)' }}>
          © {new Date().getFullYear()} Barkfest · Privacy · Terms
        </p>
      </div>

      {/* ── Form panel ── */}
      <div
        style={{
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'flex-start',
          padding: '80px 40px 48px',
          minHeight: '100vh',
        }}
      >
        {/* Back link */}
        <div style={{ width: '100%', maxWidth: 380, marginBottom: 0 }}>
          <Link
            to="/"
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: 6,
              fontSize: 13,
              color: 'var(--muted-foreground)',
              textDecoration: 'none',
              marginBottom: 32,
              transition: 'color 0.15s',
            }}
            onMouseEnter={e => (e.currentTarget.style.color = 'var(--foreground)')}
            onMouseLeave={e => (e.currentTarget.style.color = 'var(--muted-foreground)')}
          >
            <ArrowLeft />
            Back to Barkfest
          </Link>
        </div>

        <div style={{ width: '100%', maxWidth: 380 }}>
          {/* Mobile logo — hidden on desktop via brand panel presence */}
          <div className="mobile-auth-header" style={{ display: 'none', alignItems: 'center', gap: 10, marginBottom: 32 }}>
            <BarkfestMark size={28} />
            <span className="font-heading" style={{ fontSize: 18, fontWeight: 700 }}>Barkfest</span>
          </div>

          {/* Heading */}
          <div style={{ marginBottom: 36 }}>
            <h2
              className="font-heading"
              style={{ fontSize: 'clamp(22px, 2.5vw, 30px)', fontWeight: 700, marginBottom: 6 }}
            >
              Welcome back!
            </h2>
            <p style={{ fontSize: 14, color: 'var(--muted-foreground)', margin: 0 }}>
              New here?{' '}
              <Link
                to="/register"
                style={{ color: 'var(--primary)', fontWeight: 600, textDecoration: 'none' }}
              >
                Create an account
              </Link>
            </p>
          </div>

          <form onSubmit={handleSubmit} noValidate>

            {/* Username */}
            <div style={{ marginBottom: 20 }}>
              <label
                htmlFor="si-un"
                style={{ display: 'block', fontSize: 13, fontWeight: 600, marginBottom: 6, color: 'var(--foreground)' }}
              >
                Username <span style={{ color: 'var(--destructive)' }}>*</span>
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
                style={INPUT_BASE}
                onFocus={focusIn}
                onBlur={focusOut}
              />
            </div>

            {/* Password */}
            <div style={{ marginBottom: 20 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: 6 }}>
                <label
                  htmlFor="si-pw"
                  style={{ fontSize: 13, fontWeight: 600, color: 'var(--foreground)', margin: 0 }}
                >
                  Password <span style={{ color: 'var(--destructive)' }}>*</span>
                </label>
                <button
                  type="button"
                  onClick={() => setForgotOpen(true)}
                  style={{ fontSize: 12, color: 'var(--primary)', textDecoration: 'none', fontWeight: 500, background: 'none', border: 'none', cursor: 'pointer', padding: 0 }}
                >
                  Forgot password?
                </button>
              </div>
              <div style={{ position: 'relative' }}>
                <input
                  id="si-pw"
                  type={showPw ? 'text' : 'password'}
                  autoComplete="current-password"
                  maxLength={72}
                  value={password}
                  onChange={e => setPassword(e.target.value)}
                  style={{ ...INPUT_BASE, paddingRight: 48 }}
                  onFocus={focusIn}
                  onBlur={focusOut}
                />
                <button
                  type="button"
                  onClick={() => setShowPw(v => !v)}
                  style={{
                    position: 'absolute',
                    right: 0,
                    top: 0,
                    height: 48,
                    width: 48,
                    background: 'none',
                    border: 'none',
                    cursor: 'pointer',
                    color: 'var(--muted-foreground)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  {showPw ? <EyeOff /> : <EyeOpen />}
                </button>
              </div>
            </div>

            {error && (
              <p style={{ fontSize: 13, color: 'var(--destructive)', marginBottom: 12, textAlign: 'center' }}>
                {error}
              </p>
            )}

            {/* Submit */}
            <button
              type="submit"
              disabled={!allFilled || loading}
              style={{
                width: '100%',
                height: 52,
                borderRadius: 14,
                border: 'none',
                background: 'var(--primary)',
                color: '#fff',
                fontFamily: "'DM Sans', sans-serif",
                fontSize: 15,
                fontWeight: 600,
                cursor: !allFilled || loading ? 'not-allowed' : 'pointer',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: 8,
                transition: 'opacity 0.15s',
                opacity: !allFilled || loading ? 0.45 : 1,
                marginTop: 8,
              }}
            >
              {loading && <Spinner />}
              {loading ? 'Signing in…' : 'Sign in'}
            </button>
          </form>

          {/* TODO (Roadmap #25): "or continue with" divider + Google/Apple buttons hidden until
              third-party OAuth providers are implemented. Restore this block when ready. */}

          <p style={{ textAlign: 'center', fontSize: 12, color: 'var(--muted-foreground)', marginTop: 24, lineHeight: 1.5 }}>
            Privacy Policy · Terms of Use
          </p>
        </div>
      </div>

      {/* ── Forgot password modal ── */}
      {forgotOpen && (
        <div
          className="animate-backdrop-in"
          style={{ position: 'fixed', inset: 0, zIndex: 100, display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'rgba(0,0,0,0.5)', backdropFilter: 'blur(4px)', padding: 16 }}
          onClick={() => setForgotOpen(false)}
        >
          <div
            className="animate-dialog-appear"
            onClick={e => e.stopPropagation()}
            style={{ width: '100%', maxWidth: 360, background: 'var(--card)', borderRadius: 20, padding: 28, boxShadow: '0 24px 64px rgba(0,0,0,0.18)', position: 'relative' }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 16 }}>
              <BarkfestMark size={22} />
              <span className="font-heading" style={{ fontSize: 17, fontWeight: 700 }}>Barkfest</span>
            </div>
            <h3 className="font-heading" style={{ fontSize: 20, fontWeight: 700, marginBottom: 8 }}>Forgot your password?</h3>
            <p style={{ fontSize: 14, color: 'var(--muted-foreground)', lineHeight: 1.65, marginBottom: 6 }}>
              Woof! Automated reset is on its way. Until then, shoot us an email and we'll get your paws back on the keys. Don't forget to include your username:
            </p>
            <a
              href="mailto:srpeterson@outlook.com"
              style={{ fontSize: 14, fontWeight: 600, color: 'var(--primary)', textDecoration: 'none', display: 'inline-block', marginBottom: 22 }}
            >
              srpeterson@outlook.com
            </a>
            <button
              onClick={() => setForgotOpen(false)}
              style={{ display: 'block', width: '100%', height: 42, borderRadius: 10, border: '1.5px solid var(--border)', background: 'transparent', color: 'var(--muted-foreground)', fontFamily: "'DM Sans', sans-serif", fontSize: 14, fontWeight: 500, cursor: 'pointer' }}
            >
              Close
            </button>
          </div>
        </div>
      )}

      {/* ── Responsive: hide brand panel ≤680px ── */}
      <style>{`
        @media (max-width: 680px) {
          .brand-panel { display: none !important; }
          .mobile-auth-header { display: flex !important; }
        }
        @keyframes spin { to { transform: rotate(360deg); } }
      `}</style>
    </div>
  )
}
