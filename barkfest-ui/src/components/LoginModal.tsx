import { useEffect, useState } from 'react'
import { Eye, EyeOff, Loader2, PawPrint, X } from 'lucide-react'
import { useAuth } from '@/hooks/useAuth'
import { adminLogin, login } from '@/lib/api'

export function LoginModal() {
  const { modal, closeModal, openRegisterModal, signIn } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [isAdmin, setIsAdmin] = useState(false)
  const [isLoading, setIsLoading] = useState(false)

  const allFieldsFilled = username.trim() !== '' && password !== ''

  useEffect(() => {
    if (modal !== 'login') {
      setUsername('')
      setPassword('')
      setShowPassword(false)
      setError(null)
    }
  }, [modal])

  if (modal !== 'login') return null

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setIsLoading(true)
    try {
      const result = isAdmin
        ? await adminLogin(username, password)
        : await login(username, password)
      signIn(result.accountId, isAdmin ? 'admin' : 'owner', result.accessToken)
      closeModal()
    } catch {
      setError('Invalid username or password.')
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
          onClick={closeModal}
          className="absolute top-4 right-4 text-muted-foreground hover:text-foreground transition-colors"
        >
          <X className="w-5 h-5" />
        </button>

        <div className="mb-6">
          <div className="flex items-center gap-2 mb-4">
            <PawPrint className="w-6 h-6 text-primary" />
            <span className="font-heading text-lg font-semibold tracking-tight">Barkfest</span>
          </div>
          <h2 className="font-heading text-2xl font-bold">Welcome back!</h2>
          <p className="text-sm text-muted-foreground mt-1">Sign in to share your pet's story.</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1.5">
            <label className="text-sm font-medium" htmlFor="login-username">
              Username <span className="text-destructive">*</span>
            </label>
            <input
              id="login-username"
              type="text"
              autoComplete="username"
              placeholder="Your username"
              required
              maxLength={25}
              value={username}
              onChange={e => setUsername(e.target.value)}
              className="w-full h-11 rounded-xl border border-input bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40 placeholder:text-muted-foreground"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-sm font-medium" htmlFor="login-password">
              Password <span className="text-destructive">*</span>
            </label>
            <div className="relative">
              <input
                id="login-password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="current-password"
                required
                maxLength={50}
                value={password}
                onChange={e => setPassword(e.target.value)}
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
          </div>

          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={isAdmin}
              onChange={e => setIsAdmin(e.target.checked)}
              className="w-4 h-4 rounded border-input accent-primary cursor-pointer"
            />
            <span className="text-sm text-muted-foreground">I am an Administrator</span>
          </label>

          {error && <p className="text-sm text-destructive">{error}</p>}

          <button
            type="submit"
            disabled={isLoading || !allFieldsFilled}
            className="w-full h-11 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-50 flex items-center justify-center gap-2"
          >
            {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
            Sign In
          </button>
        </form>

        {!isAdmin && (
          <p className="text-center text-sm text-muted-foreground mt-5">
            Don't have an account?{' '}
            <button
              onClick={openRegisterModal}
              className="text-primary font-medium hover:underline"
            >
              Join the barkfest!
            </button>
          </p>
        )}
      </div>
    </div>
  )
}
