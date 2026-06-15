import isEmail from 'validator/lib/isEmail'

/** Loose client-side email check for UX. The backend domain is the authoritative validator. */
export function isValidEmail(value: string): boolean {
  return isEmail(value.trim())
}
