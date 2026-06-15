import { useCallback, useEffect, useMemo, useRef } from 'react'

/**
 * Tracks object URLs created from files and revokes them all on unmount.
 * Use `create` to make a tracked preview URL, and `revoke` to release one early.
 * The returned object is stable across renders, so it is safe in dependency arrays.
 */
export function useObjectUrls() {
  const urls = useRef<string[]>([])

  useEffect(
    () => () => {
      urls.current.forEach(url => URL.revokeObjectURL(url))
      urls.current = []
    },
    []
  )

  const create = useCallback((file: File): string => {
    const url = URL.createObjectURL(file)
    urls.current.push(url)
    return url
  }, [])

  const revoke = useCallback((url: string) => {
    URL.revokeObjectURL(url)
    urls.current = urls.current.filter(u => u !== url)
  }, [])

  return useMemo(() => ({ create, revoke }), [create, revoke])
}
