export function formatAge(dateOfBirth: string | undefined, format: 'short' | 'long' = 'long'): string | null {
  if (!dateOfBirth) return null

  const dob = new Date(dateOfBirth)
  const today = new Date()

  let months = (today.getFullYear() - dob.getFullYear()) * 12
    + (today.getMonth() - dob.getMonth())
  if (today.getDate() < dob.getDate()) months--
  months = Math.max(months, 0)

  if (format === 'short') {
    if (months < 12) return months === 1 ? '1 mo' : `${months} mo`
    const years = Math.floor(months / 12)
    return years === 1 ? '1 yr' : `${years} yr`
  }

  if (months < 12) return months === 1 ? '1 month old' : `${months} months old`
  const years = Math.floor(months / 12)
  return years === 1 ? '1 year old' : `${years} years old`
}
