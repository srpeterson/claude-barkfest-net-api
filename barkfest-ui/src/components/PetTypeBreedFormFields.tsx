import { useQuery } from '@tanstack/react-query'
import { NativeSelect } from '@/components/ui/select'
import { getBrowseBreeds, getBrowsePetTypes } from '@/lib/api'

interface PetTypeBreedFormFieldsProps {
  petTypeValue: number
  onPetTypeChange: (value: number) => void
  breedValue: number
  onBreedChange: (value: number) => void
  petTypeClassName?: string
  breedClassName?: string
}

export function PetTypeBreedFormFields({
  petTypeValue,
  onPetTypeChange,
  breedValue,
  onBreedChange,
  petTypeClassName,
  breedClassName,
}: PetTypeBreedFormFieldsProps) {
  const { data: petTypes = [] } = useQuery({
    queryKey: ['browse', 'pet-types'],
    queryFn: getBrowsePetTypes,
    staleTime: Infinity,
  })

  const { data: rawBreeds = [] } = useQuery({

    queryKey: ['browse', 'breeds', petTypeValue],
    queryFn: () => getBrowseBreeds(petTypeValue),
    enabled: !!petTypeValue,
    staleTime: Infinity,
  })

  const breeds = [...rawBreeds].sort((a, b) => {
    if (a.name === 'Other') return 1
    if (b.name === 'Other') return -1
    if (a.name === 'Mixed') return 1
    if (b.name === 'Mixed') return -1
    return a.name.localeCompare(b.name)
  })

  return (
    <>
      <div className="space-y-1.5">
        <label className="text-sm font-semibold">
          Type <span className="text-destructive">*</span>
        </label>
        <NativeSelect
          value={petTypeValue || ''}
          onChange={e => onPetTypeChange(Number(e.target.value))}
          className={petTypeClassName}
        >
          <option value="">Select type</option>
          {petTypes.map(pt => (
            <option key={pt.value} value={pt.value}>{pt.name}</option>
          ))}
        </NativeSelect>
      </div>

      <div className="space-y-1.5">
        <label className="text-sm font-semibold">
          Breed <span className="text-destructive">*</span>
        </label>
        <NativeSelect
          value={breedValue || ''}
          onChange={e => onBreedChange(Number(e.target.value))}
          disabled={!petTypeValue}
          className={breedClassName}
        >
          <option value="">Select breed</option>
          {breeds.map(b => (
            <option key={b.value} value={b.value}>{b.name}</option>
          ))}
        </NativeSelect>
      </div>
    </>
  )
}
