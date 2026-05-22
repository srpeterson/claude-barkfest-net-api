import { NativeSelect } from '@/components/ui/select'

interface FilterBarProps {
  petType: string
  onPetTypeChange: (value: string) => void
  breed: string
  onBreedChange: (value: string) => void
}

const DOG_BREEDS = ['Golden Retriever', 'Labrador', 'Beagle', 'Shih Tzu', 'Poodle', 'Bulldog']
const CAT_BREEDS = ['Tabby', 'Domestic Shorthair', 'Persian', 'Siamese', 'Maine Coon']

export function FilterBar({ petType, onPetTypeChange, breed, onBreedChange }: FilterBarProps) {
  return (
    <div className="sticky top-16 z-40 px-4 py-4">

      <div className="max-w-6xl mx-auto flex gap-4 items-center justify-center">
        {/* Pet type */}
        <NativeSelect
          value={petType}
          onChange={e => { onPetTypeChange(e.target.value); onBreedChange('') }}
          className="w-40"
        >
          <option value="All">All</option>
          <option value="Dog">Dog</option>
          <option value="Cat">Cat</option>
        </NativeSelect>

        {/* Breed (conditional) */}
        {petType !== 'All' && (
          <NativeSelect
            value={breed}
            onChange={e => onBreedChange(e.target.value)}
            className="w-40"
          >
            <option value="">All Breeds</option>
            {(petType === 'Dog' ? DOG_BREEDS : CAT_BREEDS).map(b => (
              <option key={b} value={b}>{b}</option>
            ))}
          </NativeSelect>
        )}
      </div>
    </div>
  )
}
