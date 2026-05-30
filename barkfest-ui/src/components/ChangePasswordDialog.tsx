import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Eye, EyeOff, Loader2, X } from 'lucide-react'
import zxcvbn from 'zxcvbn'
import { useAuth } from '@/hooks/useAuth'
import { changePassword, logout } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'

const STRENGTH_LABELS = ['Very weak', 'Weak', 'Fair', 'Strong', 'Very strong']
const STRENGTH_COLORS = ['#e5484d', '#f76b15', '#d4a017', 'var(--accent)', 'var(--accent)']

// Shared input style matching the reference modal inputs
const INPUT: React.CSSProperties = {
  width: '100%',
  height: 44,
  border: '1.5px solid var(--border)',
  borderRadius: 12,
  background: 'var(--background)',
  color: 'var(--foreground)',
  padding: '0 42px 0 12px',
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

interface ChangePasswordDialogProps {
  onClose: () => void
}

export function ChangePasswordDialog({ onClose }: ChangePasswordDialogProps) {
  const navigate = useNavigate()
  const { accountId, signOut } = useAuth()

  const [current, setCurrent]     = useState('')
  const [next, setNext]           = useState('')
  const [confirm, setConfirm]     = useState('')
  const [showCurrent, setShowCurrent] = useState(false)
  const [showNew, setShowNew]     = useState(false)
  const [error, setError]         = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const strength      = zxcvbn(next)
  const pwWeak        = next.length > 0 && strength.score < 2
  const pwMismatch    = confirm !== '' && next !== confirm
  const allFilled     = current !== '' && next !== '' && confirm !== ''
  const canSubmit     = allFilled && !pwWeak && !pwMismatch && !isLoading

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!canSubmit || !accountId) return
    setError(null)
    setIsLoading(true)
    try {
      // TODO: Backend endpoint not yet implemented. Wire when available.
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
          borderRadius: 24,
          padding: 32,
          boxShadow: '0 32px 80px rgba(0,0,0,0.22)',
        }}
      >
        {/* Close */}
        <button
          onClick={onClose}
          disabled={isLoading}
          style={{
            position: 'absolute', top: 14, right: 14,
            width: 32, height: 32, borderRadius: 8,
            border: 'none', background: 'transparent', cursor: 'pointer',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: 'var(--muted-foreground)',
          }}
        >
          <X className="w-5 h-5" />
        </button>

        {/* Brand */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 18 }}>
          <BarkfestMark size={22} />
          <span className="font-heading font-bold" style={{ fontSize: 17 }}>Barkfest</span>
        </div>

        <h2 className="font-heading font-bold" style={{ fontSize: 24, marginBottom: 4 }}>Change password</h2>
        <p style={{ fontSize: 13.5, color: 'var(--muted-foreground)', marginBottom: 22 }}>
          You'll be signed out after updating your password.
        </p>

        <form onSubmit={handleSubmit} noValidate>

          {/* Current password */}
          <div style={{ marginBottom: 14 }}>
            <label style={LABEL}>Current password <Req /></label>
            <div style={{ position: 'relative' }}>
              <input
                id="cp-cur"
                type={showCurrent ? 'text' : 'password'}
                autoComplete="current-password"
                value={current}
                onChange={e => setCurrent(e.target.value)}
                style={INPUT}
                onFocus={focusIn}
                onBlur={focusOut}
              />
              <EyeBtn show={showCurrent} onToggle={() => setShowCurrent(v => !v)} />
            </div>
          </div>

          {/* New password */}
          <div style={{ marginBottom: 14 }}>
            <label style={LABEL}>New password <Req /></label>
            <div style={{ position: 'relative' }}>
              <input
                id="cp-new"
                type={showNew ? 'text' : 'password'}
                autoComplete="new-password"
                value={next}
                onChange={e => setNext(e.target.value)}
                style={INPUT}
                onFocus={focusIn}
                onBlur={focusOut}
              />
              <EyeBtn show={showNew} onToggle={() => setShowNew(v => !v)} />
            </div>
            {next && (
              <div style={{ marginTop: 6 }}>
                <div style={{ display: 'flex', gap: 3, marginBottom: 3 }}>
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
            {pwWeak && strength.feedback.suggestions[0] && (
              <p style={{ fontSize: 12, color: 'var(--destructive)', margin: '4px 0 0' }}>
                {strength.feedback.suggestions[0]}
              </p>
            )}
          </div>

          {/* Confirm new password */}
          <div style={{ marginBottom: 14 }}>
            <label style={LABEL}>Confirm new password <Req /></label>
            <div style={{ position: 'relative' }}>
              <input
                id="cp-confirm"
                type={showNew ? 'text' : 'password'}
                autoComplete="new-password"
                value={confirm}
                onChange={e => setConfirm(e.target.value)}
                style={INPUT}
                onFocus={focusIn}
                onBlur={focusOut}
              />
              <EyeBtn show={showNew} onToggle={() => setShowNew(v => !v)} />
            </div>
            {pwMismatch && (
              <p style={{ fontSize: 12, color: 'var(--destructive)', margin: '4px 0 0' }}>
                Passwords do not match.
              </p>
            )}
          </div>

          {error && (
            <p style={{ fontSize: 13, color: 'var(--destructive)', textAlign: 'center', marginBottom: 10 }}>
              {error}
            </p>
          )}

          <div style={{ display: 'flex', gap: 8, marginTop: 4 }}>
            <button
              type="button"
              onClick={onClose}
              disabled={isLoading}
              style={{
                flex: 1, height: 44, borderRadius: 12,
                border: '1.5px solid var(--border)',
                background: 'transparent',
                color: 'var(--muted-foreground)',
                fontFamily: "'DM Sans', sans-serif", fontSize: 14, fontWeight: 500,
                cursor: isLoading ? 'not-allowed' : 'pointer',
                opacity: isLoading ? 0.5 : 1,
              }}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={!canSubmit}
              style={{
                flex: 1, height: 44, borderRadius: 12,
                border: 'none',
                background: 'var(--primary)', color: '#fff',
                fontFamily: "'DM Sans', sans-serif", fontSize: 14, fontWeight: 600,
                cursor: !canSubmit ? 'not-allowed' : 'pointer',
                opacity: !canSubmit ? 0.5 : 1,
                display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 8,
                transition: 'opacity 0.15s',
              }}
            >
              {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
              Update password
            </button>
          </div>

          <p style={{ textAlign: 'center', marginTop: 12, fontSize: 13 }}>
            <button
              type="button"
              style={{
                background: 'none', border: 'none', cursor: 'pointer',
                color: 'var(--primary)', fontWeight: 600, fontSize: 13,
                fontFamily: "'DM Sans', sans-serif",
              }}
            >
              Forgot password?
            </button>
          </p>
        </form>
      </div>
    </div>
  )
}

const LABEL: React.CSSProperties = {
  display: 'block', fontSize: 13, fontWeight: 600,
  marginBottom: 6, color: 'var(--foreground)',
}

function Req() {
  return <span style={{ color: 'var(--destructive)', marginLeft: 2 }}>*</span>
}

function EyeBtn({ show, onToggle }: { show: boolean; onToggle: () => void }) {
  return (
    <button
      type="button"
      onClick={onToggle}
      style={{
        position: 'absolute', right: 0, top: 0,
        height: 44, width: 42,
        background: 'none', border: 'none', cursor: 'pointer',
        color: 'var(--muted-foreground)',
        display: 'flex', alignItems: 'center', justifyContent: 'center',
      }}
    >
      {show ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
    </button>
  )
}
