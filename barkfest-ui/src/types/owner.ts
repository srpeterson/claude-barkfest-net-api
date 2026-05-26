export interface ProfileImageDto {
  blobName: string
  contentType: string
}

export interface OwnerDto {
  id: string
  username: string
  displayName: string | null
  firstName: string
  lastName: string
  email: string
  phoneNumber: string | null
  isVisible: boolean
  profileImage: ProfileImageDto | null
  createdAt: string
}

export interface UpdateOwnerRequest {
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string | null
  displayName?: string | null
}
