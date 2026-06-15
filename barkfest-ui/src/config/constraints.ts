// Form field length limits. These mirror the constraints in Barkfest.Domain —
// keep them in sync with the backend.
export const LIMITS = {
  // FE intentionally stricter than AccountConstraints.UsernameMaxLength (50).
  username: 25,
  email: 75, //          AccountConstraints.EmailMaxLength
  passwordMin: 8, //     AccountConstraints.PasswordMinLength
  passwordMax: 72, //    AccountConstraints.PasswordMaxLength
  displayName: 25, //    Owner.DisplayNameMaxLength
  firstName: 50, //      Owner.FirstNameMaxLength
  lastName: 100, //      Owner.LastNameMaxLength
  petName: 75, //        Pet.NameMaxLength
  petDescription: 300, // FE-only soft cap (the domain has no Description max)
} as const
