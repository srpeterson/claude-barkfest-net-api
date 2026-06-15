import { NativeSelect } from '@/components/ui/select'
import { useBreedOptions, usePetTypeOptions } from '@/hooks/usePetOptions'
import { getPetTypeLabel } from '@/config/petTypes'

interface PetTypeBreedSelectorProps {
  petTypeValue: number
  onPetTypeChange: (value: number) => void
  breedValue: number
  onBreedChange: (value: number) => void
  petTypeClassName?: string
  breedClassName?: string
}

export function PetTypeBreedSelector({
  petTypeValue,
  onPetTypeChange,
  breedValue,
  onBreedChange,
  petTypeClassName,
  breedClassName,
}: PetTypeBreedSelectorProps) {
  const { data: petTypes = [] } = usePetTypeOptions()
  const { data: breeds = [] } = useBreedOptions(petTypeValue)

  return (
    <>
      <NativeSelect
        value={petTypeValue || ''}
        onChange={e => onPetTypeChange(Number(e.target.value))}
        className={petTypeClassName}
      >
        <option value={0}>All Pets</option>
        {petTypes.map(pt => (
          <option key={pt.value} value={pt.value}>{getPetTypeLabel(pt.name)}</option>
        ))}
      </NativeSelect>

      {!!petTypeValue && (
        <NativeSelect
          value={breedValue || ''}
          onChange={e => onBreedChange(Number(e.target.value))}
          className={breedClassName}
        >
          <option value={0}>All Breeds</option>
          {breeds.map(b => (
            <option key={b.value} value={b.value}>{b.name}</option>
          ))}
        </NativeSelect>
      )}
    </>
  )
}
