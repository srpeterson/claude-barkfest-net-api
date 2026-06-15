import { useCallback, useEffect, useRef, useState, type ChangeEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Check, Eye, EyeOff, Loader2 } from 'lucide-react'
import zxcvbn from 'zxcvbn'
import { useAuth } from '@/hooks/useAuth'
import { checkDisplayName, checkUsername, login, register } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'
import { PasswordStrengthMeter } from '@/components/PasswordStrengthMeter'
import { inputBaseCls } from '@/lib/formStyles'
import { isValidEmail } from '@/lib/email'
import { LIMITS } from '@/config/constraints'

const PET_IMAGES = [
  'https://images.unsplash.com/photo-1587300003388-59208cc962cb?w=120&h=120&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=120&h=120&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1518717758536-85ae29035b6d?w=120&h=120&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1561037404-61cd46aa615b?w=120&h=120&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1505628346881-b72b27e84530?w=120&h=120&fit=crop&auto=format',
]

const inputCls = `${inputBaseCls} h-[46px] bg-card px-3.5`

type UnStatus = 'idle' | 'checking' | 'available' | 'taken'
type DnStatus = 'idle' | 'checking' | 'available' | 'taken'

interface FormState {
  firstName: string
  lastName: string
  email: string
  username: string
  displayName: string
  password: string
  confirmPassword: string
}

