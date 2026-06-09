import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Eye, EyeOff, Loader2, X } from 'lucide-react'
import zxcvbn from 'zxcvbn'
import { cn } from '@/lib/utils'
import { useAuth } from '@/hooks/useAuth'
import { ApiError, changePassword, logout } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'

const STRENGTH_LABELS = ['Very weak', 'Weak', 'Fair', 'Strong', 'Very strong']
const STRENGTH_BG     = ['bg-[#e5484d]', 'bg-[#f76b15]', 'bg-[#d4a017]', 'bg-accent', 'bg-accent']
const STRENGTH_TEXT   = ['text-[#e5484d]', 'text-[#f76b15]', 'text-[#d4a017]', 'text-accent', 'text-accent']

const inputCls = [
  'w-full h-11 rounded-xl border-[1.5px] border-border',
  'bg-background text-foreground pl-3 pr-11 text-sm',
  'outline-none box-border transition',
  'focus:border-primary focus:ring-2 focus:ring-primary/30',
].join(' ')

interface ChangePasswordDialogProps {
  onClose: () => void
}

export function ChangePasswordDialog({ onClose }: ChangePasswordDialogProps) {
  const navigate = useNavigate()
  const { accountId, signOut } = useAuth()

  const [current, setCurrent]         = useState('')
  const [next, setNext]               = useState('')
  const [confirm, setConfirm]         = useState('')
  const [showCurrent, setShowCurrent] = useState(false)
  const [showNew, setShowNew]         = useState(false)
  const [currentError, setCurrentError] = useState<string | null>(null)
  const [error, setError]               = useState<string | null>(null)
  const [isLoading, setIsLoading]       = useState(false)
  const [forgotOpen, setForgotOpen]     = useState(false)

  const strength    = zxcvbn(next)
  const pwWeak      = next.length > 0 && strength.score < 2
  const pwSameAsOld = current !== '' && next !== '' && next === current
  const pwMismatch  = confirm !== '' && next !== confirm
  const allFilled   = current !== '' && next !== '' && confirm !== ''
  const canSubmit   = allFilled && !pwSameAsOld && !pwMismatch && !isLoading

  async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    if (!canSubmit || !accountId) return
    setError(null)
    setCurrentError(null)
    setIsLoading(true)
    try {
      await changePassword(accountId, current, next)
      await logout()
      signOut()
      navigate('/login')
    } catch (err) {
      if (err instanceof ApiError && err.status === 403) {
        setCurrentError('Incorrect password.')
      } else {
        setError(err instanceof Error ? err.message : 'Failed to change password.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center p-4 animate-backdrop-in bg-black/50 backdrop-blur-sm">
      <div className="relative w-full max-w-sm animate-dialog-appear bg-card rounded-3xl p-8 shadow-[0_32px_80px_rgba(0,0,0,0.22)]">

        {/* Close */}
        <button
          onClick={onClose}
          disabled={isLoading}
          aria-label="Close"
          className="absolute top-3.5 right-3.5 flex items-center justify-center w-8 h-8 rounded-lg bg-transparent border-0 cursor-pointer text-muted-foreground hover:text-foreground transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <X className="w-5 h-5" />
        </button>

        {/* Brand */}
        <div className="flex items-center gap-2 mb-[18px]">
          <BarkfestMark size={22} />
          <span className="font-heading font-bold text-[17px]">Barkfest</span>
        </div>

        <h2 className="font-heading font-bold text-2xl mb-1">Change password</h2>
        <p className="text-[13.5px] text-muted-foreground mb-[22px]">
          You'll be signed out after updating your password.
        </p>

        <form onSubmit={handleSubmit} noValidate>

          {/* Current password */}
          <div className="mb-3.5">
            <label className="block text-[13px] font-semibold mb-1.5 text-foreground">
              Current password <Req />
            </label>
            <div className="relative">
              <input
                id="cp-cur"
                type={showCurrent ? 'text' : 'password'}
                autoComplete="current-password"
                value={current}
                onChange={e => { setCurrent(e.target.value); setCurrentError(null) }}
                className={inputCls}
              />
              <EyeBtn show={showCurrent} onToggle={() => setShowCurrent(v => !v)} />
            </div>
            {currentError && (
              <p className="text-xs text-destructive mt-1">{currentError}</p>
            )}
          </div>

          {/* New password */}
          <div className="mb-3.5">
            <label className="block text-[13px] font-semibold mb-1.5 text-foreground">
              New password <Req />
            </label>
            <div className="relative">
              <input
                id="cp-new"
                type={showNew ? 'text' : 'password'}
                autoComplete="new-password"
                minLength={8} maxLength={72}
                value={next}
                onChange={e => setNext(e.target.value)}
                className={inputCls}
              />
              <EyeBtn show={showNew} onToggle={() => setShowNew(v => !v)} />
            </div>
            {next && (
              <div className="mt-1.5">
                <div className="flex gap-[3px] mb-[3px]">
                  {[0, 1, 2, 3].map(i => (
                    <div
                      key={i}
                      className={cn(
                        'flex-1 h-[3px] rounded-sm transition-colors',
                        i < strength.score ? STRENGTH_BG[strength.score] : 'bg-border'
                      )}
                    />
                  ))}
                </div>
                <p className={cn('text-[11px] font-semibold', STRENGTH_TEXT[strength.score])}>
                  {STRENGTH_LABELS[strength.score]}
                </p>
              </div>
            )}
            {pwSameAsOld && (
              <p className="text-xs text-destructive mt-1">New password must be different from your current password.</p>
            )}
            {!pwSameAsOld && pwWeak && (
              <p className="text-xs text-destructive mt-1">Try a longer or less predictable password.</p>
            )}
          </div>

          {/* Confirm new password */}
          <div className="mb-3.5">
            <label className="block text-[13px] font-semibold mb-1.5 text-foreground">
              Confirm new password <Req />
            </label>
            <div className="relative">
              <input
                id="cp-confirm"
                type={showNew ? 'text' : 'password'}
                autoComplete="new-password"
                value={confirm}
                onChange={e => setConfirm(e.target.value)}
                className={inputCls}
              />
              <EyeBtn show={showNew} onToggle={() => setShowNew(v => !v)} />
            </div>
            {pwMismatch && (
              <p className="text-xs text-destructive mt-1">Passwords do not match.</p>
            )}
          </div>

          {error && (
            <p className="text-[13px] text-destructive text-center mb-2.5">{error}</p>
          )}

          <div className="flex gap-2 mt-1">
            <button
              type="button"
              onClick={onClose}
              disabled={isLoading}
              className="flex-1 h-11 rounded-xl border-[1.5px] border-border bg-transparent text-muted-foreground text-sm font-medium cursor-pointer disabled:cursor-not-allowed disabled:opacity-50 transition-opacity"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={!canSubmit}
              className="flex-1 h-11 rounded-xl bg-primary text-white text-sm font-semibold flex items-center justify-center gap-2 disabled:cursor-not-allowed disabled:opacity-50 transition-opacity"
            >
              {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
              Update password
            </button>
          </div>

          <p className="text-center mt-3 text-[13px]">
            <button
              type="button"
              onClick={() => setForgotOpen(true)}
              className="bg-transparent border-0 p-0 cursor-pointer text-primary font-semibold text-[13px]"
            >
              Forgot password?
            </button>
          </p>
        </form>
      </div>

      {forgotOpen && (
        <div
          className="animate-backdrop-in fixed inset-0 z-[100] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4"
          onClick={() => setForgotOpen(false)}
        >
          <div
            className="animate-dialog-appear w-full max-w-[360px] bg-card rounded-[20px] p-7 shadow-[0_24px_64px_rgba(0,0,0,0.18)] relative"
            onClick={e => e.stopPropagation()}
          >
            <div className="flex items-center gap-2 mb-4">
              <BarkfestMark size={22} />
              <span className="font-heading text-[17px] font-bold">Barkfest</span>
            </div>
            <h3 className="font-heading text-xl font-bold mb-2">Forgot your password?</h3>
            <p className="text-sm text-muted-foreground leading-relaxed mb-1.5">
              Woof! Automated reset is on its way. Until then, shoot us an email and we'll get your paws back on the keys. Don't forget to include your username:
            </p>
            <a
              href="mailto:srpeterson@outlook.com"
              className="text-sm font-semibold text-primary no-underline inline-block mb-[22px]"
            >
              srpeterson@outlook.com
            </a>
            <button
              onClick={() => setForgotOpen(false)}
              className="block w-full h-[42px] rounded-[10px] border-[1.5px] border-border bg-transparent text-muted-foreground text-sm font-medium cursor-pointer"
            >
              Close
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

function Req() {
  return <span className="text-destructive ml-0.5">*</span>
}

function EyeBtn({ show, onToggle }: { show: boolean; onToggle: () => void }) {
  return (
    <button
      type="button"
      onClick={onToggle}
      aria-label={show ? 'Hide password' : 'Show password'}
      className="absolute right-0 top-0 h-11 w-[42px] flex items-center justify-center bg-transparent border-0 cursor-pointer text-muted-foreground hover:text-foreground transition-colors"
    >
      {show ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
    </button>
  )
}
