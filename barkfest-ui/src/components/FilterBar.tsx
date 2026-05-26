import { PetTypeBreedSelector } from '@/components/PetTypeBreedSelector'

interface FilterBarProps {
  petTypeValue: number
  onPetTypeChange: (value: number) => void
  breedValue: number
  onBreedChange: (value: number) => void
}

export function FilterBar({ petTypeValue, onPetTypeChange, breedValue, onBreedChange }: FilterBarProps) {
  return (
    <div className="sticky top-16 z-40 px-4 py-4">
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
