import { useQuery } from '@tanstack/react-query'
import { getNotifications } from '@/lib/api'
import { queryKeys } from '@/lib/queryClient'
import type { NotificationEvent } from '@/lib/types'

export function useNotifications() {
  return useQuery({
    queryKey: queryKeys.notifications,
    queryFn: getNotifications,
    refetchInterval: 3000,
    select: (data: NotificationEvent[]): NotificationEvent[] => {
      // Deduplicate by eventType + receivedAt
      const seen = new Set<string>()
      const deduped = data.filter((e) => {
        const key = `${e.eventType}-${e.receivedAt}`
        if (seen.has(key)) return false
        seen.add(key)
        return true
      })
      // Cap to latest 50
      return deduped.slice(0, 50)
    },
  })
}
