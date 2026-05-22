export interface Pet {
  id: string
  name: string
  age?: string
  description?: string
  image_url: string
  breed?: string
  type?: 'Dog' | 'Cat' | string
  owner_id?: string
  created_date?: string
}
