import { Loader2, PawPrint } from 'lucide-react'
import { PetCard } from '@/components/PetCard'
import type { BrowseImageDto } from '@/types/browse'

interface PetGridProps {
  pets: BrowseImageDto[]
  isLoading: boolean
  hasActiveFilters: boolean
}

export function PetGrid({ pets, isLoading, hasActiveFilters }: PetGridProps) {
  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <Loader2 className="w-8 h-8 animate-spin text-primary" />
      </div>
    )
  }

  if (!pets || pets.length === 0) {
    const message = hasActiveFilters
      ? 'No furry friends here yet. Try a different breed or pet type!'
      : 'No pets posted yet. Be the first!'

    return (
      <div className="flex flex-col items-center gap-3 py-20 text-muted-foreground">
        <PawPrint className="w-12 h-12" />
        <p className="text-lg">{message}</p>
      </div>
    )
  }

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      {pets.map((pet, index) => (
        <PetCard key={pet.imageId} pet={pet} index={index} />
      ))}
    </div>
  )
}
