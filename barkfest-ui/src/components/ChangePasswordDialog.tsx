import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Loader2, X } from 'lucide-react'
import zxcvbn from 'zxcvbn'
import { useAuth } from '@/hooks/useAuth'
import { changePassword, logout } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'

const STRENGTH_LABELS = ['Very weak', 'Weak', 'Fair', 'Strong', 'Very strong']
const STRENGTH_COLORS = [
  'var(--destructive)',
  '#f97316',
  '#eab308',
  'var(--accent)',
  '#22c55e',
]

interface ChangePasswordDialogProps {
  onClose: () => void
}

export function ChangePasswordDialog({ onClose }: ChangePasswordDialogProps) {
  const navigate = useNavigate()
  const { accountId, signOut } = useAuth()
  const [current, setCurrent] = useState('')
  const [next, setNext] = useState('')
  const [confirm, setConfirm] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const strength = zxcvbn(next)
  const passwordsMatch = next === confirm && confirm !== ''
  const canSubmit = current && next && strength.score >= 2 && passwordsMatch && !isLoading

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!canSubmit || !accountId) return
    setError(null)
    setIsLoading(true)
    try {
      await changePassword(accountId, current, next)
      await logout()
      signOut()
      navigate('/login')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to change password.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div
      className="fixed inset-0 z-[60] flex items-center justify-center p-4 animate-backdrop-in"
      style={{ background: 'rgba(0,0,0,0.5)', backdropFilter: 'blur(4px)' }}
    >
      <div
        className="relative w-full max-w-sm animate-dialog-appear"
        style={{
          background: 'var(--card)',
          borderRadius: '24px',
          padding: '32px',
          boxShadow: '0 32px 80px rgba(0,0,0,0.22)',
        }}
      >
        <button
          onClick={onClose}
          disabled={isLoading}
          className="absolute top-4 right-4 p-1 rounded-full hover:bg-secondary transition-colors disabled:opacity-50"
        >
          <X className="w-5 h-5 text-muted-foreground" />
        </button>

        {/* Header */}
        <div className="flex items-center gap-2 mb-5">
          <BarkfestMark size={22} />
          <span className="font-heading font-bold" style={{ fontSize: '17px' }}>Barkfest</span>
        </div>

        <h2 className="font-heading font-bold text-xl mb-1">Change password</h2>
        <p className="text-sm mb-6" style={{ color: 'var(--muted-foreground)' }}>
          You'll be signed out after changing your password.
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1.5">
            <label className="text-sm font-medium" htmlFor="cp-current">Current password</label>
            <input
              id="cp-current"
              type="password"
              autoComplete="current-password"
              required
              value={current}
              onChange={e => setCurrent(e.target.value)}
              className="w-full px-4 text-sm focus:outline-none focus:ring-2 transition-shadow"
              style={{
                height: '44px',
                borderRadius: '12px',
                border: '1px solid var(--border)',
                background: 'var(--background)',
                color: 'var(--foreground)',
              }}
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-sm font-medium" htmlFor="cp-new">New password</label>
            <input
              id="cp-new"
              type="password"
              autoComplete="new-password"
              required
              value={next}
              onChange={e => setNext(e.target.value)}
              className="w-full px-4 text-sm focus:outline-none focus:ring-2 transition-shadow"
              style={{
                height: '44px',
                borderRadius: '12px',
                border: '1px solid var(--border)',
                background: 'var(--background)',
                color: 'var(--foreground)',
              }}
            />
            {next && (
              <div className="space-y-1 pt-0.5">
                <div className="flex gap-1">
                  {[0, 1, 2, 3].map(i => (
                    <div
                      key={i}
                      className="h-1 flex-1 rounded-full transition-colors"
                      style={{ background: i < strength.score ? STRENGTH_COLORS[strength.score] : 'var(--border)' }}
                    />
                  ))}
                </div>
                <p className="text-xs" style={{ color: 'var(--muted-foreground)' }}>
                  {STRENGTH_LABELS[strength.score]}
                </p>
              </div>
            )}
          </div>

          <div className="space-y-1.5">
            <label className="text-sm font-medium" htmlFor="cp-confirm">Confirm new password</label>
            <input
              id="cp-confirm"
              type="password"
              autoComplete="new-password"
              required
              value={confirm}
              onChange={e => setConfirm(e.target.value)}
              className="w-full px-4 text-sm focus:outline-none focus:ring-2 transition-shadow"
              style={{
                height: '44px',
                borderRadius: '12px',
                border: `1px solid ${confirm && !passwordsMatch ? 'var(--destructive)' : 'var(--border)'}`,
                background: 'var(--background)',
                color: 'var(--foreground)',
              }}
            />
            {confirm && !passwordsMatch && (
              <p className="text-xs" style={{ color: 'var(--destructive)' }}>Passwords do not match</p>
            )}
          </div>

          {error && <p className="text-sm" style={{ color: 'var(--destructive)' }}>{error}</p>}

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              disabled={isLoading}
              className="flex-1 h-11 rounded-xl text-sm font-medium transition-colors hover:bg-secondary disabled:opacity-50"
              style={{ border: '1.5px solid var(--border)', background: 'transparent', color: 'var(--muted-foreground)' }}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={!canSubmit}
              className="flex-1 h-11 rounded-xl text-sm font-semibold text-white flex items-center justify-center gap-2 transition-opacity hover:opacity-90 disabled:opacity-50 disabled:cursor-not-allowed"
              style={{ background: 'var(--primary)' }}
            >
              {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
              Change password
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
