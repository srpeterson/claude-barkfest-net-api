// Shared image-upload config for react-dropzone across all upload flows.

// Mirrors PetImage.MaxImageSizeBytes in Barkfest.Domain (10 MB).
export const MAX_IMAGE_SIZE_BYTES = 10 * 1024 * 1024

// Accepted image types (MIME → allowed extensions).
export const IMAGE_ACCEPT = {
  'image/jpeg': ['.jpg', '.jpeg'],
  'image/png': ['.png'],
}
