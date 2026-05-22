import { Calendar } from 'lucide-react'
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { getBlobImageUrl } from '@/lib/imageUrl'
import type { BrowseImageDto } from '@/types/browse'

interface PetCardProps {
  pet: BrowseImageDto
  index: number
}

export function PetCard({ pet, index }: PetCardProps) {
  return (
    <div
      className="animate-fade-in-up"
      style={{ animationDelay: `${index * 0.06}s`, opacity: 0 }}
    >
      <Card className="group overflow-hidden border-0 shadow-sm hover:shadow-xl transition-shadow duration-500 bg-card">
        {/* Image */}
        <div className="aspect-square overflow-hidden">
          <img
            src={getBlobImageUrl(pet.blobName)}
            alt={pet.petName}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700"
          />
        </div>

        {/* Body */}
        <div className="p-4 space-y-2">
          <div className="flex items-center justify-between">
            <h3 className="font-heading text-lg font-semibold">{pet.petName}</h3>
            {pet.age != null && (
              <Badge variant="secondary" className="gap-1 text-xs font-sans">
                <Calendar size={12} />
                {pet.age}y
              </Badge>
            )}
          </div>

          {pet.petDescription && (
            <p className="text-sm text-muted-foreground leading-relaxed line-clamp-2">
              {pet.petDescription}
            </p>
          )}

          {pet.breed && (
            <p className="text-xs text-muted-foreground">{pet.breed}</p>
          )}
        </div>
      </Card>
    </div>
  )
}
