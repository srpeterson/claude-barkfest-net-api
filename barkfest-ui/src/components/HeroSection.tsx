import { useQuery } from '@tanstack/react-query'
import { getBrowseImages } from '@/lib/api'
import { getBlobImageUrl } from '@/lib/imageUrl'

const STRIP_SIZE = 5

interface HeroSectionProps {
  petTypeValue: number
  breedValue: number
}

export function HeroSection({ petTypeValue, breedValue }: HeroSectionProps) {
  const { data } = useQuery({
    queryKey: ['browse', 'hero-strip', petTypeValue, breedValue],
    queryFn: () => getBrowseImages({
      page: 1,
      pageSize: STRIP_SIZE,
      ...(petTypeValue && { petTypeValue }),
      ...(breedValue   && { breedValue }),
    }),
    staleTime: 5 * 60 * 1000,
  })

  const stripImages = data?.items ?? []
  const totalCount  = data?.totalCount ?? 0

  return (
    <section className="relative overflow-hidden pt-10 pb-8 sm:pt-14 sm:pb-10">
      {/* Decorative blobs */}
      <div className="absolute top-10 left-10 w-72 h-72 bg-primary/10 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-10 right-10 w-56 h-56 bg-accent/10 rounded-full blur-3xl pointer-events-none" />

      <div className="relative max-w-3xl mx-auto text-center px-4 space-y-6">
        {/* Headline */}
        <h1
          className="animate-fade-in-up font-heading text-4xl sm:text-5xl md:text-6xl font-bold tracking-tight leading-tight"
          style={{ animationDelay: '0s' }}
        >
          Every Pet Has a
          <br />
          <span className="text-primary">Story to Tell</span>
        </h1>

        {/* Sub-copy */}
        <p
          className="animate-fade-in-up text-lg text-muted-foreground max-w-xl mx-auto leading-relaxed"
          style={{ animationDelay: '0.1s' }}
        >
          A community where pet lovers share photos and stories of their furry friends.
          Browse adorable pets or sign in to share yours.
        </p>

        {/* Social proof strip — hidden only when there are no results */}
        {stripImages.length > 0 && (
          <div
            className="animate-fade-in-up flex items-center justify-center gap-3"
            style={{ animationDelay: '0.2s' }}
          >
            {/* Overlapping circular thumbnails */}
            <div className="flex items-center">
              {stripImages.map((img, i) => (
                <div
                  key={img.imageId}
                  className="w-9 h-9 rounded-full overflow-hidden border-2 border-card -ml-2 first:ml-0 shrink-0"
                  style={{ zIndex: STRIP_SIZE - i }}
                >
                  <img
                    src={getBlobImageUrl(img.blobName)}
                    alt={img.petName}
                    className="w-full h-full object-cover"
                  />
                </div>
              ))}
            </div>

            {/* Count text */}
            <span className="text-sm text-muted-foreground">
              <strong className="text-foreground font-semibold">
                {totalCount.toLocaleString()}
              </strong>{' '}
              {totalCount === 1 ? 'pet' : 'pets'} shared
            </span>
          </div>
        )}
      </div>
    </section>
  )
}
