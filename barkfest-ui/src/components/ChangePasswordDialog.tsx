import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Eye, EyeOff, Loader2, X } from 'lucide-react'
import zxcvbn from 'zxcvbn'
import { useAuth } from '@/hooks/useAuth'
import { ApiError, changePassword, logout } from '@/lib/api'
import { BarkfestMark } from '@/components/BarkfestMark'
import { PasswordStrengthMeter } from '@/components/PasswordStrengthMeter'
import { ForgotPasswordModal } from '@/components/ForgotPasswordModal'
import { inputBaseCls } from '@/lib/formStyles'
import { LIMITS } from '@/config/constraints'

const inputCls = `${inputBaseCls} h-11 bg-background pl-3 pr-11`

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

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
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
                minLength={LIMITS.passwordMin} maxLength={LIMITS.passwordMax}
                value={next}
                onChange={e => setNext(e.target.value)}
                className={inputCls}
              />
              <EyeBtn show={showNew} onToggle={() => setShowNew(v => !v)} />
            </div>
            {next && <PasswordStrengthMeter score={strength.score} />}
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

      {forgotOpen && <ForgotPasswordModal onClose={() => setForgotOpen(false)} />}
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
