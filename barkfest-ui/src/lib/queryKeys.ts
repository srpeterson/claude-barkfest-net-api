// Centralized React Query keys — the single source of truth for cache identity.
// Read sites use the `*List`/builder functions for full keys; invalidations rely on
// React Query's prefix matching (e.g. invalidating `browseImages` clears every
// `browseImagesList(page, …)` entry, and `owner(id)` clears `ownerMeta(id)` /
// `ownerProfileImage(id)` since those extend the same prefix).
export const queryKeys = {
  // ── Browse (public gallery) ──
  browseImages: ['browse', 'images'] as const,
  browseImagesList: (page: number, petTypeValue: number, breedValue: number) =>
    ['browse', 'images', page, petTypeValue, breedValue] as const,
  browseHeroStrip: ['browse', 'hero-strip'] as const,
  browseHeroStripList: (petTypeValue: number, breedValue: number) =>
    ['browse', 'hero-strip', petTypeValue, breedValue] as const,
  browsePetTypes: ['browse', 'pet-types'] as const,
  browseBreeds: (petTypeValue: number) => ['browse', 'breeds', petTypeValue] as const,

  // ── Owner ──
  owner: (id: string) => ['owner', id] as const,
  ownerPets: (id: string) => ['owner', 'pets', id] as const,
  ownerMeta: (id: string) => ['owner', id, 'meta'] as const,
  ownerProfileImage: (id: string) => ['owner', id, 'profile-image'] as const,

  // ── Pet ──
  pet: (id: string) => ['pet', id] as const,
}
