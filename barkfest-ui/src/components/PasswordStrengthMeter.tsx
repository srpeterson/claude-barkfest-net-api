import { cn } from '@/lib/utils'

const STRENGTH_LABELS = ['Very weak', 'Weak', 'Fair', 'Strong', 'Very strong']
const STRENGTH_BG     = ['bg-[#e5484d]', 'bg-[#f76b15]', 'bg-[#d4a017]', 'bg-accent', 'bg-accent']
const STRENGTH_TEXT   = ['text-[#e5484d]', 'text-[#f76b15]', 'text-[#d4a017]', 'text-accent', 'text-accent']

/** zxcvbn strength meter — four bars plus a label. `score` is the zxcvbn 0–4 score. */
export function PasswordStrengthMeter({ score }: { score: number }) {
  return (
    <div className="mt-2">
      <div className="flex gap-1 mb-[3px]">
        {[0, 1, 2, 3].map(i => (
          <div
            key={i}
            className={cn(
              'flex-1 h-[3px] rounded-sm transition-colors',
              i < score ? STRENGTH_BG[score] : 'bg-border'
            )}
          />
        ))}
      </div>
      <p className={cn('text-[11px] font-semibold m-0', STRENGTH_TEXT[score])}>
        {STRENGTH_LABELS[score]}
      </p>
    </div>
  )
}
