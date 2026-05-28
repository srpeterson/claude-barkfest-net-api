import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { ArrowLeft, Loader2 } from 'lucide-react'
import { useAuth } from '@/hooks/useAuth'
import { login } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'

export function LoginPage() {
  const navigate = useNavigate()
  const { signIn } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setIsLoading(true)
    try {
      const result = await login(username, password)
      signIn(result.accountId, 'owner', result.accessToken)
      navigate('/')
    } catch {
      setError('Invalid username or password.')
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
        {/* Decorative blobs */}
        <div
          className="absolute -top-20 -left-20 w-80 h-80 rounded-full opacity-20"
          style={{ background: 'rgba(255,255,255,0.3)' }}
        />
        <div
          className="absolute -bottom-10 -right-10 w-64 h-64 rounded-full opacity-15"
          style={{ background: 'rgba(255,255,255,0.3)' }}
        />

        <div className="relative flex flex-col h-full px-10 py-10">
          {/* Logo */}
          <div className="flex items-center gap-2">
            <BarkfestMark inverted size={32} />
            <span className="font-heading text-xl font-bold text-white">Barkfest</span>
          </div>

          {/* Headline */}
          <div className="mt-auto mb-8">
            <h1 className="font-heading text-4xl font-bold text-white leading-tight mb-4">
              Good to have<br />you back.
            </h1>
            <p className="text-white/75 text-sm leading-relaxed max-w-xs">
              Sign in to manage your pets, update your profile, and stay connected with the community.
            </p>
          </div>

          {/* Pet photo mosaic */}
          <div className="grid grid-cols-2 gap-3 mb-8">
            {[
              { h: '140px', bg: 'rgba(255,255,255,0.15)' },
              { h: '110px', bg: 'rgba(255,255,255,0.1)' },
              { h: '110px', bg: 'rgba(255,255,255,0.1)' },
              { h: '140px', bg: 'rgba(255,255,255,0.15)' },
            ].map((cell, i) => (
              <div
                key={i}
                className="rounded-2xl flex items-center justify-center"
                style={{ height: cell.h, background: cell.bg }}
              >
                <span className="text-3xl opacity-60">🐾</span>
              </div>
            ))}
          </div>

          {/* Footer links */}
          <div className="flex gap-4">
            <a href="#" className="text-white/60 text-xs hover:text-white/90 transition-colors">Privacy Policy</a>
            <a href="#" className="text-white/60 text-xs hover:text-white/90 transition-colors">Terms of Use</a>
          </div>
        </div>
      </div>

      {/* Form panel — 58% */}
      <div className="flex-1 flex flex-col">
        {/* Back link */}
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

        {/* Mobile logo — only on small screens */}
        <div className="flex md:hidden items-center justify-center gap-2 mt-6">
          <BarkfestMark size={28} />
          <span className="font-heading text-xl font-bold">Barkfest</span>
        </div>

        {/* Centered form */}
        <div className="flex-1 flex items-center justify-center px-8 py-8">
          <div className="w-full max-w-[380px] space-y-6">
            <div className="space-y-1">
              <h2 className="font-heading text-3xl font-bold">Welcome back!</h2>
              <p className="text-sm" style={{ color: 'var(--muted-foreground)' }}>
                New here?{' '}
                <Link to="/register" className="font-medium hover:underline" style={{ color: 'var(--primary)' }}>
                  Create an account →
                </Link>
              </p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="space-y-1.5">
                <label className="text-sm font-medium" htmlFor="username">Username</label>
                <input
                  id="username"
                  type="text"
                  autoComplete="username"
                  required
                  value={username}
                  onChange={e => setUsername(e.target.value)}
                  className="w-full px-4 text-sm focus:outline-none focus:ring-2 transition-shadow"
                  style={{
                    height: '48px',
                    borderRadius: '12px',
                    border: '1px solid var(--border)',
                    background: 'var(--background)',
                    color: 'var(--foreground)',
                  }}
                />
              </div>

              <div className="space-y-1.5">
                <label className="text-sm font-medium" htmlFor="password">Password</label>
                <div className="relative">
                  <input
                    id="password"
                    type={showPassword ? 'text' : 'password'}
                    autoComplete="current-password"
                    required
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                    className="w-full px-4 pr-12 text-sm focus:outline-none focus:ring-2 transition-shadow"
                    style={{
                      height: '48px',
                      borderRadius: '12px',
                      border: '1px solid var(--border)',
                      background: 'var(--background)',
                      color: 'var(--foreground)',
                    }}
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(s => !s)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-xs font-medium"
                    style={{ color: 'var(--muted-foreground)' }}
                  >
                    {showPassword ? 'Hide' : 'Show'}
                  </button>
                </div>
              </div>

              {error && <p className="text-sm" style={{ color: 'var(--destructive)' }}>{error}</p>}

              <button
                type="submit"
                disabled={isLoading}
                className="w-full flex items-center justify-center gap-2 text-sm font-semibold text-white transition-opacity hover:opacity-90 disabled:opacity-50"
                style={{
                  height: '52px',
                  borderRadius: '14px',
                  background: 'var(--primary)',
                }}
              >
                {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
                Sign In
              </button>
            </form>

            {/* Divider */}
            <div className="flex items-center gap-3">
              <div className="flex-1 border-t" style={{ borderColor: 'var(--border)' }} />
              <span className="text-xs" style={{ color: 'var(--muted-foreground)' }}>or continue with</span>
              <div className="flex-1 border-t" style={{ borderColor: 'var(--border)' }} />
            </div>

            {/* OAuth placeholders */}
            <div className="flex gap-3">
              {['Google', 'Apple'].map(provider => (
                <button
                  key={provider}
                  type="button"
                  disabled
                  className="flex-1 flex items-center justify-center gap-2 text-sm font-medium transition-opacity opacity-50 cursor-not-allowed"
                  style={{
                    height: '46px',
                    borderRadius: '12px',
                    border: '1px solid var(--border)',
                    background: 'var(--card)',
                    color: 'var(--foreground)',
                  }}
                >
                  {provider}
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
