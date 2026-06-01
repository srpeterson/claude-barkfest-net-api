import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { X, Search, Check } from 'lucide-react'
import { cn } from '@/lib/utils'
import { getBrowseBreeds, getBrowsePetTypes } from '@/lib/api'
import { getPetTypeLabel } from '@/config/petTypes'

interface FilterProps {
  petTypeValue: number
  onPetTypeChange: (value: number) => void
  breedValue: number
  onBreedChange: (value: number) => void
}

interface MobileFilterSheetProps {
  filterProps: FilterProps
  onClose: () => void
}

// Unmounts on close so pending state resets automatically on next open.
export function MobileFilterSheet({ filterProps, onClose }: MobileFilterSheetProps) {
  const [pendingType, setPendingType]   = useState(filterProps.petTypeValue)
  const [pendingBreed, setPendingBreed] = useState(filterProps.breedValue)
  const [breedSearch, setBreedSearch]   = useState('')

  const { data: petTypes = [] } = useQuery({
    queryKey: ['browse', 'pet-types'],
    queryFn: getBrowsePetTypes,
    staleTime: Infinity,
  })

  const { data: breeds = [] } = useQuery({
    queryKey: ['browse', 'breeds', pendingType],
    queryFn: () => getBrowseBreeds(pendingType),
    enabled: !!pendingType,
    staleTime: Infinity,
  })

  const filteredBreeds = [
    { name: 'All Breeds', value: 0 } as const,
    ...breeds
      .filter(b => b.name.toLowerCase().includes(breedSearch.toLowerCase()))
      .sort((a, b) => {
        if (a.name === 'Other') return 1
        if (b.name === 'Other') return -1
        if (a.name === 'Mixed') return 1
        if (b.name === 'Mixed') return -1
        return a.name.localeCompare(b.name)
      }),
  ]

  function applyFilters() {
    if (pendingType !== filterProps.petTypeValue) {
      filterProps.onPetTypeChange(pendingType)
      filterProps.onBreedChange(0)
    } else {
      filterProps.onBreedChange(pendingBreed)
    }
    onClose()
  }

  return (
    <>
      {/* Backdrop */}
      <div
        onClick={onClose}
        className="animate-backdrop-in fixed inset-0 z-[200] bg-black/30 backdrop-blur-sm"
      />

      {/* Sheet */}
      <div className="animate-sheet-in fixed bottom-0 left-0 right-0 z-[201] bg-card rounded-t-[20px] shadow-[0_-8px_40px_rgba(0,0,0,0.15)] flex flex-col max-h-[80vh]">

        {/* Drag handle */}
        <div className="flex justify-center pt-3 pb-1 shrink-0">
          <div className="w-9 h-1 rounded-full bg-border" />
        </div>

        {/* Header */}
        <div className="flex items-center justify-between px-5 pt-2 pb-3.5 shrink-0">
          <h3 className="font-heading text-lg font-bold text-foreground m-0">Filters</h3>
          <button
            onClick={onClose}
            aria-label="Close"
            className="w-8 h-8 rounded-lg border-0 bg-secondary flex items-center justify-center cursor-pointer text-muted-foreground hover:text-foreground transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Scrollable content */}
        <div className="overflow-y-auto px-5 flex-1">

          {/* Pet type chips */}
          <div className="mb-5">
            <p className="text-[11px] font-bold tracking-[0.08em] uppercase text-muted-foreground mb-2.5">
              Pet Type
            </p>
            <div className="flex gap-2">
              {[
                { value: 0, label: 'All Pets' },
                ...petTypes.map(pt => ({ value: pt.value, label: getPetTypeLabel(pt.name) })),
              ].map(({ value, label }) => (
                <button
                  key={value}
                  onClick={() => { setPendingType(value); setPendingBreed(0) }}
                  className={cn(
                    'h-10 px-[18px] rounded-full border-[1.5px] text-[13px] cursor-pointer transition-all',
                    pendingType === value
                      ? 'border-primary bg-primary/10 text-primary font-semibold'
                      : 'border-border bg-transparent text-foreground font-normal'
                  )}
                >
                  {label}
                </button>
              ))}
            </div>
          </div>

          {/* Breed list — only when a type is selected */}
          {!!pendingType && (
            <div className="mb-4">
              <p className="text-[11px] font-bold tracking-[0.08em] uppercase text-muted-foreground mb-2.5">
                Breed{' '}
                <span className="font-normal normal-case tracking-normal text-[12px]">
                  ({breeds.length} breeds)
                </span>
              </p>

              {/* Search box */}
              <div className="relative mb-2">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground pointer-events-none" />
                <input
                  placeholder="Search breeds…"
                  value={breedSearch}
                  onChange={e => setBreedSearch(e.target.value)}
                  className="w-full h-10 rounded-[10px] border-[1.5px] border-border bg-secondary pl-[34px] pr-9 text-[13px] text-foreground outline-none box-border"
                />
                {breedSearch && (
                  <button
                    onClick={() => setBreedSearch('')}
                    aria-label="Clear search"
                    className="absolute right-2.5 top-1/2 -translate-y-1/2 flex items-center justify-center bg-transparent border-0 cursor-pointer text-muted-foreground hover:text-foreground transition-colors p-0"
                  >
                    <X className="w-4 h-4" />
                  </button>
                )}
              </div>

              {/* Breed list */}
              <div className="border-[1.5px] border-border rounded-xl overflow-hidden max-h-[220px] overflow-y-auto">
                {filteredBreeds.length === 0 ? (
                  <p className="py-3.5 px-4 m-0 text-[13px] text-muted-foreground text-center">
                    No breeds found
                  </p>
                ) : (
                  filteredBreeds.map((b, i) => (
                    <button
                      key={b.value}
                      onClick={() => setPendingBreed(b.value)}
                      className={cn(
                        'w-full h-11 px-4 flex items-center justify-between border-0 cursor-pointer text-sm text-left transition-colors',
                        i < filteredBreeds.length - 1 ? 'border-b border-border' : '',
                        pendingBreed === b.value
                          ? 'bg-primary/10 text-primary font-semibold'
                          : 'bg-transparent text-foreground font-normal'
                      )}
                    >
                      {b.name}
                      {pendingBreed === b.value && (
                        <Check className="w-4 h-4 text-primary shrink-0" />
                      )}
                    </button>
                  ))
                )}
              </div>
            </div>
          )}
        </div>

        {/* Show results CTA */}
        <div className="px-5 pt-3 pb-8 shrink-0">
          <button
            onClick={applyFilters}
            className="w-full h-12 rounded-xl border-0 bg-primary text-white text-[15px] font-semibold cursor-pointer"
          >
            Show results
          </button>
        </div>
      </div>
    </>
  )
}
