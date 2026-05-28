import { useCallback, useEffect, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { ArrowLeft, Check, Loader2, X } from 'lucide-react'
import zxcvbn from 'zxcvbn'
import { useAuth } from '@/hooks/useAuth'
import { checkDisplayName, login, register } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

const STRENGTH_LABELS = ['Very weak', 'Weak', 'Fair', 'Strong', 'Very strong']
const STRENGTH_COLORS = [
  'var(--destructive)',
  '#f97316',
  '#eab308',
  'var(--accent)',
  '#22c55e',
]

type DisplayNameStatus = 'idle' | 'checking' | 'available' | 'taken'

interface FormState {
  firstName: string
  lastName: string
  email: string
  username: string
  displayName: string
  password: string
  confirmPassword: string
  phoneNumber: string
}

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
    phoneNumber: '',
  })
  const [dnStatus, setDnStatus] = useState<DisplayNameStatus>('idle')
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const strength = zxcvbn(form.password)
  const emailValid = EMAIL_REGEX.test(form.email)
  const passwordsMatch = form.password === form.confirmPassword && form.confirmPassword !== ''
  const dnTrimmed = form.displayName.trim()
  const dnLongEnough = dnTrimmed.replace(/\s/g, '').length >= 4

  const canSubmit =
    form.firstName &&
    form.lastName &&
    form.email &&
    emailValid &&
    form.username &&
    dnTrimmed &&
    dnLongEnough &&
    dnStatus === 'available' &&
    form.password &&
    strength.score >= 2 &&
    passwordsMatch &&
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
        phoneNumber: form.phoneNumber || undefined,
        password: form.password,
        displayName: dnTrimmed,
      })
      const result = await login(form.username, form.password)
      signIn(result.accountId, 'owner', result.accessToken)
      navigate('/')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Registration failed.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex">
      {/* Brand panel — 42%, hidden on mobile */}
      <div
        className="hidden md:flex w-[42%] flex-col relative overflow-hidden"
        style={{ background: 'var(--primary)' }}
      >
        <div
          className="absolute -top-16 -right-16 w-72 h-72 rounded-full opacity-15"
          style={{ background: 'rgba(255,255,255,0.3)' }}
        />
        <div
          className="absolute -bottom-16 -left-16 w-64 h-64 rounded-full opacity-10"
          style={{ background: 'rgba(255,255,255,0.3)' }}
        />

        <div className="relative flex flex-col h-full px-10 py-10">
          <div className="flex items-center gap-2">
            <BarkfestMark inverted size={32} />
            <span className="font-heading text-xl font-bold text-white">Barkfest</span>
          </div>

          <div className="mt-auto">
            <h1 className="font-heading text-4xl font-bold text-white leading-tight mb-4">
              Every pet has<br />a story to tell.
            </h1>
            <p className="text-white/75 text-sm leading-relaxed max-w-xs mb-8">
              Join thousands of pet lovers sharing their furry friends' adventures with the world.
            </p>

            {/* Testimonial card */}
            <div
              className="rounded-2xl p-5 mb-6"
              style={{ background: 'rgba(255,255,255,0.15)' }}
            >
              <p className="text-white text-sm leading-relaxed italic mb-3">
                "Barkfest has been the perfect place to share Mochi's adventures. The community is so warm and welcoming!"
              </p>
              <div className="flex items-center gap-2">
                <div
                  className="w-8 h-8 rounded-full flex items-center justify-center text-sm"
                  style={{ background: 'rgba(255,255,255,0.25)' }}
                >
                  🐕
                </div>
                <span className="text-white/80 text-xs font-medium">Sarah & Mochi</span>
              </div>
            </div>

            {/* Overlapping thumbnails */}
            <div className="flex items-center mb-8">
              {['🐶', '🐱', '🐩', '🐈', '🦮'].map((emoji, i) => (
                <div
                  key={i}
                  className="w-9 h-9 rounded-full flex items-center justify-center text-base border-2 border-primary"
                  style={{
                    background: 'rgba(255,255,255,0.2)',
                    marginLeft: i === 0 ? 0 : '-14px',
                    zIndex: i,
                  }}
                >
                  {emoji}
                </div>
              ))}
              <span className="text-white/70 text-xs ml-3">2,400+ pets shared</span>
            </div>
          </div>

          <div className="flex gap-4">
            <a href="#" className="text-white/60 text-xs hover:text-white/90 transition-colors">Privacy Policy</a>
            <a href="#" className="text-white/60 text-xs hover:text-white/90 transition-colors">Terms of Use</a>
          </div>
        </div>
      </div>

      {/* Form panel — 58% */}
      <div className="flex-1 flex flex-col overflow-y-auto">
        <div className="px-8 pt-8">
          <Link
            to="/"
            className="inline-flex items-center gap-1.5 text-sm font-medium hover:opacity-70 transition-opacity"
            style={{ color: 'var(--muted-foreground)' }}
          >
            <ArrowLeft className="w-4 h-4" />
            Back to Barkfest
          </Link>
        </div>

        {/* Mobile logo */}
        <div className="flex md:hidden items-center justify-center gap-2 mt-6">
          <BarkfestMark size={28} />
          <span className="font-heading text-xl font-bold">Barkfest</span>
        </div>

        <div className="flex-1 flex items-start justify-center px-8 py-8">
          <div className="w-full max-w-[380px] space-y-5">
            <div className="space-y-1">
              <h2 className="font-heading text-3xl font-bold">Create account</h2>
              <p className="text-sm" style={{ color: 'var(--muted-foreground)' }}>
                Already have one?{' '}
                <Link to="/login" className="font-medium hover:underline" style={{ color: 'var(--primary)' }}>
                  Sign in →
                </Link>
              </p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
              {/* Name row */}
              <div className="grid grid-cols-2 gap-3">
                <Field label="First name" id="firstName" name="firstName" autoComplete="given-name" required value={form.firstName} onChange={handleChange} />
                <Field label="Last name" id="lastName" name="lastName" autoComplete="family-name" required value={form.lastName} onChange={handleChange} />
              </div>

              {/* Email */}
              <div className="space-y-1.5">
                <label className="text-sm font-medium" htmlFor="email">Email</label>
                <input
                  id="email"
                  name="email"
                  type="email"
                  autoComplete="email"
                  required
                  value={form.email}
                  onChange={handleChange}
                  className="w-full px-4 text-sm focus:outline-none focus:ring-2 transition-shadow"
                  style={{
                    height: '44px',
                    borderRadius: '12px',
                    border: `1px solid ${form.email && !emailValid ? 'var(--destructive)' : 'var(--border)'}`,
                    background: 'var(--background)',
                    color: 'var(--foreground)',
                  }}
                />
                {form.email && !emailValid && (
                  <p className="text-xs" style={{ color: 'var(--destructive)' }}>Enter a valid email address</p>
                )}
              </div>

              {/* Username */}
              <Field label="Username" id="username" name="username" autoComplete="username" required value={form.username} onChange={handleChange} />

              {/* Display name with availability check */}
              <div className="space-y-1.5">
                <label className="text-sm font-medium" htmlFor="displayName">Display name</label>
                <div className="relative">
                  <input
                    id="displayName"
                    name="displayName"
                    type="text"
                    autoComplete="off"
                    value={form.displayName}
                    onChange={handleChange}
                    placeholder="How the community sees you"
                    className="w-full px-4 pr-10 text-sm focus:outline-none focus:ring-2 transition-shadow placeholder:text-muted-foreground"
                    style={{
                      height: '44px',
                      borderRadius: '12px',
                      border: `1px solid ${dnStatus === 'taken' ? 'var(--destructive)' : dnStatus === 'available' ? '#22c55e' : 'var(--border)'}`,
                      background: 'var(--background)',
                      color: 'var(--foreground)',
                    }}
                  />
                  {dnStatus === 'checking' && (
                    <Loader2 className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 animate-spin text-muted-foreground" />
                  )}
                  {dnStatus === 'available' && (
                    <Check className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-green-500" />
                  )}
                  {dnStatus === 'taken' && (
                    <X className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-destructive" />
                  )}
                </div>
                {dnStatus === 'available' && (
                  <p className="text-xs text-green-600">Available</p>
                )}
                {dnStatus === 'taken' && (
                  <p className="text-xs" style={{ color: 'var(--destructive)' }}>Already taken</p>
                )}
              </div>

              {/* Password */}
              <div className="space-y-1.5">
                <label className="text-sm font-medium" htmlFor="password">Password</label>
                <input
                  id="password"
                  name="password"
                  type="password"
                  autoComplete="new-password"
                  required
                  value={form.password}
                  onChange={handleChange}
                  className="w-full px-4 text-sm focus:outline-none focus:ring-2 transition-shadow"
                  style={{
                    height: '44px',
                    borderRadius: '12px',
                    border: '1px solid var(--border)',
                    background: 'var(--background)',
                    color: 'var(--foreground)',
                  }}
                />
                {form.password && (
                  <div className="space-y-1 pt-0.5">
                    <div className="flex gap-1">
                      {[0, 1, 2, 3].map(i => (
                        <div
                          key={i}
                          className="h-1 flex-1 rounded-full transition-colors"
                          style={{
                            background: i < strength.score
                              ? STRENGTH_COLORS[strength.score]
                              : 'var(--border)',
                          }}
                        />
                      ))}
                    </div>
                    <p className="text-xs" style={{ color: 'var(--muted-foreground)' }}>
                      {STRENGTH_LABELS[strength.score]}
                    </p>
                  </div>
                )}
              </div>

              {/* Confirm password */}
              <div className="space-y-1.5">
                <label className="text-sm font-medium" htmlFor="confirmPassword">Confirm password</label>
                <input
                  id="confirmPassword"
                  name="confirmPassword"
                  type="password"
                  autoComplete="new-password"
                  required
                  value={form.confirmPassword}
                  onChange={handleChange}
                  className="w-full px-4 text-sm focus:outline-none focus:ring-2 transition-shadow"
                  style={{
                    height: '44px',
                    borderRadius: '12px',
                    border: `1px solid ${form.confirmPassword && !passwordsMatch ? 'var(--destructive)' : 'var(--border)'}`,
                    background: 'var(--background)',
                    color: 'var(--foreground)',
                  }}
                />
                {form.confirmPassword && !passwordsMatch && (
                  <p className="text-xs" style={{ color: 'var(--destructive)' }}>Passwords do not match</p>
                )}
              </div>

              {/* Phone (optional) */}
              <Field label="Phone (optional)" id="phoneNumber" name="phoneNumber" type="tel" autoComplete="tel" value={form.phoneNumber} onChange={handleChange} placeholder="+15555550100" />

              {error && <p className="text-sm" style={{ color: 'var(--destructive)' }}>{error}</p>}

              <button
                type="submit"
                disabled={!canSubmit}
                className="w-full flex items-center justify-center gap-2 text-sm font-semibold text-white transition-opacity hover:opacity-90 disabled:opacity-50 disabled:cursor-not-allowed"
                style={{
                  height: '50px',
                  borderRadius: '14px',
                  background: 'var(--primary)',
                }}
              >
                {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
                Create Account
              </button>

              <p className="text-xs text-center" style={{ color: 'var(--muted-foreground)' }}>
                By creating an account, you agree to our{' '}
                <a href="#" className="underline hover:opacity-70">Terms of Service</a>
                {' '}and{' '}
                <a href="#" className="underline hover:opacity-70">Privacy Policy</a>.
              </p>
            </form>
          </div>
        </div>
      </div>
    </div>
  )
}

interface FieldProps {
  label: string
  id: string
  name: string
  type?: string
  autoComplete?: string
  required?: boolean
  value: string
  onChange: (e: ChangeEvent<HTMLInputElement>) => void
  placeholder?: string
}

function Field({ label, id, name, type = 'text', autoComplete, required, value, onChange, placeholder }: FieldProps) {
  return (
    <div className="space-y-1.5">
      <label className="text-sm font-medium" htmlFor={id}>{label}</label>
      <input
        id={id}
        name={name}
        type={type}
        autoComplete={autoComplete}
        required={required}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        className="w-full px-4 text-sm focus:outline-none focus:ring-2 transition-shadow placeholder:text-muted-foreground"
        style={{
          height: '44px',
          borderRadius: '12px',
          border: '1px solid var(--border)',
          background: 'var(--background)',
          color: 'var(--foreground)',
        }}
      />
    </div>
  )
}
