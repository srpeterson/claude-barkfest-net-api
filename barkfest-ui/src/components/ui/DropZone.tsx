import { Upload } from 'lucide-react'
import { cn } from '@/lib/utils'
import type { useDropzone } from 'react-dropzone'

interface DropZoneProps {
  getRootProps: ReturnType<typeof useDropzone>['getRootProps']
  getInputProps: ReturnType<typeof useDropzone>['getInputProps']
  isDragActive: boolean
  hint?: string
}

export function DropZone({ getRootProps, getInputProps, isDragActive, hint }: DropZoneProps) {
  return (
    <div
      {...getRootProps()}
      className={cn(
        'w-full border-2 border-dashed rounded-xl py-7 flex flex-col items-center gap-2 cursor-pointer transition-colors',
        isDragActive
          ? 'border-primary bg-primary/10'
          : 'border-border hover:border-primary/50 hover:bg-primary/5'
      )}
    >
      <input {...getInputProps()} />
      <Upload className="w-6 h-6 text-muted-foreground" />
      <span className="text-sm text-muted-foreground">
        {isDragActive ? 'Drop photos here...' : 'Drag & drop or click to upload'}
      </span>
      {hint && (
        <span className="text-xs text-muted-foreground/70">{hint}</span>
      )}
    </div>
  )
}
