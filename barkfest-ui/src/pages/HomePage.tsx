import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Navbar } from '@/components/Navbar'
import { HeroSection } from '@/components/HeroSection'
import { PetGrid } from '@/components/PetGrid'
import { BarkfestMark } from '@/components/BarkfestMark'
import { getBrowseImages } from '@/lib/api'
import type { BrowseImageDto, PagedResult } from '@/types/browse'

const PAGE_SIZE = 6

function ChevLeft() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="m15 18-6-6 6-6"/>
    </svg>
  )
}
function ChevRight() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="m9 18 6-6-6-6"/>
    </svg>
  )
}

function Footer() {
  const year = new Date().getFullYear()
  const linkStyle: React.CSSProperties = {
    fontSize: 13,
    color: 'var(--foreground)',
    textDecoration: 'none',
    opacity: 0.6,
    display: 'block',
    marginBottom: 10,
    fontFamily: "'DM Sans', sans-serif",
    transition: 'opacity 0.15s',
  }
  const colHead: React.CSSProperties = {
    margin: '0 0 12px',
    fontSize: 11,
    fontWeight: 700,
    letterSpacing: '0.08em',
    textTransform: 'uppercase',
    color: 'var(--muted-foreground)',
  }

  return (
    <footer
      style={{
        borderTop: '1px solid var(--border)',
        background: 'var(--card)',
        padding: '40px 24px 0',
      }}
    >
      <div
        style={{
          maxWidth: '72rem',
          margin: '0 auto',
          display: 'flex',
          flexWrap: 'wrap',
          gap: 56,
          alignItems: 'flex-start',
        }}
      >
        {/* Brand column */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 14, maxWidth: 220 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <BarkfestMark size={24} />
            <span
              className="font-heading"
              style={{ fontSize: 17, fontWeight: 700, letterSpacing: '-0.02em', color: 'var(--foreground)' }}
            >
              Barkfest
            </span>
          </div>
          <p style={{ margin: 0, fontSize: 13, color: 'var(--muted-foreground)', lineHeight: 1.65 }}>
            A community where pet lovers share photos and stories of their furry friends.
          </p>
        </div>

        {/* Link columns */}
        <div style={{ display: 'flex', gap: 48, flexWrap: 'wrap' }}>
          <div>
            <p style={colHead}>Company</p>
            {['About', 'Blog', 'Careers'].map(l => (
              <p key={l} style={{ ...linkStyle, cursor: 'default' }}>{l}</p>
            ))}
          </div>
          <div>
            <p style={colHead}>Legal</p>
            {['Privacy Policy', 'Terms of Use', 'Contact'].map(l => (
              <p key={l} style={{ ...linkStyle, cursor: 'default' }}>{l}</p>
            ))}
          </div>
        </div>
      </div>

      {/* Bottom bar */}
      <div
        style={{
          maxWidth: '72rem',
          margin: '32px auto 0',
          padding: '16px 0 24px',
          borderTop: '1px solid var(--border)',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: 8,
        }}
      >
        <p style={{ margin: 0, fontSize: 12, color: 'var(--muted-foreground)' }}>
          © {year} Barkfest. All rights reserved.
        </p>
        <p style={{ margin: 0, fontSize: 12, color: 'var(--muted-foreground)' }}>
          Made with love for pet lovers.
        </p>
      </div>
    </footer>
  )
}

export function HomePage() {
  const [page, setPage]               = useState(1)
  const [petTypeValue, setPetTypeValue] = useState(0)
  const [breedValue, setBreedValue]     = useState(0)

  const { data, isLoading } = useQuery<PagedResult<BrowseImageDto>>({
    queryKey: ['browse', 'images', page, petTypeValue, breedValue],
    queryFn: () => getBrowseImages({
      page,
      pageSize: PAGE_SIZE,
      ...(petTypeValue && { petTypeValue }),
      ...(breedValue   && { breedValue }),
    }),
  })

  const pets             = data?.items ?? []
  const hasMore          = data?.hasMore ?? false
  const hasPrev          = page > 1
  const hasActiveFilters = petTypeValue !== 0 || breedValue !== 0

  const handleTypeChange = (val: number) => {
    setPetTypeValue(val)
    setBreedValue(0)
    setPage(1)
  }

  const handleBreedChange = (val: number) => {
    setBreedValue(val)
    setPage(1)
  }

  return (
    <div className="min-h-screen bg-background" style={{ display: 'flex', flexDirection: 'column' }}>
      <Navbar
        filterProps={{
          petTypeValue,
          onPetTypeChange: handleTypeChange,
          breedValue,
          onBreedChange: handleBreedChange,
        }}
      />
      <HeroSection petTypeValue={petTypeValue} breedValue={breedValue} />
      <main className="max-w-6xl mx-auto px-4 sm:px-6 pb-20 space-y-8 -mt-6" style={{ flex: 1, width: '100%' }}>
        <PetGrid pets={pets} isLoading={isLoading} hasActiveFilters={hasActiveFilters} />

        {/* Pagination — only shown when there is more than one page */}
        {(hasPrev || hasMore) && (
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 12, paddingTop: 16 }}>
            <button
              onClick={() => setPage(p => p - 1)}
              disabled={!hasPrev}
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: 6,
                height: 32,
                padding: '0 12px',
                borderRadius: 8,
                border: '1px solid var(--primary)',
                background: 'transparent',
                color: 'var(--primary)',
                fontSize: 13,
                fontWeight: 500,
                cursor: !hasPrev ? 'not-allowed' : 'pointer',
                opacity: !hasPrev ? 0.3 : 1,
                fontFamily: "'DM Sans', sans-serif",
              }}
              onMouseEnter={e => { if (hasPrev) e.currentTarget.style.background = 'var(--primary-10)' }}
              onMouseLeave={e => { e.currentTarget.style.background = 'transparent' }}
            >
              <ChevLeft />Previous
            </button>
            <span
              style={{
                padding: '6px 16px',
                borderRadius: 999,
                background: 'var(--primary-10)',
                color: 'var(--primary)',
                fontSize: 13,
                fontWeight: 600,
              }}
            >
              Page {page}
            </span>
            <button
              onClick={() => setPage(p => p + 1)}
              disabled={!hasMore}
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: 6,
                height: 32,
                padding: '0 12px',
                borderRadius: 8,
                border: '1px solid var(--primary)',
                background: 'transparent',
                color: 'var(--primary)',
                fontSize: 13,
                fontWeight: 500,
                cursor: !hasMore ? 'not-allowed' : 'pointer',
                opacity: !hasMore ? 0.3 : 1,
                fontFamily: "'DM Sans', sans-serif",
              }}
              onMouseEnter={e => { if (hasMore) e.currentTarget.style.background = 'var(--primary-10)' }}
              onMouseLeave={e => { e.currentTarget.style.background = 'transparent' }}
            >
              Next<ChevRight />
            </button>
          </div>
        )}
      </main>
      <Footer />
    </div>
  )
}
