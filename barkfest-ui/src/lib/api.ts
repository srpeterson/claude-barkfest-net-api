import type { BrowseImageDto, PagedResult } from '@/types/browse'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    ...options,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `HTTP ${response.status}`)
  }

  // 204 No Content — return undefined
  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
}

// ── Browse API ────────────────────────────────────────────────────────────

export interface BrowseImagesParams {
  page: number
  pageSize: number
  petType?: string
  breed?: string
}

export function getBrowseImages(params: BrowseImagesParams): Promise<PagedResult<BrowseImageDto>> {
  const query = new URLSearchParams({
    page:     String(params.page),
    pageSize: String(params.pageSize),
    ...(params.petType && { petType: params.petType }),
    ...(params.breed   && { breed:   params.breed }),
  })
  return request<PagedResult<BrowseImageDto>>(`/v1/browse/images?${query}`)
}

export function getBrowsePetTypes(): Promise<string[]> {
  return request<string[]>('/v1/browse/pet-types')
}

export function getBrowseBreeds(petType: string): Promise<string[]> {
  return request<string[]>(`/v1/browse/breeds?petType=${encodeURIComponent(petType)}`)
}
