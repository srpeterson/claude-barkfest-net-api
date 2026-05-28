interface BarkfestMarkProps {
  inverted?: boolean
  size?: number
  className?: string
}

export function BarkfestMark({ inverted = false, size = 40, className }: BarkfestMarkProps) {
  const fill = 'var(--primary)'

  if (inverted) {
    return (
      <svg
        width={size}
        height={size}
        viewBox="0 0 80 80"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        className={className}
      >
        <rect width="80" height="80" rx="20" fill="white" />
        <rect x="16" y="8" width="14" height="58" rx="7" fill={fill} />
        <circle cx="48" cy="54" r="20" fill={fill} />
        <circle cx="48" cy="54" r="13" fill="white" />
        <circle cx="48" cy="54" r="8.5" fill={fill} />
        <ellipse cx="48" cy="57.5" rx="3" ry="2.5" fill="white" />
        <circle cx="43.5" cy="52" r="1.8" fill="white" />
        <circle cx="48" cy="50.5" r="1.8" fill="white" />
        <circle cx="52.5" cy="52" r="1.8" fill="white" />
      </svg>
    )
  }

  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 80 80"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={className}
    >
      <rect width="80" height="80" rx="20" fill={fill} />
      <rect x="16" y="8" width="14" height="58" rx="7" fill="white" />
      <circle cx="48" cy="54" r="20" fill="white" />
      <circle cx="48" cy="54" r="13" fill={fill} />
      <circle cx="48" cy="54" r="8.5" fill="white" />
      <ellipse cx="48" cy="57.5" rx="3" ry="2.5" fill={fill} />
      <circle cx="43.5" cy="52" r="1.8" fill={fill} />
      <circle cx="48" cy="50.5" r="1.8" fill={fill} />
      <circle cx="52.5" cy="52" r="1.8" fill={fill} />
    </svg>
  )
}
