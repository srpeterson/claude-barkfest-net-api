import { getBlobImageUrl } from './imageUrl'

const BLOB_BASE = 'http://127.0.0.1:10000/devstoreaccount1'

describe('getBlobImageUrl', () => {
  beforeEach(() => {
    vi.stubEnv('VITE_BLOB_BASE_URL', BLOB_BASE)
  })

  afterEach(() => {
    vi.unstubAllEnvs()
  })

  it('constructs a full blob URL from a blobName', () => {
    const result = getBlobImageUrl('pets/abc/gallery/photo.jpg')
    expect(result).toBe(`${BLOB_BASE}/pet-images/pets/abc/gallery/photo.jpg`)
  })

  it('includes the pet-images container segment', () => {
    const result = getBlobImageUrl('pets/abc/gallery/photo.jpg')
    expect(result).toContain('/pet-images/')
  })

  it('appends the blobName verbatim', () => {
    const blobName = 'pets/some-id/gallery/some-guid.png'
    const result = getBlobImageUrl(blobName)
    expect(result).toContain(blobName)
  })

  it('falls back to empty string when VITE_BLOB_BASE_URL is not set', () => {
    vi.stubEnv('VITE_BLOB_BASE_URL', '')
    const result = getBlobImageUrl('pets/abc/gallery/photo.jpg')
    expect(result).toBe('/pet-images/pets/abc/gallery/photo.jpg')
  })
})
