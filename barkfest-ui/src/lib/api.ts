import type { BrowseImageDto, PagedResult } from '@/types/browse'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

let unauthorizedHandler: (() => void) | null = null

export function setUnauthorizedHandler(fn: () => void) {
  unauthorizedHandler = fn
}

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
    if (response.status === 401) {
      unauthorizedHandler?.()
      throw new Error('Unauthorized')
    }
    const text = await response.text()
    try {
      const problem = JSON.parse(text)
      throw new Error(problem.detail || problem.title || `HTTP ${response.status}`)
    } catch {
      throw new Error(text || `HTTP ${response.status}`)
    }
  }

  const text = await response.text()
  if (!text) return undefined as T
  return JSON.parse(text) as T
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
}

// ── Auth API ──────────────────────────────────────────────────────────────

export interface LoginResponse {
  accountId: string
  expiresAt: string
}

export function login(username: string, password: string): Promise<LoginResponse> {
  return request<LoginResponse>('/v1/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  })
}

export function adminLogin(username: string, password: string): Promise<LoginResponse> {
  return request<LoginResponse>('/v1/auth/admin/login', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  })
}

export interface RegisterRequest {
  username: string
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string
  password: string
}

export function register(data: RegisterRequest): Promise<void> {
  return request<void>('/v1/auth/register', {
    method: 'POST',
    body: JSON.stringify(data),
  })
}

export function logout(): Promise<void> {
  return request<void>('/v1/auth/logout', { method: 'POST' })
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
