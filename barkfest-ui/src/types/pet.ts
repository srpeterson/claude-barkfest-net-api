// ── Create Pet ────────────────────────────────────────────────────────────

export interface CreatePetRequest {
  name: string
  petTypeValue: number
  breedValue: number
  dateOfBirth: string    // ISO date "YYYY-MM-DD" — maps to DateOnly on the API
  description?: string
}

// ── Add Pet Images ────────────────────────────────────────────────────────

export interface PetImageUploadResult {
  fileName: string
  success: boolean
  imageId: string | null
  failureReason: string | null
}

export interface AddPetImagesResult {
  results: PetImageUploadResult[]
}

// ── Update Pet ────────────────────────────────────────────────────────────

export interface UpdatePetRequest {
  name: string
  petTypeValue: number
  breedValue: number
  dateOfBirth?: string
  description?: string
}
