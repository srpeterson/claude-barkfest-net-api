import { useQuery } from '@tanstack/react-query'
import { getBrowseBreeds, getBrowsePetTypes } from '@/lib/api'
import { sortBreeds } from '@/lib/breeds'
import { queryKeys } from '@/lib/queryKeys'

/** Pet-type options from the browse API. Cached for the session.
 *  Pass `enabled={false}` to defer the fetch (e.g. when no filter is active). */
export function usePetTypeOptions(enabled = true) {
  return useQuery({
    queryKey: queryKeys.browsePetTypes,
    queryFn: getBrowsePetTypes,
    enabled,
    staleTime: Infinity,
  })
}

/** Breed options for a pet type, sorted (Mixed/Other pinned last).
 *  Disabled until a pet type is selected. */
export function useBreedOptions(petTypeValue: number) {
  return useQuery({
    queryKey: queryKeys.browseBreeds(petTypeValue),
    queryFn: () => getBrowseBreeds(petTypeValue),
    enabled: !!petTypeValue,
    staleTime: Infinity,
    select: sortBreeds,
  })
}
