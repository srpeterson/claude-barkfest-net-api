// Maps API values → display labels.
// Update this file when a new PetType SmartEnum is added to the backend.
export const PET_TYPE_LABELS: Record<string, string> = {
  Dog: 'Doggies',
  Cat: 'Kitties',
}

export function getPetTypeLabel(apiValue: string): string {
  return PET_TYPE_LABELS[apiValue] ?? apiValue
}

export const MAX_PETS_PER_OWNER = 10
