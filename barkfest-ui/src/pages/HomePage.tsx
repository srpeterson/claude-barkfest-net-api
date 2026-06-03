import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Navbar } from '@/components/Navbar'
import { HeroSection } from '@/components/HeroSection'
import { PetGrid } from '@/components/PetGrid'
import { Footer } from '@/components/Footer'
import { getBrowseImages } from '@/lib/api'
import type { BrowseImageDto, PagedResult } from '@/types/browse'

const PAGE_SIZE = 9

export function HomePage() {
  const [searchParams, setSearchParams] = useSearchParams()

  const [page, setPage]                 = useState(1)
  const [petTypeValue, setPetTypeValue] = useState(() =>
    Number(searchParams.get('petTypeValue')) ||
    Number(localStorage.getItem('barkfest_filter_type') || 0)
  )
  const [breedValue, setBreedValue]     = useState(() =>
    Number(searchParams.get('breedValue')) ||
    Number(localStorage.getItem('barkfest_filter_breed') || 0)
  )

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
    localStorage.setItem('barkfest_filter_type', String(val))
    localStorage.setItem('barkfest_filter_breed', '0')
    const next = new URLSearchParams()
    if (val) next.set('petTypeValue', String(val))
    setSearchParams(next, { replace: true })
  }

  const handleBreedChange = (val: number) => {
    setBreedValue(val)
    setPage(1)
    localStorage.setItem('barkfest_filter_breed', String(val))
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

      <main className="flex-1 w-full max-w-[72rem] mx-auto px-5 pb-20 space-y-8 -mt-6">
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
