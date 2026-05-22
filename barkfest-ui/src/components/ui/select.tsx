import { forwardRef, type SelectHTMLAttributes } from 'react'
import { ChevronDown } from 'lucide-react'
import { cn } from '@/lib/utils'

interface NativeSelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  wrapperClassName?: string
}

/**
 * A styled native <select> element.
 *
 * We deliberately use a native select rather than a headless component so it
 * renders consistently on all platforms and requires zero JS beyond React.
 * Swap for @base-ui/react Select when you need multi-select or custom option
 * rendering.
 */
const NativeSelect = forwardRef<HTMLSelectElement, NativeSelectProps>(
  ({ className, wrapperClassName, children, ...props }, ref) => {
    return (
      <div className={cn('relative', wrapperClassName)}>
        <select
          ref={ref}
          className={cn(
            // Layout
            'h-10 w-full appearance-none',
            // Shape
            'rounded-lg border border-border',
            // Color
            'bg-card text-foreground',
            // Typography
            'px-3 pr-9 py-2 text-sm',
            // Interaction
            'cursor-pointer',
            'focus:outline-none focus:ring-2 focus:ring-ring/40',
            'disabled:cursor-not-allowed disabled:opacity-50',
            className
          )}
          {...props}
        >
          {children}
        </select>
        <ChevronDown
          className="pointer-events-none absolute right-2.5 top-1/2 -translate-y-1/2 text-muted-foreground"
          size={16}
        />
      </div>
    )
  }
)

NativeSelect.displayName = 'NativeSelect'

export { NativeSelect }
