import { useCallback, useEffect, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import zxcvbn from 'zxcvbn'
import { useAuth } from '@/hooks/useAuth'
import { checkDisplayName, login, register } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'

// ── Constants ─────────────────────────────────────────────────────────
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

const STRENGTH_LABELS = ['Very weak', 'Weak', 'Fair', 'Strong', 'Very strong']
const STRENGTH_COLORS = ['#e5484d', '#f76b15', '#d4a017', 'var(--accent)', 'var(--accent)']

const PET_IMAGES = [
  'https://images.unsplash.com/photo-1587300003388-59208cc962cb?w=120&h=120&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=120&h=120&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1518717758536-85ae29035b6d?w=120&h=120&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1561037404-61cd46aa615b?w=120&h=120&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1505628346881-b72b27e84530?w=120&h=120&fit=crop&auto=format',
]

// ── Icon helpers ──────────────────────────────────────────────────────
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

function CheckIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
      <polyline points="20 6 9 17 4 12"/>
    </svg>
  )
}

function Spinner() {
  return (
    <span
      style={{
        width: 10,
        height: 10,
        border: '2px solid var(--muted-foreground)',
        borderTopColor: 'transparent',
        borderRadius: '50%',
        display: 'inline-block',
        animation: 'spin 0.6s linear infinite',
      }}
    />
  )
}

