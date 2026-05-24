import { useState, type ChangeEvent, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Loader2, PawPrint } from 'lucide-react'
import zxcvbn from 'zxcvbn'
import { useAuth } from '@/hooks/useAuth'
import { login, register } from '@/lib/api'

const STRENGTH_LABELS = ['Very weak', 'Weak', 'Fair', 'Strong', 'Very strong']
const STRENGTH_COLORS = [
  'bg-destructive',
  'bg-orange-400',
  'bg-yellow-400',
  'bg-accent',
  'bg-green-500',
]

interface FormState {
  username: string
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  password: string
}

export function RegisterPage() {
  const navigate = useNavigate()
  const { signIn } = useAuth()
  const [form, setForm] = useState<FormState>({
    username: '',
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    password: '',
  })
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const strength = zxcvbn(form.password)

  function handleChange(e: ChangeEvent<HTMLInputElement>) {
    setForm(f => ({ ...f, [e.target.name]: e.target.value }))
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
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
      })
      const result = await login(form.username, form.password)
      signIn(result.accountId, 'owner')
      navigate('/owners')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Registration failed.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-background flex items-center justify-center px-4 py-12 relative overflow-hidden">
      <div className="absolute top-20 left-10 w-72 h-72 bg-primary/10 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-20 right-10 w-56 h-56 bg-accent/10 rounded-full blur-3xl pointer-events-none" />

      <div className="relative w-full max-w-sm">
        <div className="flex flex-col items-center gap-3 mb-8">
          <Link to="/" className="flex items-center gap-2 hover:opacity-80 transition-opacity">
            <PawPrint className="w-8 h-8 text-primary" />
            <span className="font-heading text-2xl font-semibold tracking-tight">Barkfest</span>
          </Link>
          <h1 className="font-heading text-3xl font-bold">Create account</h1>
          <p className="text-sm text-muted-foreground">Join the Barkfest community</p>
        </div>

        <div className="bg-card border border-border rounded-2xl shadow-sm p-8 space-y-5">
          <form onSubmit={handleSubmit} className="space-y-4">
            <Field label="Username" id="username" name="username" autoComplete="username" required value={form.username} onChange={handleChange} />

            <div className="grid grid-cols-2 gap-3">
              <Field label="First name" id="firstName" name="firstName" autoComplete="given-name" required value={form.firstName} onChange={handleChange} />
              <Field label="Last name" id="lastName" name="lastName" autoComplete="family-name" required value={form.lastName} onChange={handleChange} />
            </div>

            <Field label="Email" id="email" name="email" type="email" autoComplete="email" required value={form.email} onChange={handleChange} />

            <Field label="Phone (optional)" id="phoneNumber" name="phoneNumber" type="tel" autoComplete="tel" value={form.phoneNumber} onChange={handleChange} placeholder="+15555550100" />

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
                className="w-full h-10 rounded-lg border border-input bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
              />
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
                </div>
              )}
            </div>

            {error && <p className="text-sm text-destructive">{error}</p>}

            <button
              type="submit"
              disabled={isLoading}
              className="w-full h-10 rounded-lg bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-50 flex items-center justify-center gap-2"
            >
              {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
              Create Account
            </button>
          </form>

          <p className="text-center text-sm text-muted-foreground">
            Already have an account?{' '}
            <Link to="/login" className="text-primary font-medium hover:underline">
              Sign in
            </Link>
          </p>
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
        className="w-full h-10 rounded-lg border border-input bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40 placeholder:text-muted-foreground"
      />
    </div>
  )
}
