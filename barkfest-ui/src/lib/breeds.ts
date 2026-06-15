import type { BreedOption } from '@/lib/api'

// Catch-all breeds pinned to the end of the list, in this order.
const PINNED_LAST = ['Mixed', 'Other']

const breedRank = (name: string) => {
  const i = PINNED_LAST.indexOf(name)
  return i === -1 ? 0 : i + 1 // 0 = normal (sorts alphabetically); 1, 2… = pinned
}

/** Sort breeds alphabetically, with the catch-all "Mixed" and "Other" pinned to the end. */
export function sortBreeds(breeds: BreedOption[]): BreedOption[] {
  return [...breeds].sort(
    (a, b) => breedRank(a.name) - breedRank(b.name) || a.name.localeCompare(b.name)
  )
}
