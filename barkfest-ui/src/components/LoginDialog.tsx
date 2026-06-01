import { useState } from 'react'
import { Eye, EyeOff, Loader2, X } from 'lucide-react'
import { useAuth } from '@/hooks/useAuth'
import { adminLogin, getOwnerById, login, setAuthToken } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'

export function LoginDialog() {
  const { dialog } = useAuth()
  if (dialog !== 'login') return null
  return <LoginDialogInner />
}

function LoginDialogInner() {
  const { closeDialog, signIn } = useAuth()
  const [form, setForm] = useState({ username: '', password: '', showPassword: false, error: null as string | null })
  const [isLoading, setIsLoading] = useState(false)
  const isAdmin = false

  const allFieldsFilled = form.username.trim() !== '' && form.password !== ''

  async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    setForm(f => ({ ...f, error: null }))
    setIsLoading(true)
    try {
      const result = isAdmin
        ? await adminLogin(form.username, form.password)
        : await login(form.username, form.password)

      let profileImageBlobName: string | null = null
      if (!isAdmin) {
        try {
          setAuthToken(result.accessToken)
          const owner = await getOwnerById(result.accountId)
          profileImageBlobName = owner.profileImage?.blobName ?? null
        } catch {
          // Non-fatal — proceed with null profile image
        }
      }

      signIn(result.accountId, isAdmin ? 'admin' : 'owner', result.accessToken, profileImageBlobName)
      closeDialog()
    } catch {
      setForm(f => ({ ...f, error: 'Invalid username or password.' }))
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
              value={form.username}
              onChange={e => setForm(f => ({ ...f, username: e.target.value }))}
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
                type={form.showPassword ? 'text' : 'password'}
                autoComplete="current-password"
                required
                maxLength={50}
                value={form.password}
                onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
                className="w-full h-11 rounded-xl border border-input bg-background px-3 pr-10 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
              />
              <button
                type="button"
                onClick={() => setForm(f => ({ ...f, showPassword: !f.showPassword }))}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
              >
                {form.showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              </button>
            </div>
          </div>

          {/* Admin checkbox — visible but disabled per design spec (admin login via Scalar) */}
          <label style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 16, opacity: 0.4, cursor: 'not-allowed', userSelect: 'none' }}>
            <input
              type="checkbox"
              disabled
              style={{ width: 16, height: 16, borderRadius: 4, accentColor: 'var(--primary)', cursor: 'not-allowed' }}
            />
            <span className="text-sm text-muted-foreground">I am an Administrator</span>
          </label>

          {form.error && <p className="text-sm text-destructive">{form.error}</p>}

          <button
            type="submit"
            disabled={isLoading || !allFieldsFilled}
            className="w-full h-11 rounded-xl bg-primary text-primary-foreground text-sm font-medium hover:opacity-90 transition-opacity disabled:opacity-50 flex items-center justify-center gap-2"
          >
            {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
            Sign In
          </button>
        </form>

      </div>
    </div>
  )
}
