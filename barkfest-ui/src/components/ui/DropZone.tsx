import { Upload } from 'lucide-react'
import { cn } from '@/lib/utils'
import type { useDropzone } from 'react-dropzone'

interface DropZoneProps {
  getRootProps: ReturnType<typeof useDropzone>['getRootProps']
  getInputProps: ReturnType<typeof useDropzone>['getInputProps']
  isDragActive: boolean
  hint?: string
  disabled?: boolean
}

export function DropZone({ getRootProps, getInputProps, isDragActive, hint, disabled }: DropZoneProps) {
  return (
    <div
      {...getRootProps()}
      className={cn(
        'w-full border-2 border-dashed rounded-xl py-7 flex flex-col items-center gap-2 transition-colors',
        disabled
          ? 'border-border opacity-50 cursor-not-allowed'
          : isDragActive
            ? 'border-primary bg-primary/10 cursor-pointer'
            : 'border-border hover:border-primary/50 hover:bg-primary/5 cursor-pointer'
      )}
    >
      <input {...getInputProps()} />
      <Upload className="w-6 h-6 text-primary" />
      <span className="text-sm text-foreground/70">
        {disabled ? 'Photo limit reached' : isDragActive ? 'Drop photos here...' : 'Drag & drop or click to upload'}
      </span>
      {hint && !disabled && (
        <span className="text-xs text-foreground/50">{hint}</span>
      )}
      {disabled && (
        <span className="text-xs text-foreground/50">Remove a photo to add another</span>
      )}
    </div>
  )
}
