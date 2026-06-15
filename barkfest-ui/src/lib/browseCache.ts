import type { QueryClient } from '@tanstack/react-query'

/**
 * Invalidate both public browse views (the grid + the hero strip) after a
 * mutation that changes what's shown publicly — pet create/edit/delete,
 * like/unlike, visibility toggle, or a change to owner display name / avatar.
 */
export function invalidateBrowse(queryClient: QueryClient) {
  queryClient.invalidateQueries({ queryKey: ['browse', 'images'] })
  queryClient.invalidateQueries({ queryKey: ['browse', 'hero-strip'] })
}
