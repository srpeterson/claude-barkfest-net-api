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

        {/* Link columns */}
        <div className="flex gap-12 flex-wrap">
          <div>
            <p className="m-0 mb-3 text-[11px] font-bold tracking-[0.08em] uppercase text-muted-foreground">
              Company
            </p>
            {['About', 'Blog', 'Careers'].map(l => (
              <p key={l} className="text-[13px] text-foreground/60 block mb-2.5 cursor-default">{l}</p>
            ))}
          </div>
          <div>
            <p className="m-0 mb-3 text-[11px] font-bold tracking-[0.08em] uppercase text-muted-foreground">
              Legal
            </p>
            {['Privacy Policy', 'Terms of Use', 'Contact'].map(l => (
              <p key={l} className="text-[13px] text-foreground/60 block mb-2.5 cursor-default">{l}</p>
            ))}
          </div>
        </div>
      </div>
    </footer>
  )
}
