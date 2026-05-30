import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react'
import isEmail from 'validator/lib/isEmail'
import { Eye, EyeOff, Loader2, X } from 'lucide-react'
import zxcvbn from 'zxcvbn'
import { useAuth } from '@/hooks/useAuth'
import { ApiError, checkDisplayName, getOwnerById, login, register, setAuthToken } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'

const STRENGTH_LABELS = ['Very weak', 'Weak', 'Fair', 'Strong', 'Very strong']
const STRENGTH_COLORS = [
  'bg-destructive',
  'bg-orange-400',
  'bg-yellow-400',
  'bg-accent',
  'bg-green-500',
]

export function RegisterDialog() {
  const { dialog, closeDialog, openLoginDialog, signIn } = useAuth()
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    username: '',
    displayName: '',
    password: '',
    confirmPassword: '',
  })
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [displayNameAvailable, setDisplayNameAvailable] = useState<boolean | null>(null)
  const [displayNameChecking, setDisplayNameChecking] = useState(false)

  const strength = zxcvbn(form.password)
  const displayNameStripped = form.displayName.replace(/\s/g, '')
  const displayNameTooShort = displayNameStripped.length > 0 && displayNameStripped.length < 4
  const emailInvalid = form.email.trim() !== '' && !isEmail(form.email.trim())
  const allFieldsFilled = form.firstName.trim() !== '' && form.lastName.trim() !== '' &&
    form.email.trim() !== '' && !emailInvalid && form.username.trim() !== '' && form.displayName.trim() !== '' &&
    form.password !== '' && form.confirmPassword !== ''
  const passwordMismatch = form.confirmPassword !== '' && form.password !== form.confirmPassword
  const passwordTooWeak = form.password.length > 0 && strength.score < 2
  const hint = passwordTooWeak
    ? (strength.feedback.suggestions[0] ?? 'Try mixing words, numbers, and symbols.')
    : null

  useEffect(() => {
    if (dialog !== 'register') {
      setForm({ firstName: '', lastName: '', email: '', username: '', displayName: '', password: '', confirmPassword: '' })
      setShowPassword(false)
      setError(null)
      setDisplayNameAvailable(null)
      setDisplayNameChecking(false)
    }
  }, [dialog])

  useEffect(() => {
    if (!form.displayName.trim() || displayNameTooShort) {
      setDisplayNameAvailable(null)
      setDisplayNameChecking(false)
      return
    }

    setDisplayNameAvailable(null)

    const timer = setTimeout(async () => {
      setDisplayNameChecking(true)
      try {
        const available = await checkDisplayName(form.displayName)
        setDisplayNameAvailable(available)
      } catch {
        setDisplayNameAvailable(null)
      } finally {
        setDisplayNameChecking(false)
      }
    }, 500)

    return () => clearTimeout(timer)
  }, [form.displayName])

  if (dialog !== 'register') return null

  function handleChange(e: ChangeEvent<HTMLInputElement>) {
    setForm(f => ({ ...f, [e.target.name]: e.target.value }))
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setIsLoading(true)
    try {
      await register({
        username: form.username.trim(),
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim().toLowerCase(),
        password: form.password,
        displayName: form.displayName.trim(),
      })
      const result = await login(form.username, form.password)

      let profileImageBlobName: string | null = null
      try {
        setAuthToken(result.accessToken)
        const owner = await getOwnerById(result.accountId)
        profileImageBlobName = owner.profileImage?.blobName ?? null
      } catch {
        // Non-fatal — new owner will always have null profile image
      }

      signIn(result.accountId, 'owner', result.accessToken, profileImageBlobName)
      closeDialog()
    } catch (err) {
      if (err instanceof ApiError && err.status < 500) {
        setError(err.message)
      } else {
        setError('Woof! Something went wrong. Please try again.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4"
    >
      <div
        className="relative w-full max-w-sm bg-card rounded-3xl shadow-2xl p-8"
      >
        <button
          onClick={closeDialog}
          className="absolute top-4 right-4 text-muted-foreground hover:text-foreground transition-colors"
        >
          <X className="w-5 h-5" />
        </button>

        <div className="mb-6">
          <div className="flex items-center gap-2 mb-4">
            <BarkfestMark size={22} />
            <span className="font-heading font-bold" style={{ fontSize: '17px' }}>Barkfest</span>
          </div>
          <h2 className="text-2xl font-bold">Welcome to Barkfest!</h2>
          <p className="text-sm text-muted-foreground mt-1">Create your account to get started.</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <DialogField label="First name" id="reg-firstName" name="firstName" autoComplete="given-name" autoFocus placeholder="Jane" required maxLength={30} value={form.firstName} onChange={handleChange} />
            <DialogField label="Last name" id="reg-lastName" name="lastName" autoComplete="family-name" placeholder="Doe" required maxLength={50} value={form.lastName} onChange={handleChange} />
          </div>

          <div className="space-y-1">
            <DialogField label="Email" id="reg-email" name="email" type="email" autoComplete="email" placeholder="you@example.com" required maxLength={75} value={form.email} onChange={handleChange} />
            {emailInvalid && (
              <p className="text-xs text-destructive">Must be a valid email address.</p>
            )}
          </div>

          <DialogField label="Username" id="reg-username" name="username" autoComplete="username" placeholder="Pick a username" required maxLength={25} value={form.username} onChange={handleChange} />

          <div className="space-y-1">
            <DialogField label="Display name" id="reg-displayName" name="displayName" autoComplete="nickname" placeholder="e.g. Cool Pet Dad" required maxLength={25} value={form.displayName} onChange={handleChange} />
            {form.displayName.trim() && (
              displayNameTooShort ? (
                <p className="text-xs text-destructive">At least 4 characters required</p>
              ) : (displayNameChecking || displayNameAvailable !== null) ? (
                <p className={`text-xs flex items-center gap-1 ${
                  displayNameChecking ? 'text-muted-foreground' :
                  displayNameAvailable ? 'text-green-500' : 'text-destructive'
                }`}>
                  {displayNameChecking && <Loader2 className="w-3 h-3 animate-spin" />}
                  {displayNameChecking ? 'Checking…' :
                   displayNameAvailable ? '✓ Available' : 'Already taken'}
                </p>
              ) : null
            )}
          </div>

          <div className="space-y-1.5">
            <label className="text-sm font-medium" htmlFor="reg-password">
              Password <span className="text-destructive">*</span>
            </label>
            <div className="relative">
              <input
                id="reg-password"
                name="password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="new-password"
                required
                minLength={10}
                maxLength={50}
                value={form.password}
                onChange={handleChange}
                className="w-full h-11 rounded-xl border border-input bg-background px-3 pr-10 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
              />
              <button
                type="button"
                onClick={() => setShowPassword(v => !v)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
              >
                {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              </button>
            </div>
            {form.password && (
              <div className="space-y-1 pt-0.5">
                <div className="flex gap-1">
                  {[0, 1, 2, 3].map(i => (
                    <div
                      key={i}
                      className={`h-1 flex-1 rounded-full transition-colors ${
                        i < strength.score ? STRENGTH_COLORS[strength.score] : 'bg-border'
                      }`}
                    />
                  ))}
                </div>
                <p className="text-xs text-muted-foreground">{STRENGTH_LABELS[strength.score]}</p>
                {hint && <p className="text-xs text-destructive">{hint}</p>}
              </div>
            )}
          </div>

          <div className="space-y-1.5">
            <label className="text-sm font-medium" htmlFor="reg-confirmPassword">
              Confirm password <span className="text-destructive">*</span>
            </label>
            <div className="relative">
              <input
                id="reg-confirmPassword"
                name="confirmPassword"
                type={showPassword ? 'text' : 'password'}
                autoComplete="new-password"
                required
                maxLength={50}
                value={form.confirmPassword}
                onChange={handleChange}
                className="w-full h-11 rounded-xl border border-input bg-background px-3 pr-10 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
              />
              <button
                type="button"
                onClick={() => setShowPassword(v => !v)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
              >
                {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              </button>
            </div>
            {passwordMismatch && (
              <p className="text-xs text-destructive">Passwords do not match.</p>
            )}
          </div>

          {error && <p className="text-sm text-destructive">{error}</p>}

          <button
            type="submit"
            disabled={isLoading || !allFieldsFilled || passwordTooWeak || passwordMismatch || displayNameTooShort || displayNameChecking || displayNameAvailable !== true}
            className="w-full h-11 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-50 flex items-center justify-center gap-2"
          >
            {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
            Join the barkfest!
          </button>
        </form>

        <p className="text-center text-sm text-muted-foreground mt-5">
          Already have an account?{' '}
          <button
            onClick={openLoginDialog}
            className="text-primary font-medium hover:underline"
          >
            Sign in
          </button>
        </p>
      </div>
    </div>
  )
}

interface DialogFieldProps {
  label: string
  id: string
  name: string
  type?: string
  autoComplete?: string
  autoFocus?: boolean
  required?: boolean
  maxLength?: number
  value: string
  onChange: (e: ChangeEvent<HTMLInputElement>) => void
  placeholder?: string
}

function DialogField({ label, id, name, type = 'text', autoComplete, autoFocus, required, maxLength, value, onChange, placeholder }: DialogFieldProps) {
  return (
    <div className="space-y-1.5">
      <label className="text-sm font-medium" htmlFor={id}>
        {label} {required && <span className="text-destructive">*</span>}
      </label>
      <input
        id={id}
        name={name}
        type={type}
        autoComplete={autoComplete}
        autoFocus={autoFocus}
        required={required}
        maxLength={maxLength}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        className="w-full h-11 rounded-xl border border-input bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40 placeholder:text-muted-foreground"
      />
    </div>
  )
}
