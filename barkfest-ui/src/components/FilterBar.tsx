import { useState } from 'react'
import { SlidersHorizontal, X } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { PetTypeBreedSelector } from '@/components/PetTypeBreedSelector'
import { useIsMobile } from '@/hooks/useIsMobile'
import { getBrowseBreeds, getBrowsePetTypes } from '@/lib/api'
import { getPetTypeLabel } from '@/config/petTypes'

interface FilterBarProps {
  petTypeValue: number
  onPetTypeChange: (value: number) => void
  breedValue: number
  onBreedChange: (value: number) => void
}

export function FilterBar({ petTypeValue, onPetTypeChange, breedValue, onBreedChange }: FilterBarProps) {
  const isMobile = useIsMobile()
  const [sheetOpen, setSheetOpen] = useState(false)
  const [pendingType, setPendingType] = useState(petTypeValue)
  const [pendingBreed, setPendingBreed] = useState(breedValue)
  const [breedSearch, setBreedSearch] = useState('')

  const hasActiveFilters = petTypeValue !== 0 || breedValue !== 0

  const { data: petTypes = [] } = useQuery({
    queryKey: ['browse', 'pet-types'],
    queryFn: getBrowsePetTypes,
    staleTime: Infinity,
  })

  const { data: rawBreeds = [] } = useQuery({
    queryKey: ['browse', 'breeds', pendingType],
    queryFn: () => getBrowseBreeds(pendingType),
    enabled: !!pendingType,
    staleTime: Infinity,
  })

  const filteredBreeds = rawBreeds
    .filter(b => b.name.toLowerCase().includes(breedSearch.toLowerCase()))
    .sort((a, b) => {
      if (a.name === 'Other') return 1
      if (b.name === 'Other') return -1
      if (a.name === 'Mixed') return 1
      if (b.name === 'Mixed') return -1
      return a.name.localeCompare(b.name)
    })

  function openSheet() {
    setPendingType(petTypeValue)
    setPendingBreed(breedValue)
    setBreedSearch('')
    setSheetOpen(true)
  }

  function applyFilters() {
    if (pendingType !== petTypeValue) {
      onPetTypeChange(pendingType)
      onBreedChange(0)
    } else {
      onBreedChange(pendingBreed)
    }
    setSheetOpen(false)
  }

  function getFilterLabel() {
    if (!petTypeValue) return 'All Pets'
    const pt = petTypes.find(p => p.value === petTypeValue)
    const typeName = pt ? getPetTypeLabel(pt.name) : 'Pets'
    if (!breedValue) return typeName
    const breed = rawBreeds.find(b => b.value === breedValue)
    return breed ? `${typeName} · ${breed.name}` : typeName
  }

  if (isMobile) {
    return (
      <>
        {/* Mobile filter pill button */}
        <div className="sticky top-[72px] z-40 flex justify-center py-3">
          <button
            onClick={openSheet}
            className="flex items-center gap-2 px-4 py-2 rounded-full text-sm font-medium transition-colors"
            style={{
              background: 'var(--card)',
              border: '1px solid var(--border)',
              boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
              color: 'var(--foreground)',
            }}
          >
            <SlidersHorizontal className="w-4 h-4 text-primary" />
            <span>{getFilterLabel()}</span>
            {hasActiveFilters && (
              <span className="w-2 h-2 rounded-full bg-primary shrink-0" />
            )}
          </button>
        </div>

        {/* Bottom sheet backdrop */}
        {sheetOpen && (
          <div
            className="fixed inset-0 z-50 animate-backdrop-in"
            style={{ background: 'rgba(0,0,0,0.5)' }}
            onClick={() => setSheetOpen(false)}
          >
            {/* Bottom sheet */}
            <div
              className="absolute bottom-0 left-0 right-0 rounded-t-3xl animate-sheet-in"
              style={{ background: 'var(--card)' }}
              onClick={e => e.stopPropagation()}
            >
              {/* Handle + header */}
              <div className="flex items-center justify-between px-5 pt-4 pb-2">
                <div className="w-10 h-1 rounded-full bg-border absolute top-3 left-1/2 -translate-x-1/2" />
                <span className="font-heading font-semibold text-base mt-2">Filter Pets</span>
                <button
                  onClick={() => setSheetOpen(false)}
                  className="p-1 rounded-full hover:bg-secondary transition-colors mt-2"
                >
                  <X className="w-5 h-5 text-muted-foreground" />
                </button>
              </div>

              <div className="px-5 pb-6 space-y-5">
                {/* Pet Type chips */}
                <div className="space-y-2">
                  <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Pet Type</p>
                  <div className="flex flex-wrap gap-2">
                    <button
                      onClick={() => { setPendingType(0); setPendingBreed(0) }}
                      className="px-4 py-2 rounded-full text-sm font-medium transition-colors"
                      style={{
                        background: pendingType === 0 ? 'var(--primary)' : 'var(--secondary)',
                        color: pendingType === 0 ? 'white' : 'var(--foreground)',
                      }}
                    >
                      All Pets
                    </button>
                    {petTypes.map(pt => (
                      <button
                        key={pt.value}
                        onClick={() => { setPendingType(pt.value); setPendingBreed(0) }}
                        className="px-4 py-2 rounded-full text-sm font-medium transition-colors"
                        style={{
                          background: pendingType === pt.value ? 'var(--primary)' : 'var(--secondary)',
                          color: pendingType === pt.value ? 'white' : 'var(--foreground)',
                        }}
                      >
                        {getPetTypeLabel(pt.name)}
                      </button>
                    ))}
                  </div>
                </div>

                {/* Breed list — only shown when a pet type is selected */}
                {!!pendingType && (
                  <div className="space-y-2">
                    <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Breed</p>
                    <input
                      type="text"
                      placeholder="Search breeds..."
                      value={breedSearch}
                      onChange={e => setBreedSearch(e.target.value)}
                      className="w-full h-10 rounded-xl px-3 text-sm focus:outline-none focus:ring-2"
                      style={{
                        background: 'var(--secondary)',
                        border: '1px solid var(--border)',
                        color: 'var(--foreground)',
                      }}
                    />
                    <div className="max-h-48 overflow-y-auto space-y-0.5">
                      <button
                        onClick={() => setPendingBreed(0)}
                        className="w-full flex items-center justify-between h-11 px-3 rounded-xl text-sm transition-colors"
                        style={{
                          background: pendingBreed === 0 ? 'var(--primary)/10' : 'transparent',
                          color: pendingBreed === 0 ? 'var(--primary)' : 'var(--foreground)',
                        }}
                      >
                        All Breeds
                        {pendingBreed === 0 && <span className="text-primary">✓</span>}
                      </button>
                      {filteredBreeds.map(b => (
                        <button
                          key={b.value}
                          onClick={() => setPendingBreed(b.value)}
                          className="w-full flex items-center justify-between h-11 px-3 rounded-xl text-sm transition-colors"
                          style={{
                            background: pendingBreed === b.value ? 'rgba(223,103,73,0.1)' : 'transparent',
                            color: pendingBreed === b.value ? 'var(--primary)' : 'var(--foreground)',
                          }}
                        >
                          {b.name}
                          {pendingBreed === b.value && <span className="text-primary">✓</span>}
                        </button>
                      ))}
                    </div>
                  </div>
                )}

                {/* Show results CTA */}
                <button
                  onClick={applyFilters}
                  className="w-full h-12 rounded-2xl text-sm font-semibold text-white transition-opacity hover:opacity-90"
                  style={{ background: 'var(--primary)' }}
                >
                  Show results
                </button>
              </div>
            </div>
          </div>
        )}
      </>
    )
  }

  return (
    <div className="sticky top-[72px] z-40 px-4 py-4">
      <div className="max-w-6xl mx-auto flex gap-4 items-center justify-center">
        <span className="text-sm text-primary font-bold">Show me:</span>
        <PetTypeBreedSelector
          petTypeValue={petTypeValue}
          onPetTypeChange={onPetTypeChange}
          breedValue={breedValue}
          onBreedChange={onBreedChange}
          petTypeClassName="w-40"
          breedClassName="w-48"
        />
      </div>
    </div>
  )
}