// ── Shared input style ────────────────────────────────────────────────
const INPUT_BASE: React.CSSProperties = {
  width: '100%',
  height: 46,
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

function focusIn(e: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement>) {
  e.target.style.borderColor = 'var(--primary)'
  e.target.style.boxShadow = '0 0 0 3px var(--primary-10)'
}
function focusOut(e: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement>) {
  e.target.style.borderColor = 'var(--border)'
  e.target.style.boxShadow = 'none'
}

// ── DisplayName availability state ────────────────────────────────────
type DnStatus = 'idle' | 'checking' | 'available' | 'taken'

// ── Form state ────────────────────────────────────────────────────────
interface FormState {
  firstName: string
  lastName: string
  email: string
  username: string
  displayName: string
  password: string
  confirmPassword: string
}

// ── RegisterPage ──────────────────────────────────────────────────────
export function RegisterPage() {
  const navigate = useNavigate()
  const { signIn } = useAuth()

  const [form, setForm] = useState<FormState>({
    firstName: '',
    lastName: '',
    email: '',
    username: '',
    displayName: '',
    password: '',
    confirmPassword: '',
  })
  const [showPw, setShowPw]       = useState(false)
  const [dnStatus, setDnStatus]   = useState<DnStatus>('idle')
  const [error, setError]         = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const debounceRef               = useRef<ReturnType<typeof setTimeout> | null>(null)

  const strength        = zxcvbn(form.password)
  const emailBad        = form.email.trim() !== '' && !EMAIL_REGEX.test(form.email.trim())
  const dnStripped      = form.displayName.replace(/\s/g, '')
  const dnTooShort      = dnStripped.length > 0 && dnStripped.length < 4
  const pwMismatch      = form.confirmPassword !== '' && form.password !== form.confirmPassword
  const pwWeak          = form.password.length > 0 && strength.score < 2

  const canSubmit =
    form.firstName.trim() &&
    form.lastName.trim() &&
    form.email.trim() &&
    !emailBad &&
    form.username.trim() &&
    form.displayName.trim() &&
    !dnTooShort &&
    dnStatus === 'available' &&
    form.password &&
    !pwWeak &&
    form.confirmPassword &&
    !pwMismatch &&
    !isLoading

  function handleChange(e: ChangeEvent<HTMLInputElement>) {
    const { name, value } = e.target
    setForm(f => ({ ...f, [name]: value }))
  }

  const checkDN = useCallback((value: string) => {
    if (debounceRef.current) clearTimeout(debounceRef.current)
    const trimmed = value.trim()
    if (!trimmed || trimmed.replace(/\s/g, '').length < 4) {
      setDnStatus('idle')
      return
    }
    setDnStatus('checking')
    debounceRef.current = setTimeout(async () => {
      try {
        const available = await checkDisplayName(trimmed)
        setDnStatus(available ? 'available' : 'taken')
      } catch {
        setDnStatus('idle')
      }
    }, 500)
  }, [])

  useEffect(() => {
    checkDN(form.displayName)
  }, [form.displayName, checkDN])

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!canSubmit) return
    setError(null)
    setIsLoading(true)
    try {
      await register({
        username: form.username,
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        password: form.password,
        displayName: form.displayName.trim(),
      })
      const result = await login(form.username, form.password)
      signIn(result.accountId, 'owner', result.accessToken)
      navigate('/')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Registration failed. Check your details and try again.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>

      {/* ── Brand panel — 42%, hidden ≤680px ── */}
      <div
        className="brand-panel"
        style={{
          width: '42%',
          minHeight: '100vh',
          background: 'var(--primary)',
          display: 'flex',
          flexDirection: 'column',
          padding: '40px 48px',
          position: 'sticky',
          top: 0,
          alignSelf: 'flex-start',
        }}
      >
        {/* Logo */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <BarkfestMark size={32} inverted />
          <span className="font-heading" style={{ fontSize: 20, fontWeight: 700, color: '#fff', letterSpacing: '-0.02em' }}>
            Barkfest
          </span>
        </div>

        {/* Headline */}
        <div style={{ marginTop: 56 }}>
          <h1
            className="font-heading"
            style={{ fontSize: 'clamp(28px, 3vw, 40px)', fontWeight: 700, color: '#fff', lineHeight: 1.2, marginBottom: 16 }}
          >
            Every pet has a<br />story to tell.
          </h1>
          <p style={{ fontSize: 15, color: 'rgba(255,255,255,0.75)', lineHeight: 1.65, maxWidth: 300, margin: 0 }}>
            Join a community of pet lovers sharing photos, stories, and the everyday magic of life with animals.
          </p>
        </div>

        {/* Testimonial card */}
        <div style={{ marginTop: 40, padding: '20px 24px', background: 'rgba(255,255,255,0.12)', borderRadius: 16 }}>
          <p style={{ fontSize: 13, color: 'rgba(255,255,255,0.85)', lineHeight: 1.6, fontStyle: 'italic', marginBottom: 14 }}>
            "Finally a place to share my cat's daily chaos without it getting lost in a general social feed."
          </p>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <img
              src={PET_IMAGES[1]}
              alt=""
              style={{ width: 32, height: 32, borderRadius: '50%', objectFit: 'cover', border: '2px solid rgba(255,255,255,0.4)' }}
            />
            <div>
              <p style={{ fontSize: 12, fontWeight: 600, color: '#fff', margin: 0 }}>Sara L.</p>
              <p style={{ fontSize: 11, color: 'rgba(255,255,255,0.6)', margin: 0 }}>Cat mum · joined 2024</p>
            </div>
          </div>
        </div>

        {/* Pet strip — overlapping circular thumbnails */}
        <div style={{ display: 'flex', marginTop: 'auto', paddingTop: 32 }}>
          <div style={{ display: 'flex', alignItems: 'center' }}>
            {PET_IMAGES.map((src, i) => (
              <div
                key={i}
                style={{
                  width: 52,
                  height: 52,
                  borderRadius: '50%',
                  overflow: 'hidden',
                  border: '2.5px solid rgba(255,255,255,0.5)',
                  marginLeft: i === 0 ? 0 : -14,
                  flexShrink: 0,
                }}
              >
                <img src={src} alt="" style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }} />
              </div>
            ))}
          </div>
          <p style={{ marginLeft: 14, fontSize: 13, color: 'rgba(255,255,255,0.75)', alignSelf: 'center' }}>
            Join <strong style={{ color: '#fff' }}>hundreds</strong> of pet lovers
          </p>
        </div>
      </div>

      {/* ── Form panel ── */}
      <div
        style={{
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          padding: '48px 40px 64px',
          overflowY: 'auto',
        }}
      >
        {/* Back link */}
        <div style={{ width: '100%', maxWidth: 420, marginBottom: 0 }}>
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

        <div style={{ width: '100%', maxWidth: 420 }}>
          {/* Mobile logo */}
          <div className="mobile-auth-header" style={{ display: 'none', alignItems: 'center', gap: 10, marginBottom: 28 }}>
            <BarkfestMark size={28} />
            <span className="font-heading" style={{ fontSize: 18, fontWeight: 700 }}>Barkfest</span>
          </div>

          {/* Heading */}
          <div style={{ marginBottom: 32 }}>
            <h2
              className="font-heading"
              style={{ fontSize: 'clamp(22px, 2.5vw, 30px)', fontWeight: 700, marginBottom: 6 }}
            >
              Create your account
            </h2>
            <p style={{ fontSize: 14, color: 'var(--muted-foreground)', margin: 0 }}>
              Already have one?{' '}
              <Link to="/login" style={{ color: 'var(--primary)', fontWeight: 600, textDecoration: 'none' }}>
                Sign in
              </Link>
            </p>
          </div>

          <form onSubmit={handleSubmit} noValidate>

            {/* First / Last name — 2-col */}
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12, marginBottom: 18 }}>
              <div>
                <label htmlFor="r-fn" style={LABEL}>First name <Required /></label>
                <input
                  id="r-fn" name="firstName" type="text"
                  autoComplete="given-name" maxLength={50} autoFocus
                  placeholder="Jane"
                  value={form.firstName} onChange={handleChange}
                  style={INPUT_BASE} onFocus={focusIn} onBlur={focusOut}
                />
              </div>
              <div>
                <label htmlFor="r-ln" style={LABEL}>Last name <Required /></label>
                <input
                  id="r-ln" name="lastName" type="text"
                  autoComplete="family-name" maxLength={100}
                  placeholder="Doe"
                  value={form.lastName} onChange={handleChange}
                  style={INPUT_BASE} onFocus={focusIn} onBlur={focusOut}
                />
              </div>
            </div>

            {/* Email */}
            <div style={{ marginBottom: 18 }}>
              <label htmlFor="r-em" style={LABEL}>Email <Required /></label>
              <input
                id="r-em" name="email" type="email"
                autoComplete="email" maxLength={75}
                placeholder="you@example.com"
                value={form.email} onChange={handleChange}
                style={INPUT_BASE} onFocus={focusIn} onBlur={focusOut}
              />
              {emailBad && <p style={ERROR_HINT}>Must be a valid email address.</p>}
            </div>

            {/* Username */}
            <div style={{ marginBottom: 18 }}>
              <label htmlFor="r-un" style={LABEL}>Username <Required /></label>
              <input
                id="r-un" name="username" type="text"
                autoComplete="username" maxLength={25}
                placeholder="Pick a username"
                value={form.username} onChange={handleChange}
                style={INPUT_BASE} onFocus={focusIn} onBlur={focusOut}
              />
            </div>

            {/* Display name with availability check */}
            <div style={{ marginBottom: 18 }}>
              <label htmlFor="r-dn" style={LABEL}>Display name <Required /></label>
              <p style={{ fontSize: 12, color: 'var(--muted-foreground)', marginBottom: 6, marginTop: 0 }}>
                Shown on your pet cards — e.g. "Cool Pet Dad"
              </p>
              <input
                id="r-dn" name="displayName" type="text"
                autoComplete="nickname" maxLength={25}
                placeholder="e.g. Cool Pet Dad"
                value={form.displayName} onChange={handleChange}
                style={INPUT_BASE} onFocus={focusIn} onBlur={focusOut}
              />
              {form.displayName.trim() && (
                dnTooShort ? (
                  <p style={ERROR_HINT}>At least 4 characters required</p>
                ) : dnStatus === 'checking' ? (
                  <p style={{ ...HINT, display: 'flex', alignItems: 'center', gap: 5 }}>
                    <Spinner />Checking availability…
                  </p>
                ) : dnStatus === 'available' ? (
                  <p style={{ ...HINT, color: '#1a7f4b', display: 'flex', alignItems: 'center', gap: 4 }}>
                    <CheckIcon />Available
                  </p>
                ) : dnStatus === 'taken' ? (
                  <p style={ERROR_HINT}>Already taken — try another</p>
                ) : null
              )}
            </div>

            {/* Password */}
            <div style={{ marginBottom: 18 }}>
              <label htmlFor="r-pw" style={LABEL}>Password <Required /></label>
              <div style={{ position: 'relative' }}>
                <input
                  id="r-pw" name="password"
                  type={showPw ? 'text' : 'password'}
                  autoComplete="new-password" minLength={10} maxLength={72}
                  value={form.password} onChange={handleChange}
                  style={{ ...INPUT_BASE, paddingRight: 44 }}
                  onFocus={focusIn} onBlur={focusOut}
                />
                <button
                  type="button" onClick={() => setShowPw(v => !v)}
                  style={{
                    position: 'absolute', right: 0, top: 0,
                    height: 46, width: 44,
                    background: 'none', border: 'none', cursor: 'pointer',
                    color: 'var(--muted-foreground)',
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                  }}
                >
                  {showPw ? <EyeOff /> : <EyeOpen />}
                </button>
              </div>
              {form.password && (
                <div style={{ marginTop: 8 }}>
                  <div style={{ display: 'flex', gap: 4, marginBottom: 3 }}>
                    {[0, 1, 2, 3].map(i => (
                      <div
                        key={i}
                        style={{
                          flex: 1, height: 3, borderRadius: 2,
                          background: i < strength.score ? STRENGTH_COLORS[strength.score] : 'var(--border)',
                          transition: 'background 0.25s',
                        }}
                      />
                    ))}
                  </div>
                  <p style={{ margin: 0, fontSize: 11, fontWeight: 600, color: STRENGTH_COLORS[strength.score] }}>
                    {STRENGTH_LABELS[strength.score]}
                  </p>
                </div>
              )}
              {pwWeak && strength.feedback?.suggestions?.[0] && (
                <p style={ERROR_HINT}>{strength.feedback.suggestions[0]}</p>
              )}
            </div>

            {/* Confirm password */}
            <div style={{ marginBottom: 18 }}>
              <label htmlFor="r-cpw" style={LABEL}>Confirm password <Required /></label>
              <div style={{ position: 'relative' }}>
                <input
                  id="r-cpw" name="confirmPassword"
                  type={showPw ? 'text' : 'password'}
                  autoComplete="new-password" maxLength={72}
                  value={form.confirmPassword} onChange={handleChange}
                  style={{ ...INPUT_BASE, paddingRight: 44 }}
                  onFocus={focusIn} onBlur={focusOut}
                />
                <button
                  type="button" onClick={() => setShowPw(v => !v)}
                  style={{
                    position: 'absolute', right: 0, top: 0,
                    height: 46, width: 44,
                    background: 'none', border: 'none', cursor: 'pointer',
                    color: 'var(--muted-foreground)',
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                  }}
                >
                  {showPw ? <EyeOff /> : <EyeOpen />}
                </button>
              </div>
              {pwMismatch && <p style={ERROR_HINT}>Passwords do not match.</p>}
            </div>

            {error && (
              <p style={{ fontSize: 13, color: 'var(--destructive)', textAlign: 'center', marginBottom: 10 }}>
                {error}
              </p>
            )}

            {/* Submit */}
            <button
              type="submit"
              disabled={!canSubmit || isLoading}
              style={{
                width: '100%',
                height: 50,
                borderRadius: 14,
                border: 'none',
                background: 'var(--primary)',
                color: '#fff',
                fontFamily: "'DM Sans', sans-serif",
                fontSize: 15,
                fontWeight: 600,
                cursor: !canSubmit || isLoading ? 'not-allowed' : 'pointer',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: 8,
                opacity: !canSubmit || isLoading ? 0.45 : 1,
                transition: 'opacity 0.15s',
                marginTop: 8,
              }}
            >
              {isLoading && (
                <span style={{ width: 16, height: 16, border: '2px solid rgba(255,255,255,0.4)', borderTopColor: '#fff', borderRadius: '50%', display: 'inline-block', animation: 'spin 0.7s linear infinite', flexShrink: 0 }} />
              )}
              {isLoading ? 'Creating your account…' : 'Create account'}
            </button>

            <p style={{ textAlign: 'center', fontSize: 13, color: 'var(--muted-foreground)', marginTop: 16 }}>
              By creating an account you agree to our Terms and Privacy Policy.
            </p>
          </form>
        </div>
      </div>

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

// ── Shared micro-components / styles ─────────────────────────────────
const LABEL: React.CSSProperties = {
  display: 'block',
  fontSize: 13,
  fontWeight: 600,
  marginBottom: 6,
  color: 'var(--foreground)',
}

const HINT: React.CSSProperties = {
  fontSize: 12,
  color: 'var(--muted-foreground)',
  margin: '4px 0 0',
}

const ERROR_HINT: React.CSSProperties = {
  fontSize: 12,
  color: 'var(--destructive)',
  margin: '4px 0 0',
}

function Required() {
  return <span style={{ color: 'var(--destructive)', marginLeft: 2 }}>*</span>
}
