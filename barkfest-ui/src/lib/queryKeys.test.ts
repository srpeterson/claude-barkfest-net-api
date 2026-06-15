import { describe, expect, it } from 'vitest'
import { queryKeys } from './queryKeys'

describe('queryKeys', () => {
  it('builds browse keys with the expected shape', () => {
    expect(queryKeys.browseImages).toEqual(['browse', 'images'])
    expect(queryKeys.browseImagesList(2, 1, 5)).toEqual(['browse', 'images', 2, 1, 5])
    expect(queryKeys.browseHeroStrip).toEqual(['browse', 'hero-strip'])
    expect(queryKeys.browseHeroStripList(1, 5)).toEqual(['browse', 'hero-strip', 1, 5])
    expect(queryKeys.browsePetTypes).toEqual(['browse', 'pet-types'])
    expect(queryKeys.browseBreeds(1)).toEqual(['browse', 'breeds', 1])
  })

  it('builds owner and pet keys with the expected shape', () => {
    expect(queryKeys.owner('o1')).toEqual(['owner', 'o1'])
    expect(queryKeys.ownerPets('o1')).toEqual(['owner', 'pets', 'o1'])
    expect(queryKeys.ownerMeta('o1')).toEqual(['owner', 'o1', 'meta'])
    expect(queryKeys.ownerProfileImage('o1')).toEqual(['owner', 'o1', 'profile-image'])
    expect(queryKeys.pet('p1')).toEqual(['pet', 'p1'])
  })

  it('keeps browse list keys under the invalidation prefix', () => {
    // invalidateBrowse relies on prefix matching, so browseImages must prefix browseImagesList.
    const prefix = queryKeys.browseImages
    const full = queryKeys.browseImagesList(2, 1, 5)
    expect(full.slice(0, prefix.length)).toEqual([...prefix])
  })

  it('nests owner detail as a prefix of ownerMeta and ownerProfileImage', () => {
    // Invalidating owner(id) must also clear ownerMeta(id) / ownerProfileImage(id).
    const base = queryKeys.owner('o1')
    expect(queryKeys.ownerMeta('o1').slice(0, base.length)).toEqual([...base])
    expect(queryKeys.ownerProfileImage('o1').slice(0, base.length)).toEqual([...base])
  })
})
