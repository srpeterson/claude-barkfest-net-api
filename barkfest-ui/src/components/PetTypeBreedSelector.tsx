import { useQuery } from '@tanstack/react-query'
import { NativeSelect } from '@/components/ui/select'
import { getBrowseBreeds, getBrowsePetTypes } from '@/lib/api'
import { getPetTypeLabel } from '@/config/petTypes'

interface PetTypeBreedSelectorProps {
  petType: string
  onPetTypeChange: (value: string) => void
  breed: string
  onBreedChange: (value: string) => void
  /**
   * 'filter' — used in FilterBar. Pet type includes an "All Pets" option;
   *            breed dropdown is hidden when "All" is selected.
   * 'form'   — used in AddPetDialog. Pet type has a "Select type" placeholder;
   *            breed dropdown is always visible but disabled until a type is chosen.
   *            Labels and required indicators are rendered.
   */
  variant?: 'filter' | 'form'
  petTypeClassName?: string
  breedClassName?: string
}

export function PetTypeBreedSelector({
  petType,
  onPetTypeChange,
  breed,
  onBreedChange,
  variant = 'form',
  petTypeClassName,
  breedClassName,
}: PetTypeBreedSelectorProps) {
  const isFilter = variant === 'filter'

  const { data: petTypes = [] } = useQuery({
    queryKey: ['browse', 'pet-types'],
    queryFn: getBrowsePetTypes,
    staleTime: Infinity,
  })

  const { data: rawBreeds = [] } = useQuery({
    queryKey: ['browse', 'breeds', petType],
    queryFn: () => getBrowseBreeds(petType),
    enabled: isFilter ? petType !== 'All' : !!petType,
    staleTime: Infinity,
  })

  const breeds = [...rawBreeds].sort((a, b) => {
    if (a === 'Other') return 1
    if (b === 'Other') return -1
    if (a === 'Mixed') return 1
    if (b === 'Mixed') return -1
    return a.localeCompare(b)
  })

  // ── Filter variant ────────────────────────────────────────────────────
  if (isFilter) {
    return (
      <>
        <NativeSelect
          value={petType}
          onChange={e => onPetTypeChange(e.target.value)}
          className={petTypeClassName}
        >
          <option value="All">All Pets</option>
          {petTypes.map(pt => (
            <option key={pt} value={pt}>{getPetTypeLabel(pt)}</option>
          ))}
        </NativeSelect>

        {petType !== 'All' && (
          <NativeSelect
            value={breed}
            onChange={e => onBreedChange(e.target.value)}
            className={breedClassName}
          >
            <option value="">All Breeds</option>
            {breeds.map(b => (
              <option key={b} value={b}>{b}</option>
            ))}
          </NativeSelect>
        )}
      </>
    )
  }

  // ── Form variant ──────────────────────────────────────────────────────
  return (
    <>
      <div className="space-y-1.5">
        <label className="text-sm font-semibold">
          Type <span className="text-destructive">*</span>
        </label>
        <NativeSelect
          value={petType}
          onChange={e => onPetTypeChange(e.target.value)}
          className={petTypeClassName}
        >
          <option value="">Select type</option>
          {petTypes.map(pt => (
            <option key={pt} value={pt}>{pt}</option>
          ))}
        </NativeSelect>
      </div>

      <div className="space-y-1.5">
        <label className="text-sm font-semibold">
          Breed <span className="text-destructive">*</span>
        </label>
        <NativeSelect
          value={breed}
          onChange={e => onBreedChange(e.target.value)}
          disabled={!petType}
          className={breedClassName}
        >
          <option value="">Select breed</option>
          {breeds.map(b => (
            <option key={b} value={b}>{b}</option>
          ))}
        </NativeSelect>
      </div>
    </>
  )
}
