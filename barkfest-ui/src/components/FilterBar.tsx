import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { PetTypeBreedSelector } from '@/components/PetTypeBreedSelector'
import { useIsMobile } from '@/hooks/useIsMobile'
import { getBrowseBreeds, getBrowsePetTypes } from '@/lib/api'
import { getPetTypeLabel } from '@/config/petTypes'

interface FilterBarProps {
  petTypeValue: number
  onPetTypeChange: (value: number) => void
  breedValue: number
  onBreedChange: (value: number) => void
}

// SVG icon components — no new lucide imports needed
function FilterIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <line x1="4" y1="6" x2="20" y2="6"/>
      <line x1="8" y1="12" x2="16" y2="12"/>
      <line x1="11" y1="18" x2="13" y2="18"/>
    </svg>
  )
}

function XIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
      <path d="M18 6 6 18"/><path d="m6 6 12 12"/>
    </svg>
  )
}

function SearchIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
      <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
    </svg>
  )
}

function CheckIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><polyline points="20 6 9 17 4 12"/></svg>
  )
}

export function FilterBar({ petTypeValue, onPetTypeChange, breedValue, onBreedChange }: FilterBarProps) {
  const isMobile = useIsMobile()
  const [sheetOpen, setSheetOpen] = useState(false)
  const [pendingType, setPendingType] = useState(petTypeValue)
  const [pendingBreed, setPendingBreed] = useState(breedValue)
  const [breedSearch, setBreedSearch] = useState('')

  const hasActiveFilters = petTypeValue !== 0 || breedValue !== 0

  const { data: petTypes = [] } = useQuery({
    queryKey: ['browse', 'pet-types'],
    queryFn: getBrowsePetTypes,
    staleTime: Infinity,
  })

  const { data: rawBreeds = [] } = useQuery({
    queryKey: ['browse', 'breeds', pendingType],
    queryFn: () => getBrowseBreeds(pendingType),
    enabled: !!pendingType,
    staleTime: Infinity,
  })

  const filteredBreeds = [
    { name: 'All Breeds', value: 0 } as const,
    ...rawBreeds
      .filter(b => b.name.toLowerCase().includes(breedSearch.toLowerCase()))
      .sort((a, b) => {
        if (a.name === 'Other') return 1
        if (b.name === 'Other') return -1
        if (a.name === 'Mixed') return 1
        if (b.name === 'Mixed') return -1
        return a.name.localeCompare(b.name)
      }),
  ]

  function openSheet() {
    setPendingType(petTypeValue)
    setPendingBreed(breedValue)
    setBreedSearch('')
    setSheetOpen(true)
  }

  function applyFilters() {
    if (pendingType !== petTypeValue) {
      onPetTypeChange(pendingType)
      onBreedChange(0)
    } else {
      onBreedChange(pendingBreed)
    }
    setSheetOpen(false)
  }

  function getFilterLabel() {
    if (!petTypeValue) return 'All Pets'
    const pt = petTypes.find(p => p.value === petTypeValue)
    const typeName = pt ? getPetTypeLabel(pt.name) : 'Pets'
    if (!breedValue) return typeName
    const breed = rawBreeds.find(b => b.value === breedValue)
    return breed ? breed.name : typeName
  }

  /* ── Mobile ──────────────────────────────────────────────────────── */
  if (isMobile) {
    return (
      <>
        {/* Filter pill — highlighted when filters are active */}
        <div
          style={{
            position: 'sticky',
            top: 64,
            zIndex: 40,
            padding: '10px 16px 4px',
            display: 'flex',
            justifyContent: 'center',
          }}
        >
          <button
            onClick={openSheet}
            style={{
              height: 44,
              padding: '0 16px',
              borderRadius: 22,
              display: 'flex',
              alignItems: 'center',
              gap: 7,
              border: `1.5px solid ${hasActiveFilters ? 'var(--primary)' : 'var(--border)'}`,
              background: hasActiveFilters ? 'var(--primary-10)' : 'var(--card)',
              color: hasActiveFilters ? 'var(--primary)' : 'var(--foreground)',
              fontFamily: "'DM Sans', sans-serif",
              fontSize: 13,
              fontWeight: hasActiveFilters ? 600 : 400,
              cursor: 'pointer',
            }}
          >
            <FilterIcon />
            {getFilterLabel()}
            {hasActiveFilters && (
              <span
                style={{
                  width: 6,
                  height: 6,
                  borderRadius: '50%',
                  background: 'var(--primary)',
                  flexShrink: 0,
                }}
              />
            )}
          </button>
        </div>

        {/* Bottom sheet */}
        {sheetOpen && (
          <>
            {/* Backdrop */}
            <div
              onClick={() => setSheetOpen(false)}
              className="animate-backdrop-in"
              style={{
                position: 'fixed',
                inset: 0,
                zIndex: 200,
                background: 'rgba(0,0,0,0.3)',
                backdropFilter: 'blur(2px)',
              }}
            />

            {/* Sheet */}
            <div
              className="animate-sheet-in"
              style={{
                position: 'fixed',
                bottom: 0,
                left: 0,
                right: 0,
                zIndex: 201,
                background: 'var(--card)',
                borderRadius: '20px 20px 0 0',
                boxShadow: '0 -8px 40px rgba(0,0,0,0.15)',
                display: 'flex',
                flexDirection: 'column',
                maxHeight: '80vh',
              }}
            >
              {/* Drag handle */}
              <div style={{ display: 'flex', justifyContent: 'center', padding: '12px 0 4px', flexShrink: 0 }}>
                <div style={{ width: 36, height: 4, borderRadius: 2, background: 'var(--border)' }} />
              </div>

              {/* Header */}
              <div
                style={{
                  padding: '8px 20px 14px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  flexShrink: 0,
                }}
              >
                <h3
                  className="font-heading"
                  style={{ margin: 0, fontSize: 18, fontWeight: 700, color: 'var(--foreground)' }}
                >
                  Filters
                </h3>
                <button
                  onClick={() => setSheetOpen(false)}
                  style={{
                    width: 32,
                    height: 32,
                    borderRadius: 8,
                    border: 'none',
                    background: 'var(--secondary)',
                    cursor: 'pointer',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: 'var(--muted-foreground)',
                  }}
                >
                  <XIcon />
                </button>
              </div>

              {/* Scrollable content */}
              <div style={{ overflowY: 'auto', padding: '0 20px', flex: 1 }}>
                {/* Pet Type chips */}
                <div style={{ marginBottom: 20 }}>
                  <p
                    style={{
                      margin: '0 0 10px',
                      fontSize: 11,
                      fontWeight: 700,
                      letterSpacing: '0.08em',
                      textTransform: 'uppercase',
                      color: 'var(--muted-foreground)',
                    }}
                  >
                    Pet Type
                  </p>
                  <div style={{ display: 'flex', gap: 8 }}>
                    {[{ value: 0, label: 'All Pets' }, ...petTypes.map(pt => ({ value: pt.value, label: getPetTypeLabel(pt.name) }))].map(
                      ({ value, label }) => (
                        <button
                          key={value}
                          onClick={() => { setPendingType(value); setPendingBreed(0) }}
                          style={{
                            height: 40,
                            padding: '0 18px',
                            borderRadius: 20,
                            border: `1.5px solid ${pendingType === value ? 'var(--primary)' : 'var(--border)'}`,
                            background: pendingType === value ? 'var(--primary-10)' : 'transparent',
                            color: pendingType === value ? 'var(--primary)' : 'var(--foreground)',
                            fontFamily: "'DM Sans', sans-serif",
                            fontSize: 13,
                            fontWeight: pendingType === value ? 600 : 400,
                            cursor: 'pointer',
                            transition: 'all 0.15s',
                          }}
                        >
                          {label}
                        </button>
                      )
                    )}
                  </div>
                </div>

                {/* Breed list — only when a type is selected */}
                {!!pendingType && (
                  <div style={{ marginBottom: 16 }}>
                    <p
                      style={{
                        margin: '0 0 10px',
                        fontSize: 11,
                        fontWeight: 700,
                        letterSpacing: '0.08em',
                        textTransform: 'uppercase',
                        color: 'var(--muted-foreground)',
                      }}
                    >
                      Breed
                      <span
                        style={{
                          fontWeight: 400,
                          textTransform: 'none',
                          letterSpacing: 0,
                          marginLeft: 6,
                          fontSize: 12,
                        }}
                      >
                        ({rawBreeds.length} breeds)
                      </span>
                    </p>

                    {/* Search box */}
                    <div style={{ position: 'relative', marginBottom: 8 }}>
                      <span
                        style={{
                          position: 'absolute',
                          left: 12,
                          top: '50%',
                          transform: 'translateY(-50%)',
                          color: 'var(--muted-foreground)',
                          pointerEvents: 'none',
                          display: 'flex',
                        }}
                      >
                        <SearchIcon />
                      </span>
                      <input
                        placeholder="Search breeds…"
                        value={breedSearch}
                        onChange={e => setBreedSearch(e.target.value)}
                        style={{
                          width: '100%',
                          height: 40,
                          borderRadius: 10,
                          border: '1.5px solid var(--border)',
                          background: 'var(--secondary)',
                          padding: '0 36px 0 34px',
                          fontFamily: "'DM Sans', sans-serif",
                          fontSize: 13,
                          outline: 'none',
                          boxSizing: 'border-box',
                          color: 'var(--foreground)',
                        }}
                      />
                      {breedSearch && (
                        <button
                          onClick={() => setBreedSearch('')}
                          style={{
                            position: 'absolute',
                            right: 10,
                            top: '50%',
                            transform: 'translateY(-50%)',
                            background: 'none',
                            border: 'none',
                            cursor: 'pointer',
                            color: 'var(--muted-foreground)',
                            padding: 0,
                            display: 'flex',
                          }}
                        >
                          <XIcon />
                        </button>
                      )}
                    </div>

                    {/* Scrollable breed list */}
                    <div
                      style={{
                        border: '1.5px solid var(--border)',
                        borderRadius: 12,
                        overflow: 'hidden',
                        maxHeight: 220,
                        overflowY: 'auto',
                      }}
                    >
                      {filteredBreeds.length === 0 ? (
                        <p
                          style={{
                            padding: '14px 16px',
                            margin: 0,
                            fontSize: 13,
                            color: 'var(--muted-foreground)',
                            textAlign: 'center',
                          }}
                        >
                          No breeds found
                        </p>
                      ) : (
                        filteredBreeds.map((b, i) => (
                          <button
                            key={b.value}
                            onClick={() => setPendingBreed(b.value)}
                            style={{
                              width: '100%',
                              height: 44,
                              padding: '0 16px',
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'space-between',
                              background: pendingBreed === b.value ? 'var(--primary-10)' : 'transparent',
                              border: 'none',
                              borderBottom:
                                i < filteredBreeds.length - 1 ? '1px solid var(--border)' : 'none',
                              cursor: 'pointer',
                              fontFamily: "'DM Sans', sans-serif",
                              fontSize: 14,
                              fontWeight: pendingBreed === b.value ? 600 : 400,
                              color: pendingBreed === b.value ? 'var(--primary)' : 'var(--foreground)',
                              textAlign: 'left',
                            }}
                          >
                            {b.name}
                            {pendingBreed === b.value && (
                              <span style={{ color: 'var(--primary)', display: 'flex' }}>
                                <CheckIcon />
                              </span>
                            )}
                          </button>
                        ))
                      )}
                    </div>
                  </div>
                )}
              </div>

              {/* Show results CTA */}
              <div style={{ padding: '12px 20px 32px', flexShrink: 0 }}>
                <button
                  onClick={applyFilters}
                  style={{
                    width: '100%',
                    height: 48,
                    borderRadius: 12,
                    border: 'none',
                    background: 'var(--primary)',
                    color: '#fff',
                    fontFamily: "'DM Sans', sans-serif",
                    fontSize: 15,
                    fontWeight: 600,
                    cursor: 'pointer',
                  }}
                >
                  Show results
                </button>
              </div>
            </div>
          </>
        )}
      </>
    )
  }

  /* ── Desktop ─────────────────────────────────────────────────────── */
  return (
    <div
      style={{
        position: 'sticky',
        top: 64,
        zIndex: 40,
        padding: '16px 16px',
      }}
    >
      <div
        style={{
          maxWidth: '72rem',
          margin: '0 auto',
          display: 'flex',
          gap: 12,
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <span
          style={{
            fontSize: 14,
            fontWeight: 700,
            color: 'var(--primary)',
          }}
        >
          Show me:
        </span>
        <PetTypeBreedSelector
          petTypeValue={petTypeValue}
          onPetTypeChange={onPetTypeChange}
          breedValue={breedValue}
          onBreedChange={onBreedChange}
          petTypeClassName="w-40"
          breedClassName="w-48"
        />
      </div>
    </div>
  )
}
