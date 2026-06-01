import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Navbar } from '@/components/Navbar'
import { HeroSection } from '@/components/HeroSection'
import { PetGrid } from '@/components/PetGrid'
import { BarkfestMark } from '@/components/BarkfestMark'
import { getBrowseImages } from '@/lib/api'
import type { BrowseImageDto, PagedResult } from '@/types/browse'

const PAGE_SIZE = 6

function Footer() {
  const year = new Date().getFullYear()
  return (
    <footer className="border-t border-border bg-card px-6 pt-10 pb-0">
      <div className="max-w-[72rem] mx-auto flex flex-wrap gap-14 items-start">

        {/* Brand column */}
        <div className="flex flex-col gap-3.5 max-w-[220px]">
          <div className="flex items-center gap-2">
            <BarkfestMark size={24} />
            <span className="font-heading text-[17px] font-bold tracking-[-0.02em] text-foreground">
              Barkfest
            </span>
          </div>
          <p className="m-0 text-[13px] text-muted-foreground leading-relaxed">
            A community where pet lovers share photos and stories of their furry friends.
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

      {/* Bottom bar */}
      <div className="max-w-[72rem] mx-auto mt-8 py-6 border-t border-border flex justify-between items-center flex-wrap gap-2">
        <p className="m-0 text-xs text-muted-foreground">
          © {year} Barkfest. All rights reserved.
        </p>
        <p className="m-0 text-xs text-muted-foreground">
          Made with love for pet lovers.
        </p>
      </div>
    </footer>
  )
}

export function HomePage() {
  const [searchParams, setSearchParams] = useSearchParams()

  const [page, setPage]                 = useState(1)
  const [petTypeValue, setPetTypeValue] = useState(() => Number(searchParams.get('petTypeValue') || 0))
  const [breedValue, setBreedValue]     = useState(() => Number(searchParams.get('breedValue') || 0))

  const { data, isLoading } = useQuery<PagedResult<BrowseImageDto>>({
    queryKey: ['browse', 'images', page, petTypeValue, breedValue],
    queryFn: () => getBrowseImages({
      page,
      pageSize: PAGE_SIZE,
      ...(petTypeValue && { petTypeValue }),
      ...(breedValue   && { breedValue }),
    }),
  })

  const pets             = data?.items ?? []
  const hasMore          = data?.hasMore ?? false
  const hasPrev          = page > 1
  const hasActiveFilters = petTypeValue !== 0 || breedValue !== 0

  const handleTypeChange = (val: number) => {
    setPetTypeValue(val)
    setBreedValue(0)
    setPage(1)
    const next = new URLSearchParams()
    if (val) next.set('petTypeValue', String(val))
    setSearchParams(next, { replace: true })
  }

  const handleBreedChange = (val: number) => {
    setBreedValue(val)
    setPage(1)
    setSearchParams(prev => {
      const next = new URLSearchParams(prev)
      if (val) next.set('breedValue', String(val))
      else next.delete('breedValue')
      return next
    }, { replace: true })
  }

  return (
    <div className="min-h-screen bg-background flex flex-col">
      <Navbar
        filterProps={{
          petTypeValue,
          onPetTypeChange: handleTypeChange,
          breedValue,
          onBreedChange: handleBreedChange,
        }}
      />
      <HeroSection petTypeValue={petTypeValue} breedValue={breedValue} />

      <main className="flex-1 w-full max-w-6xl mx-auto px-4 sm:px-6 pb-20 space-y-8 -mt-6">
        <PetGrid pets={pets} isLoading={isLoading} hasActiveFilters={hasActiveFilters} />

        {/* Pagination */}
        {(hasPrev || hasMore) && (
          <div className="flex items-center justify-center gap-3 pt-4">
            <button
              onClick={() => setPage(p => p - 1)}
              disabled={!hasPrev}
              className="inline-flex items-center gap-1.5 h-8 px-3 rounded-lg border border-primary bg-transparent text-primary text-[13px] font-medium cursor-pointer disabled:cursor-not-allowed disabled:opacity-30 hover:enabled:bg-primary/10 transition-colors"
            >
              <ChevronLeft className="w-4 h-4" />Previous
            </button>
            <span className="px-4 py-1.5 rounded-full bg-primary/10 text-primary text-[13px] font-semibold">
              Page {page}
            </span>
            <button
              onClick={() => setPage(p => p + 1)}
              disabled={!hasMore}
              className="inline-flex items-center gap-1.5 h-8 px-3 rounded-lg border border-primary bg-transparent text-primary text-[13px] font-medium cursor-pointer disabled:cursor-not-allowed disabled:opacity-30 hover:enabled:bg-primary/10 transition-colors"
            >
              Next<ChevronRight className="w-4 h-4" />
            </button>
          </div>
        )}
      </main>

      <Footer />
    </div>
  )
}
