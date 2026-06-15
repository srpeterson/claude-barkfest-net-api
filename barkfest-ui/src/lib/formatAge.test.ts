import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { formatAge } from './formatAge'

describe('formatAge', () => {
  // Pin "today" to noon UTC on the 15th so the day-of-month comparisons are
  // stable across timezones (DOBs are also on the 15th / well clear of boundaries).
  beforeEach(() => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-15T12:00:00Z'))
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('returns null when no date of birth is provided', () => {
    expect(formatAge(undefined)).toBeNull()
  })

  it('returns 0 months for a date of birth of today', () => {
    expect(formatAge('2026-06-15')).toBe('0 months old')
  })

  it('formats exactly one month as singular', () => {
    expect(formatAge('2026-05-15')).toBe('1 month old')
  })

  it('formats several months under a year', () => {
    expect(formatAge('2026-02-15')).toBe('4 months old')
  })

  it('formats exactly one year as singular', () => {
    expect(formatAge('2025-06-15')).toBe('1 year old')
  })

  it('formats multiple years', () => {
    expect(formatAge('2023-06-15')).toBe('3 years old')
  })

  it('rolls back a month when the birth day has not been reached yet', () => {
    // Born May 20; today is June 15 — not yet a full month old.
    expect(formatAge('2026-05-20')).toBe('0 months old')
  })

  it('floors a future date of birth at zero months', () => {
    expect(formatAge('2027-06-15')).toBe('0 months old')
  })

  describe('short format', () => {
    it('abbreviates months', () => {
      expect(formatAge('2026-05-15', 'short')).toBe('1 mo')
      expect(formatAge('2026-02-15', 'short')).toBe('4 mo')
    })

    it('abbreviates years', () => {
      expect(formatAge('2025-06-15', 'short')).toBe('1 yr')
      expect(formatAge('2023-06-15', 'short')).toBe('3 yr')
    })
  })
})
