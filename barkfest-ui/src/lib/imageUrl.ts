export function getBlobImageUrl(blobName: string): string {
  const base = import.meta.env.VITE_BLOB_BASE_URL ?? ''
  return `${base}/pet-images/${blobName}`
}
