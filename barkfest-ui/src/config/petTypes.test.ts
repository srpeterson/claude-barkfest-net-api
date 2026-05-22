import { getPetTypeLabel, PET_TYPE_LABELS } from './petTypes'

describe('PET_TYPE_LABELS', () => {
  it('contains an entry for Dog', () => {
    expect(PET_TYPE_LABELS['Dog']).toBeDefined()
  })

  it('contains an entry for Cat', () => {
    expect(PET_TYPE_LABELS['Cat']).toBeDefined()
  })
})

describe('getPetTypeLabel', () => {
  it('returns the mapped label for a known pet type', () => {
    expect(getPetTypeLabel('Dog')).toBe('Doggies')
    expect(getPetTypeLabel('Cat')).toBe('Kitties')
  })

  it('returns the raw API value when no mapping exists', () => {
    expect(getPetTypeLabel('Rabbit')).toBe('Rabbit')
  })

  it('is case-sensitive — unrecognised casing falls back to raw value', () => {
    expect(getPetTypeLabel('dog')).toBe('dog')
    expect(getPetTypeLabel('CAT')).toBe('CAT')
  })
})
