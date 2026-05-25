export function getBlobImageUrl(blobName: string): string {
  const base = import.meta.env.VITE_API_BASE_URL ?? ''
  return `${base}/v1/images/pet-images/${blobName}`
}