export function RegisterPage() {
  const navigate = useNavigate()
  const { signIn } = useAuth()

  const [form, setForm] = useState<FormState>({
    firstName: '', lastName: '', email: '', username: '',
    displayName: '', password: '', confirmPassword: '',
  })
  const [showPw, setShowPw]       = useState(false)
  const [unStatus, setUnStatus]   = useState<UnStatus>('idle')
  const [dnStatus, setDnStatus]   = useState<DnStatus>('idle')
  const [error, setError]         = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const debounceUnRef             = useRef<ReturnType<typeof setTimeout> | null>(null)
  const debounceRef               = useRef<ReturnType<typeof setTimeout> | null>(null)

  const strength   = zxcvbn(form.password)
  const emailBad   = form.email.trim() !== '' && !isValidEmail(form.email)
  const unTooShort = form.username.trim().length > 0 && form.username.trim().length < 5
  const dnStripped = form.displayName.replace(/\s/g, '')
  const dnTooShort = dnStripped.length > 0 && dnStripped.length < 4
  const pwMismatch = form.confirmPassword !== '' && form.password !== form.confirmPassword
  const pwWeak     = form.password.length > 0 && strength.score < 2

  const canSubmit =
    form.firstName.trim() &&
    form.lastName.trim() &&
    form.email.trim() &&
    !emailBad &&
    form.username.trim() &&
    !unTooShort &&
    unStatus === 'available' &&
    form.displayName.trim() &&
    !dnTooShort &&
    dnStatus === 'available' &&
    form.password &&
    form.confirmPassword &&
    !pwMismatch &&
    !isLoading

  function handleChange(e: ChangeEvent<HTMLInputElement>) {
    const { name, value } = e.target
    setForm(f => ({ ...f, [name]: value }))
  }

  const checkUN = useCallback((value: string) => {
    if (debounceUnRef.current) clearTimeout(debounceUnRef.current)
    const trimmed = value.trim()
    if (!trimmed || trimmed.length < 5) { setUnStatus('idle'); return }
    setUnStatus('checking')
    debounceUnRef.current = setTimeout(async () => {
      try {
        const available = await checkUsername(trimmed)
        setUnStatus(available ? 'available' : 'taken')
      } catch {
        setUnStatus('idle')
      }
    }, 500)
  }, [])

  useEffect(() => { checkUN(form.username) }, [form.username, checkUN])

  const checkDN = useCallback((value: string) => {
    if (debounceRef.current) clearTimeout(debounceRef.current)
    const trimmed = value.trim()
    if (!trimmed || trimmed.replace(/\s/g, '').length < 4) { setDnStatus('idle'); return }
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

  useEffect(() => { checkDN(form.displayName) }, [form.displayName, checkDN])

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
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
    <div className="flex min-h-screen">

      {/* ── Brand panel — 42%, hidden ≤680px ── */}
      <div className="brand-panel w-[42%] min-h-screen bg-primary flex flex-col p-[40px_48px] sticky top-0 self-start">

        {/* Logo */}
        <div className="flex items-center gap-2.5">
          <BarkfestMark size={32} inverted />
          <span className="font-heading text-xl font-bold text-white tracking-[-0.02em]">Barkfest</span>
        </div>

        {/* Headline */}
        <div className="mt-14">
          <h1 className="font-heading font-bold text-white leading-[1.2] mb-4 text-[clamp(28px,3vw,40px)]">
            Every pet has a<br />story to tell.
          </h1>
          <p className="text-[15px] text-white/75 leading-relaxed max-w-[300px] m-0">
            Join a community of pet lovers sharing photos, stories, and the everyday magic of life with pets.
          </p>
        </div>

        {/* Testimonial card */}
        <div className="mt-10 p-5 bg-white/[0.12] rounded-2xl">
          <p className="text-[13px] text-white/85 leading-relaxed italic mb-3.5">
            "Finally a place to share my dog's daily shenanigans without it getting lost in a general social feed."
          </p>
          <div className="flex items-center gap-2.5">
            <img
              src="/pets/pet-6.jpg"
              alt=""
              className="w-8 h-8 rounded-full object-cover border-2 border-white/40"
            />
            <div>
              <p className="text-xs font-semibold text-white m-0">Stephen P.</p>
              <p className="text-[11px] text-white/60 m-0">Tascha's dad · joined 2026</p>
            </div>
          </div>
        </div>

        {/* Pet strip */}
        <div className="flex mt-auto pt-8">
          <div className="flex items-center">
            {PET_IMAGES.map((src, i) => (
              <div
                key={i}
                className="w-[52px] h-[52px] rounded-full overflow-hidden border-[2.5px] border-white/50 shrink-0"
                style={{ marginLeft: i === 0 ? 0 : -14 }}
              >
                <img src={src} alt="" className="w-full h-full object-cover block" />
              </div>
            ))}
          </div>
          <p className="ml-3.5 text-[13px] text-white/75 self-center">
            Join a community of pet lovers already making their pets famous
          </p>
        </div>
      </div>

      {/* ── Form panel ── */}
      <div className="flex-1 flex flex-col items-center px-10 pt-20 pb-16 overflow-y-auto">

        {/* Back link */}
        <div className="w-full max-w-[420px]">
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

        <div className="w-full max-w-[420px]">
          {/* Mobile logo */}
          <div className="mobile-auth-header hidden items-center gap-2.5 mb-7">
            <BarkfestMark size={28} />
            <span className="font-heading text-[18px] font-bold">Barkfest</span>
          </div>

          {/* Heading */}
          <div className="mb-8">
            <h2 className="font-heading font-bold mb-1.5 text-[clamp(22px,2.5vw,30px)]">
              Create your account
            </h2>
            <p className="text-sm text-muted-foreground m-0">
              Already have one?{' '}
              <Link to="/login" className="text-primary font-semibold no-underline">Sign in</Link>
            </p>
          </div>

          <form onSubmit={handleSubmit} noValidate>

            {/* First / Last name */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 mb-[18px]">
              <div>
                <label htmlFor="r-fn" className="block text-[13px] font-semibold mb-1.5 text-foreground">
                  First name <Required />
                </label>
                <input
                  id="r-fn" name="firstName" type="text"
                  autoComplete="given-name" maxLength={LIMITS.firstName} autoFocus
                  placeholder="Jane"
                  value={form.firstName} onChange={handleChange}
                  className={inputCls}
                />
              </div>
              <div>
                <label htmlFor="r-ln" className="block text-[13px] font-semibold mb-1.5 text-foreground">
                  Last name <Required />
                </label>
                <input
                  id="r-ln" name="lastName" type="text"
                  autoComplete="family-name" maxLength={LIMITS.lastName}
                  placeholder="Doe"
                  value={form.lastName} onChange={handleChange}
                  className={inputCls}
                />
              </div>
            </div>

            {/* Email */}
            <div className="mb-[18px]">
              <label htmlFor="r-em" className="block text-[13px] font-semibold mb-1.5 text-foreground">
                Email <Required />
              </label>
              <input
                id="r-em" name="email" type="email"
                autoComplete="email" maxLength={LIMITS.email}
                placeholder="you@example.com"
                value={form.email} onChange={handleChange}
                className={inputCls}
              />
              {emailBad && <p className="text-xs text-destructive mt-1">Must be a valid email address.</p>}
            </div>

            {/* Username */}
            <div className="mb-[18px]">
              <label htmlFor="r-un" className="block text-[13px] font-semibold mb-1.5 text-foreground">
                Username <Required />
              </label>
              <input
                id="r-un" name="username" type="text"
                autoComplete="username" maxLength={LIMITS.username}
                placeholder="Pick a username"
                value={form.username} onChange={handleChange}
                className={inputCls}
              />
              {form.username.trim() && (
                unTooShort ? (
                  <p className="text-xs text-destructive mt-1">At least 5 characters required</p>
                ) : unStatus === 'checking' ? (
                  <p className="text-xs text-muted-foreground mt-1 flex items-center gap-1">
                    <Loader2 className="w-2.5 h-2.5 animate-spin" />Checking availability…
                  </p>
                ) : unStatus === 'available' ? (
                  <p className="text-xs text-[#1a7f4b] mt-1 flex items-center gap-1">
                    <Check className="w-3.5 h-3.5" />Available
                  </p>
                ) : unStatus === 'taken' ? (
                  <p className="text-xs text-destructive mt-1">Username not available</p>
                ) : null
              )}
            </div>

            {/* Display name */}
            <div className="mb-[18px]">
              <label htmlFor="r-dn" className="block text-[13px] font-semibold mb-1.5 text-foreground">
                Display name <Required />
              </label>
              <p className="text-xs text-muted-foreground mb-1.5 mt-0">
                Shown on your pet cards — e.g. "Cool Pet Dad"
              </p>
              <input
                id="r-dn" name="displayName" type="text"
                autoComplete="nickname" maxLength={LIMITS.displayName}
                placeholder="e.g. Cool Pet Dad"
                value={form.displayName} onChange={handleChange}
                className={inputCls}
              />
              {form.displayName.trim() && (
                dnTooShort ? (
                  <p className="text-xs text-destructive mt-1">At least 4 characters required</p>
                ) : dnStatus === 'checking' ? (
                  <p className="text-xs text-muted-foreground mt-1 flex items-center gap-1">
                    <Loader2 className="w-2.5 h-2.5 animate-spin" />Checking availability…
                  </p>
                ) : dnStatus === 'available' ? (
                  <p className="text-xs text-[#1a7f4b] mt-1 flex items-center gap-1">
                    <Check className="w-3.5 h-3.5" />Available
                  </p>
                ) : dnStatus === 'taken' ? (
                  <p className="text-xs text-destructive mt-1">Display name not available</p>
                ) : null
              )}
            </div>

            {/* Password */}
            <div className="mb-[18px]">
              <label htmlFor="r-pw" className="block text-[13px] font-semibold mb-1.5 text-foreground">
                Password <Required />
              </label>
              <div className="relative">
                <input
                  id="r-pw" name="password"
                  type={showPw ? 'text' : 'password'}
                  autoComplete="new-password" minLength={LIMITS.passwordMin} maxLength={LIMITS.passwordMax}
                  value={form.password} onChange={handleChange}
                  className={inputCls + ' pr-11'}
                />
                <button
                  type="button"
                  onClick={() => setShowPw(v => !v)}
                  aria-label={showPw ? 'Hide password' : 'Show password'}
                  className="absolute right-0 top-0 h-[46px] w-11 flex items-center justify-center bg-transparent border-0 cursor-pointer text-muted-foreground hover:text-foreground transition-colors"
                >
                  {showPw ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
              {form.password && <PasswordStrengthMeter score={strength.score} />}
              {pwWeak && (
                <p className="text-xs text-destructive mt-1">Try a longer or less predictable password.</p>
              )}
            </div>

            {/* Confirm password */}
            <div className="mb-[18px]">
              <label htmlFor="r-cpw" className="block text-[13px] font-semibold mb-1.5 text-foreground">
                Confirm password <Required />
              </label>
              <div className="relative">
                <input
                  id="r-cpw" name="confirmPassword"
                  type={showPw ? 'text' : 'password'}
                  autoComplete="new-password" maxLength={LIMITS.passwordMax}
                  value={form.confirmPassword} onChange={handleChange}
                  className={inputCls + ' pr-11'}
                />
                <button
                  type="button"
                  onClick={() => setShowPw(v => !v)}
                  aria-label={showPw ? 'Hide password' : 'Show password'}
                  className="absolute right-0 top-0 h-[46px] w-11 flex items-center justify-center bg-transparent border-0 cursor-pointer text-muted-foreground hover:text-foreground transition-colors"
                >
                  {showPw ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
              {pwMismatch && <p className="text-xs text-destructive mt-1">Passwords do not match.</p>}
            </div>

            {error && (
              <p className="text-[13px] text-destructive text-center mb-2.5">{error}</p>
            )}

            <div className="flex gap-3 mt-2">
              <button
                type="button"
                onClick={() => navigate('/')}
                disabled={isLoading}
                className="flex-1 h-[50px] rounded-[14px] border-[1.5px] border-border bg-transparent text-foreground text-[15px] font-medium cursor-pointer disabled:cursor-not-allowed disabled:opacity-40 transition-opacity"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={!canSubmit || isLoading}
                className="flex-1 h-[50px] rounded-[14px] border-0 bg-primary text-white text-[15px] font-semibold cursor-pointer flex items-center justify-center gap-2 disabled:cursor-not-allowed disabled:opacity-[0.45] transition-opacity"
              >
                {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
                {isLoading ? 'Creating your account…' : 'Create account'}
              </button>
            </div>

            <p className="text-center text-[13px] text-muted-foreground mt-4">
              By creating an account you agree to our Terms and Privacy Policy.
            </p>
          </form>
        </div>
      </div>
    </div>
  )
}

function Required() {
  return <span className="text-destructive ml-0.5">*</span>
}
