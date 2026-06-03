export interface BrowseImageDto {
  imageId: string
  blobName: string
  contentType: string
  isFeaturedImage: boolean
  createdAt: string
  displayName: string | null
  petId: string
  petName: string
  petDescription?: string
  dateOfBirth?: string
  age?: number
  petType: string
  breed?: string
  likes: number
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  hasMore: boolean
}
