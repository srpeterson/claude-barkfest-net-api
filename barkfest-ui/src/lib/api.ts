import type { BrowseImageDto, PagedResult } from '@/types/browse'
import type { AddPetImagesResult, CreatePetRequest } from '@/types/pet'
import type { OwnerDto, UpdateOwnerRequest } from '@/types/owner'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

let authToken: string | null = null
let unauthorizedHandler: (() => void) | null = null

export function setAuthToken(token: string | null) {
  authToken = token
}

export function setUnauthorizedHandler(fn: () => void) {
  unauthorizedHandler = fn
}

export class ApiError extends Error {
  readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  }

  if (authToken) {
    headers['Authorization'] = `Bearer ${authToken}`
  }

  const response = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers,
  })

  if (!response.ok) {
    if (response.status === 401) {
      unauthorizedHandler?.()
      throw new ApiError('Unauthorized', 401)
    }
    const text = await response.text()
    let message = text || `HTTP ${response.status}`
    try {
      const problem = JSON.parse(text)
      message = problem.detail || problem.title || message
    } catch { /* use raw text */ }
    throw new ApiError(message, response.status)
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
  accessToken: string
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
  displayName?: string
}

export function register(data: RegisterRequest): Promise<void> {
  return request<void>('/v1/auth/register', {
    method: 'POST',
    body: JSON.stringify(data),
  })
}

export async function checkDisplayName(value: string): Promise<boolean> {
  const result = await request<{ available: boolean }>(
    `/v1/auth/check-display-name?value=${encodeURIComponent(value)}`
  )
  return result.available
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

// ── Pet API ───────────────────────────────────────────────────────────────

// Multipart helper — does NOT set Content-Type so the browser can supply the
// boundary automatically. Used for image uploads only.
async function requestMultipart<T>(path: string, body: FormData): Promise<T> {
  const headers: Record<string, string> = {}

  if (authToken) {
    headers['Authorization'] = `Bearer ${authToken}`
  }

  const response = await fetch(`${BASE_URL}${path}`, {
    method: 'POST',
    headers,
    body,
  })

  if (!response.ok) {
    if (response.status === 401) {
      unauthorizedHandler?.()
      throw new Error('Unauthorized')
    }
    const text = await response.text()
    let message = `HTTP ${response.status}`
    try {
      const problem = JSON.parse(text)
      message = problem.detail || problem.title || message
    } catch { /* use status code fallback */ }
    throw new Error(message)
  }

  const text = await response.text()
  if (!text) return undefined as T
  return JSON.parse(text) as T
}

// POST /v1/pets — returns the new pet id extracted from the Location header.
export async function createPet(data: CreatePetRequest): Promise<string> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  }

  if (authToken) {
    headers['Authorization'] = `Bearer ${authToken}`
  }

  const response = await fetch(`${BASE_URL}/v1/pets`, {
    method: 'POST',
    headers,
    body: JSON.stringify(data),
  })

  if (!response.ok) {
    if (response.status === 401) {
      unauthorizedHandler?.()
      throw new Error('Unauthorized')
    }
    const text = await response.text()
    let message = `HTTP ${response.status}`
    try {
      const problem = JSON.parse(text)
      message = problem.detail || problem.title || message
    } catch { /* use status code fallback */ }
    throw new Error(message)
  }

  const location = response.headers.get('Location')
  if (!location) throw new Error('No Location header in response')
  return location.split('/').pop()!
}

// POST /v1/pets/{id}/images — uploads files as multipart/form-data.
export function addPetImages(petId: string, files: File[]): Promise<AddPetImagesResult> {
  const form = new FormData()
  files.forEach(file => form.append('files', file))
  return requestMultipart<AddPetImagesResult>(`/v1/pets/${petId}/images`, form)
}

// PUT /v1/pets/{id}/images/{imageId}/featured
export function setFeaturedImage(petId: string, imageId: string): Promise<void> {
  return request<void>(`/v1/pets/${petId}/images/${imageId}/featured`, { method: 'PUT' })
}

// ── Owner API ─────────────────────────────────────────────────────────────

export function getOwnerById(id: string): Promise<OwnerDto> {
  return request<OwnerDto>(`/v1/owners/${id}`)
}

export function updateOwner(id: string, data: UpdateOwnerRequest): Promise<void> {
  return request<void>(`/v1/owners/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data),
  })
}

export function uploadOwnerProfileImage(id: string, file: File): Promise<void> {
  const form = new FormData()
  form.append('file', file)
  return requestMultipart<void>(`/v1/owners/${id}/profile-image`, form)
}

export function removeOwnerProfileImage(id: string): Promise<void> {
  return request<void>(`/v1/owners/${id}/profile-image`, { method: 'DELETE' })
}
