import { describe, expect, it } from 'vitest'
import { sortBreeds } from './breeds'

describe('sortBreeds', () => {
  it('sorts breeds alphabetically by name', () => {
    const input = [
      { name: 'Poodle', value: 3 },
      { name: 'Beagle', value: 1 },
      { name: 'Labrador', value: 2 },
    ]
    expect(sortBreeds(input).map(b => b.name)).toEqual(['Beagle', 'Labrador', 'Poodle'])
  })

  it('pins Mixed and Other to the end, with Other last', () => {
    const input = [
      { name: 'Other', value: 99 },
      { name: 'Poodle', value: 3 },
      { name: 'Mixed', value: 98 },
      { name: 'Beagle', value: 1 },
    ]
    expect(sortBreeds(input).map(b => b.name)).toEqual(['Beagle', 'Poodle', 'Mixed', 'Other'])
  })

  it('does not mutate the input array', () => {
    const input = [
      { name: 'Poodle', value: 3 },
      { name: 'Beagle', value: 1 },
    ]
    const snapshot = [...input]
    sortBreeds(input)
    expect(input).toEqual(snapshot)
  })

  it('returns an empty array unchanged', () => {
    expect(sortBreeds([])).toEqual([])
  })
})
