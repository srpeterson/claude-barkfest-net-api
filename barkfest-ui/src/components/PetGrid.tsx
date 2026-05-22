import { Loader2, PawPrint } from 'lucide-react'
import { PetCard } from '@/components/PetCard'
import type { Pet } from '@/types/pet'

interface PetGridProps {
  pets: Pet[]
  isLoading: boolean
}

export function PetGrid({ pets, isLoading }: PetGridProps) {
  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <Loader2 className="w-8 h-8 animate-spin text-primary" />
      </div>
    )
  }

  if (!pets || pets.length === 0) {
    return (
      <div className="flex flex-col items-center gap-3 py-20 text-muted-foreground">
        <PawPrint className="w-12 h-12" />
        <p className="text-lg">No pets posted yet. Be the first!</p>
      </div>
    )
  }

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      {pets.map((pet, index) => (
        <PetCard key={pet.id} pet={pet} index={index} />
      ))}
    </div>
  )
}
