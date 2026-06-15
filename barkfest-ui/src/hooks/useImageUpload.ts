import { useCallback, useState } from 'react'
import { useDropzone } from 'react-dropzone'
import type { FileRejection } from 'react-dropzone'
import { useObjectUrls } from '@/hooks/useObjectUrls'
import { IMAGE_ACCEPT, MAX_IMAGE_SIZE_BYTES } from '@/lib/imageUpload'

export interface ImageFile {
  file: File
  previewUrl: string
}

interface UseImageUploadOptions {
  maxFiles: number
  maxSizeBytes?: number
}

export function useImageUpload({
  maxFiles,
  maxSizeBytes = MAX_IMAGE_SIZE_BYTES,
}: UseImageUploadOptions) {
  const [images, setImages] = useState<ImageFile[]>([])
  const [featuredIndex, setFeaturedIndex] = useState(0)
  const [uploadError, setUploadError] = useState<string | null>(null)

  const objectUrls = useObjectUrls()

  const onDrop = useCallback((accepted: File[], rejected: FileRejection[]) => {
    setUploadError(null)

    if (rejected.length > 0) {
      const tooLarge = rejected.some(r => r.errors.some(e => e.code === 'file-too-large'))
      const badType  = rejected.some(r => r.errors.some(e => e.code === 'file-invalid-type'))
      if (tooLarge && badType) {
        setUploadError('Some files were skipped: only JPG or PNG files under 10 MB are accepted.')
      } else if (tooLarge) {
        setUploadError('Some files were skipped: each photo must be 10 MB or smaller.')
      } else if (badType) {
        setUploadError('Some files were skipped: only JPG and PNG photos are accepted.')
      }
    }

    const remaining = maxFiles - images.length
    const newFiles = accepted.slice(0, remaining).map(file => ({
      file,
      previewUrl: objectUrls.create(file),
    }))
    setImages(prev => [...prev, ...newFiles])
  }, [images.length, maxFiles, objectUrls])

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: IMAGE_ACCEPT,
    maxSize: maxSizeBytes,
    multiple: maxFiles > 1,
    disabled: images.length >= maxFiles,
  })

  function removeImage(index: number) {
    const next = images.filter((_, i) => i !== index)
    setImages(next)
    if (featuredIndex >= next.length) {
      setFeaturedIndex(Math.max(0, next.length - 1))
    } else if (index < featuredIndex) {
      setFeaturedIndex(featuredIndex - 1)
    }
  }

  return {
    images,
    featuredIndex,
    setFeaturedIndex,
    removeImage,
    uploadError,
    setUploadError,
    getRootProps,
    getInputProps,
    isDragActive,
  }
}
