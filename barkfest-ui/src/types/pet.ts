// ── Create Pet ────────────────────────────────────────────────────────────

export interface CreatePetRequest {
  name: string
  petType: string
  breed: string
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
