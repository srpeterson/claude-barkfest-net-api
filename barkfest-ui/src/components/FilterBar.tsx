import { useQuery } from '@tanstack/react-query'
import { NativeSelect } from '@/components/ui/select'
import { getBrowsePetTypes, getBrowseBreeds } from '@/lib/api'
import { getPetTypeLabel } from '@/config/petTypes'

interface FilterBarProps {
  petType: string
  onPetTypeChange: (value: string) => void
  breed: string
  onBreedChange: (value: string) => void
}

export function FilterBar({ petType, onPetTypeChange, breed, onBreedChange }: FilterBarProps) {
  const { data: petTypes = [] } = useQuery({
    queryKey: ['browse', 'pet-types'],
    queryFn: getBrowsePetTypes,
    staleTime: Infinity,
  })

  const { data: breeds = [] } = useQuery({
    queryKey: ['browse', 'breeds', petType],
    queryFn: () => getBrowseBreeds(petType),
    enabled: petType !== 'All',
    staleTime: Infinity,
  })

  return (
    <div className="sticky top-16 z-40 px-4 py-4">
      <div className="max-w-6xl mx-auto flex gap-4 items-center justify-center">
        <span className="text-sm text-primary font-bold">Show me:</span>
        {/* Pet type */}
        <NativeSelect
          value={petType}
          onChange={e => { onPetTypeChange(e.target.value); onBreedChange('') }}
          className="w-40"
        >
          <option value="All">All Pets</option>
          {petTypes.map(pt => (
            <option key={pt} value={pt}>{getPetTypeLabel(pt)}</option>
          ))}
        </NativeSelect>

        {/* Breed — only shown when a specific pet type is selected */}
        {petType !== 'All' && (
          <NativeSelect
            value={breed}
            onChange={e => onBreedChange(e.target.value)}
            className="w-48"
          >
            <option value="">All Breeds</option>
            {[...breeds].sort((a, b) => {
              if (a === 'Other') return 1
              if (b === 'Other') return -1
              if (a === 'Mixed') return 1
              if (b === 'Mixed') return -1
              return a.localeCompare(b)
            }).map(b => (
              <option key={b} value={b}>{b}</option>
            ))}
          </NativeSelect>
        )}
      </div>
    </div>
  )
}
