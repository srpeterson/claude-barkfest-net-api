import { PawPrint } from 'lucide-react'

export function HeroSection() {
  return (
    <section className="relative overflow-hidden pt-20 pb-8 sm:pt-28 sm:pb-10">
      {/* Decorative blobs */}
      <div className="absolute top-10 left-10 w-72 h-72 bg-primary/10 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-10 right-10 w-56 h-56 bg-accent/10 rounded-full blur-3xl pointer-events-none" />

      <div className="relative max-w-3xl mx-auto text-center px-4 space-y-6">
        {/* Badge */}
        <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-primary/10 text-primary text-sm font-medium">
          <PawPrint className="w-4 h-4" />
          Share the love
        </div>

        {/* Headline */}
        <h1 className="font-heading text-4xl sm:text-5xl md:text-6xl font-bold tracking-tight leading-tight">
          Every Pet Has a
          <br />
          <span className="text-primary">Story to Tell</span>
        </h1>

        {/* Sub-copy */}
        <p className="text-lg text-muted-foreground max-w-xl mx-auto leading-relaxed">
          A community where pet lovers share photos and stories of their furry friends.
          Browse adorable pets or sign in to share yours.
        </p>
      </div>
    </section>
  )
}
