import { BarkfestMark } from '@/components/BarkfestMark'

export function Footer({ maxWidth = 'max-w-[72rem]' }: { maxWidth?: string }) {
  const year = new Date().getFullYear()
  return (
    <footer className="bg-card border-t border-border">
      <div className={`${maxWidth} mx-auto px-5 pt-10 pb-10 flex flex-wrap gap-14 items-start justify-between`}>

        {/* Brand column */}
        <div className="flex flex-col gap-3.5 max-w-[220px]">
          <div className="flex items-center gap-2">
            <BarkfestMark size={24} />
            <span className="font-heading text-[17px] font-bold tracking-[-0.02em] text-foreground">
              Barkfest
            </span>
          </div>
          <p className="m-0 text-[13px] text-muted-foreground leading-relaxed">
            A community where pet lovers share photos and stories of their furry friends, made with love.
          </p>
          <p className="m-0 text-xs text-muted-foreground">
            © {year} Barkfest. All rights reserved.
          </p>
        </div>

        {/* Link row */}
        <div className="flex items-center gap-1 flex-wrap">
          {['About', 'Privacy Policy', 'Terms of Use', 'Contact'].map((l, i, arr) => (
            <span key={l} className="flex items-center gap-1">
              <span className="text-[13px] text-foreground/60 cursor-pointer hover:text-foreground transition-colors">{l}</span>
              {i < arr.length - 1 && <span className="text-muted-foreground/40 text-[13px]">·</span>}
            </span>
          ))}
        </div>
      </div>
    </footer>
  )
}
