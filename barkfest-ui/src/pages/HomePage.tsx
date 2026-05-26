import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Navbar } from '@/components/Navbar'
import { HeroSection } from '@/components/HeroSection'
import { FilterBar } from '@/components/FilterBar'
import { PetGrid } from '@/components/PetGrid'
import { getBrowseImages } from '@/lib/api'
import type { BrowseImageDto, PagedResult } from '@/types/browse'

const PAGE_SIZE = 6

export function HomePage() {
  const [page, setPage]               = useState(1)
  const [petTypeValue, setPetTypeValue] = useState(0)
  const [breedValue, setBreedValue]     = useState(0)

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
  }

  const handleBreedChange = (val: number) => {
    setBreedValue(val)
    setPage(1)
  }

  return (
    <div className="min-h-screen bg-background">
      <Navbar />
      <FilterBar
        petTypeValue={petTypeValue}
        onPetTypeChange={handleTypeChange}
        breedValue={breedValue}
        onBreedChange={handleBreedChange}
      />
      <div className="-mt-12">
        <HeroSection petTypeValue={petTypeValue} breedValue={breedValue} />
      </div>
      <main className="max-w-6xl mx-auto px-4 sm:px-6 pb-20 space-y-8 -mt-6">
        <PetGrid pets={pets} isLoading={isLoading} hasActiveFilters={hasActiveFilters} />

        {(hasPrev || hasMore) && (
          <div className="flex items-center justify-center gap-3 pt-4">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage(p => p - 1)}
              disabled={!hasPrev}
              className="gap-1.5 border-primary/40 text-primary hover:bg-primary/10 hover:text-primary disabled:opacity-30 disabled:border-border disabled:text-muted-foreground"
            >
              <ChevronLeft className="w-4 h-4" />
              Previous
            </Button>
            <span className="px-4 py-1.5 rounded-full bg-primary/10 text-primary text-sm font-semibold">
              Page {page}
            </span>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage(p => p + 1)}
              disabled={!hasMore}
              className="gap-1.5 border-primary/40 text-primary hover:bg-primary/10 hover:text-primary disabled:opacity-30 disabled:border-border disabled:text-muted-foreground"
            >
              Next
              <ChevronRight className="w-4 h-4" />
            </Button>
          </div>
        )}
      </main>
    </div>
  )
}
