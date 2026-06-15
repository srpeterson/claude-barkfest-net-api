import { NativeSelect } from '@/components/ui/select'
import { useBreedOptions, usePetTypeOptions } from '@/hooks/usePetOptions'

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
  const { data: petTypes = [] } = usePetTypeOptions()
  const { data: breeds = [] } = useBreedOptions(petTypeValue)

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
