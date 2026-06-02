import { Heart, PawPrint } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { getBlobImageUrl } from '@/lib/imageUrl'
import type { BrowseImageDto } from '@/types/browse'

interface PetCardProps {
  pet: BrowseImageDto
  index: number
}

export function PetCard({ pet, index }: PetCardProps) {
  const navigate = useNavigate()

  return (
    <div
      className="animate-fade-in-up"
      style={{ animationDelay: `${index * 0.06}s`, opacity: 0 }}
    >
      <Card
        className="group overflow-hidden border-0 shadow-sm hover:shadow-xl hover:ring-1 hover:ring-primary/25 transition-shadow duration-500 bg-card cursor-pointer"
        onClick={() => navigate(`/pets/${pet.petId}`)}
      >

        {/* Image with gradient overlay */}
        <div className="aspect-[4/5] overflow-hidden relative">
          <img
            src={getBlobImageUrl(pet.blobName)}
            alt={pet.petName}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700"
          />
          {/* Gradient overlay */}
          <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/10 to-transparent" />

          {/* Name pinned to bottom of image */}
          <div className="absolute bottom-0 left-0 right-0 px-4 pb-3">
            <h3 className="font-heading text-lg font-semibold text-white drop-shadow">
              {pet.petName}
            </h3>
          </div>
        </div>

        {/* Body */}
        <div className="px-4 pt-3 pb-4 space-y-1.5">
          {/* Owner name left, breed badge right */}
          <div className="flex items-center justify-between gap-2">
            <div className="flex items-center gap-1 min-w-0 text-xs text-muted-foreground">
              {pet.displayName && (
                <>
                  <PawPrint size={11} className="text-primary shrink-0" />
                  <span className="truncate">{pet.displayName}</span>
                </>
              )}
            </div>
            <div className="flex items-center gap-1.5 shrink-0">
              <span className="flex items-center gap-1 text-xs text-muted-foreground">
                <Heart size={11} className="fill-rose-500 text-rose-500" />
                {pet.likes}
              </span>
              {pet.breed && (
                <Badge variant="secondary" className="text-xs font-sans font-normal">
                  {pet.breed}
                </Badge>
              )}
            </div>
          </div>

          {pet.petDescription && (
            <p className="text-sm text-muted-foreground leading-relaxed line-clamp-2">
              {pet.petDescription}
            </p>
          )}
        </div>
      </Card>
    </div>
  )
}
