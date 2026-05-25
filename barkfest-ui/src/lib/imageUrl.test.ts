import { getBlobImageUrl } from './imageUrl'

const API_BASE = 'https://localhost:7001'

describe('getBlobImageUrl', () => {
  beforeEach(() => {
    vi.stubEnv('VITE_API_BASE_URL', API_BASE)
  })

  afterEach(() => {
    vi.unstubAllEnvs()
  })

  it('constructs a proxied image URL from a blobName', () => {
    const result = getBlobImageUrl('pets/abc/gallery/photo.jpg')
    expect(result).toBe(`${API_BASE}/v1/images/pet-images/pets/abc/gallery/photo.jpg`)
  })

  it('includes the pet-images container segment', () => {
    const result = getBlobImageUrl('pets/abc/gallery/photo.jpg')
    expect(result).toContain('/v1/images/pet-images/')
  })

  it('appends the blobName verbatim', () => {
    const blobName = 'pets/some-id/gallery/some-guid.png'
    const result = getBlobImageUrl(blobName)
    expect(result).toContain(blobName)
  })

  it('falls back to empty string when VITE_API_BASE_URL is not set', () => {
    vi.stubEnv('VITE_API_BASE_URL', '')
    const result = getBlobImageUrl('pets/abc/gallery/photo.jpg')
    expect(result).toBe('/v1/images/pet-images/pets/abc/gallery/photo.jpg')
  })
})
