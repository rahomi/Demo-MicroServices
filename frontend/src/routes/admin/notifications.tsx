import { useState } from 'react'
import { useNotifications } from '@/hooks/use-notifications'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Bell, RefreshCw, ChevronDown, ChevronUp } from 'lucide-react'
import type { NotificationEvent } from '@/lib/types'

function eventColor(eventType: string): string {
  switch (eventType) {
    case 'BasketCheckedOut':
      return 'bg-cyan-600 text-white'
    case 'OrderSubmitted':
      return 'bg-green-600 text-white'
    case 'ProductChanged':
      return 'bg-amber-600 text-white'
    default:
      return 'bg-secondary text-secondary-foreground'
  }
}

function relativeTime(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime()
  const seconds = Math.floor(diff / 1000)
  if (seconds < 60) return `${seconds}s ago`
  const minutes = Math.floor(seconds / 60)
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  return new Date(iso).toLocaleString()
}

function EventCard({ event }: { event: NotificationEvent }) {
  const [expanded, setExpanded] = useState(false)

  let prettyPayload = event.payload
  try {
    prettyPayload = JSON.stringify(JSON.parse(event.payload), null, 2)
  } catch {
    // keep raw
  }

  return (
    <Card>
      <CardContent className="py-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Badge className={eventColor(event.eventType)}>{event.eventType}</Badge>
            <span className="text-sm text-muted-foreground">{relativeTime(event.receivedAt)}</span>
          </div>
          <Button size="icon" variant="ghost" onClick={() => setExpanded((e) => !e)}>
            {expanded ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
          </Button>
        </div>
        {expanded && (
          <pre className="mt-3 overflow-x-auto rounded-md bg-muted p-3 text-xs">
            {prettyPayload}
          </pre>
        )}
      </CardContent>
    </Card>
  )
}

export function NotificationsPage() {
  const { data: events, isLoading, isError, refetch } = useNotifications()

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">Live Notifications Feed</h1>
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <span className="relative flex h-2.5 w-2.5">
            <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-green-400 opacity-75" />
            <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-green-500" />
          </span>
          Live
        </div>
      </div>

      {isLoading && (
        <div className="space-y-3">
          {Array.from({ length: 4 }).map((_, i) => (
            <Card key={i} className="h-16 animate-pulse bg-muted" />
          ))}
        </div>
      )}

      {isError && (
        <div className="flex flex-col items-center gap-4 py-20">
          <p className="text-muted-foreground">Failed to load notifications.</p>
          <Button onClick={() => refetch()} variant="outline">
            <RefreshCw className="mr-2 h-4 w-4" /> Retry
          </Button>
        </div>
      )}

      {!isLoading && !isError && (!events || events.length === 0) && (
        <div className="flex flex-col items-center gap-4 py-20">
          <Bell className="h-12 w-12 text-muted-foreground" />
          <p className="text-muted-foreground">
            No events yet. Try adding items to basket and checking out, or updating a product.
          </p>
        </div>
      )}

      {events && events.length > 0 && (
        <div className="space-y-3">
          {events.map((event, i) => (
            <EventCard key={`${event.eventType}-${event.receivedAt}-${i}`} event={event} />
          ))}
        </div>
      )}
    </div>
  )
}
