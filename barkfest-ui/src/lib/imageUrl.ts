export function getBlobImageUrl(blobName: string, containerName = 'pet-images'): string {
  const base = import.meta.env.VITE_API_BASE_URL ?? ''
  return `${base}/v1/images/${containerName}/${blobName}`
}
