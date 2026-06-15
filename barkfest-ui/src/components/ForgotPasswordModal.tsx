import { BarkfestMark } from '@/components/BarkfestMark'

const SUPPORT_EMAIL = 'srpeterson@outlook.com'

export function ForgotPasswordModal({ onClose }: { onClose: () => void }) {
  return (
    <div
      className="animate-backdrop-in fixed inset-0 z-[100] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4"
      onClick={onClose}
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
          href={`mailto:${SUPPORT_EMAIL}`}
          className="text-sm font-semibold text-primary no-underline inline-block mb-[22px]"
        >
          {SUPPORT_EMAIL}
        </a>
        <button
          onClick={onClose}
          className="block w-full h-[42px] rounded-[10px] border-[1.5px] border-border bg-transparent text-muted-foreground text-sm font-medium cursor-pointer"
        >
          Close
        </button>
      </div>
    </div>
  )
}
