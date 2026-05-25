import { PetTypeBreedSelector } from '@/components/PetTypeBreedSelector'

interface FilterBarProps {
  petType: string
  onPetTypeChange: (value: string) => void
  breed: string
  onBreedChange: (value: string) => void
}

export function FilterBar({ petType, onPetTypeChange, breed, onBreedChange }: FilterBarProps) {
  return (
    <div className="sticky top-16 z-40 px-4 py-4">
      <div className="max-w-6xl mx-auto flex gap-4 items-center justify-center">
        <span className="text-sm text-primary font-bold">Show me:</span>
        <PetTypeBreedSelector
          variant="filter"
          petType={petType}
          onPetTypeChange={onPetTypeChange}
          breed={breed}
          onBreedChange={onBreedChange}
          petTypeClassName="w-40"
          breedClassName="w-48"
        />
      </div>
    </div>
  )
}
