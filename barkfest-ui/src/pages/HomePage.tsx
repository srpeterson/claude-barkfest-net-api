import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Navbar } from '@/components/Navbar'
import { HeroSection } from '@/components/HeroSection'
import { FilterBar } from '@/components/FilterBar'
import { PetGrid } from '@/components/PetGrid'
import { api } from '@/lib/api'
import type { Pet } from '@/types/pet'

const PAGE_SIZE = 6

export function HomePage() {
  const [page, setPage]       = useState(1)
  const [petType, setPetType] = useState('All')
  const [breed, setBreed]     = useState('')

  const { data: pets = [], isLoading } = useQuery<Pet[]>({
    queryKey: ['pets', page, petType, breed],
    queryFn: () => {
      const params = new URLSearchParams({
        page:     String(page),
        pageSize: String(PAGE_SIZE),
        ...(petType !== 'All' && { type: petType }),
        ...(breed             && { breed }),
      })
      return api.get<Pet[]>(`/pets?${params}`)
    },
  })

  const handleTypeChange = (val: string) => {
    setPetType(val)
    setBreed('')
    setPage(1)
  }

  const hasNext = pets.length === PAGE_SIZE
  const hasPrev = page > 1

  return (
    <div className="min-h-screen bg-background">
      <Navbar />
      <FilterBar
        petType={petType}
        onPetTypeChange={handleTypeChange}
        breed={breed}
        onBreedChange={setBreed}
      />
      <div className="-mt-12">
        <HeroSection />
      </div>
      <main className="max-w-6xl mx-auto px-4 sm:px-6 pb-20 space-y-8 -mt-10">
        <PetGrid pets={pets} isLoading={isLoading} />

        {pets.length > 0 && (
          <div className="flex items-center justify-center gap-3 pt-4">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage(p => p - 1)}
              disabled={!hasPrev}
              className="gap-1.5"
            >
              <ChevronLeft className="w-4 h-4" />
              Previous
            </Button>
            <span className="text-sm text-muted-foreground font-medium px-2">
              Page {page}
            </span>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage(p => p + 1)}
              disabled={!hasNext}
              className="gap-1.5"
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
