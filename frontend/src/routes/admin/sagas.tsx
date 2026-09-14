import { useState } from 'react'
import { useSagaState } from '@/hooks/use-saga'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Info, Search } from 'lucide-react'
import type { BasketItemSnapshot } from '@/lib/types'

const STATES = ['Started', 'BasketReserved', 'OrderCreated', 'Completed', 'Compensating', 'Failed']

function stateColor(state: string): string {
  switch (state) {
    case 'Started':
      return 'bg-blue-600 text-white'
    case 'BasketReserved':
      return 'bg-cyan-600 text-white'
    case 'OrderCreated':
      return 'bg-amber-600 text-white'
    case 'Completed':
      return 'bg-green-600 text-white'
    case 'Compensating':
      return 'bg-orange-600 text-white'
    case 'Failed':
      return 'bg-red-600 text-white'
    default:
      return ''
  }
}

export function SagasPage() {
  const [inputId, setInputId] = useState('')
  const [sagaId, setSagaId] = useState<string | null>(null)
  const { data: saga, isLoading, isError } = useSagaState(sagaId)

  const handleLoad = () => {
    const trimmed = inputId.trim()
    if (trimmed) setSagaId(trimmed)
  }

  const parseSnapshot = (json: string): BasketItemSnapshot[] => {
    try {
      return JSON.parse(json) as BasketItemSnapshot[]
    } catch {
      return []
    }
  }

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">Saga State Inspector</h1>

      {/* Input */}
      <Card className="mb-6">
        <CardContent className="flex items-center gap-3 py-4">
          <Input
            placeholder="Enter saga ID..."
            value={inputId}
            onChange={(e) => setInputId(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleLoad()}
            className="flex-1"
          />
          <Button onClick={handleLoad}>
            <Search className="mr-2 h-4 w-4" /> Load
          </Button>
        </CardContent>
      </Card>

      {/* Failure instructions */}
      <Card className="mb-6 border-amber-600/50">
        <CardContent className="flex gap-3 py-4">
          <Info className="h-5 w-5 shrink-0 text-amber-500" />
          <div className="text-sm text-muted-foreground">
            <p className="font-medium text-foreground">To test saga compensation:</p>
            <ol className="mt-1 list-decimal space-y-1 pl-4">
              <li>Add items to basket</li>
              <li>Stop the orders container: <code className="rounded bg-muted px-1">docker compose stop orders</code></li>
              <li>Checkout from the basket page</li>
              <li>Return here and enter the saga ID to see the Failed state with compensation details</li>
            </ol>
          </div>
        </CardContent>
      </Card>

      {/* Saga state */}
      {sagaId && isLoading && (
        <Card className="animate-pulse bg-muted">
          <CardContent className="h-40" />
        </Card>
      )}

      {sagaId && isError && (
        <Card>
          <CardContent className="py-8 text-center text-muted-foreground">
            Saga not found. Check the ID and try again.
          </CardContent>
        </Card>
      )}

      {saga && (
        <div className="space-y-6">
          {/* State machine */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">State Machine</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap items-center gap-2">
                {STATES.map((s, i) => (
                  <div key={s} className="flex items-center gap-2">
                    <div
                      className={`rounded-md border px-3 py-1.5 text-sm font-medium ${
                        saga.currentState === s
                          ? stateColor(s) + ' border-transparent'
                          : 'border-border text-muted-foreground'
                      }`}
                    >
                      {s}
                    </div>
                    {i < STATES.length - 1 && (
                      <span className="text-muted-foreground">→</span>
                    )}
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Details */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Saga ID</span>
                <span className="font-mono">{saga.id}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Customer ID</span>
                <span className="font-mono">{saga.customerId}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Current State</span>
                <Badge className={stateColor(saga.currentState)}>{saga.currentState}</Badge>
              </div>
              {saga.orderId && (
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Order ID</span>
                  <span className="font-mono">{saga.orderId}</span>
                </div>
              )}
              {saga.errorMessage && (
                <div className="rounded-md border border-destructive/50 bg-destructive/10 p-3 text-sm">
                  <span className="font-medium text-destructive">Error: </span>
                  {saga.errorMessage}
                </div>
              )}
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Created</span>
                <span>{new Date(saga.createdAt).toLocaleString()}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Updated</span>
                <span>{new Date(saga.updatedAt).toLocaleString()}</span>
              </div>
            </CardContent>
          </Card>

          {/* Basket snapshot */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Basket Snapshot</CardTitle>
            </CardHeader>
            <CardContent>
              {parseSnapshot(saga.basketSnapshotJson).length === 0 ? (
                <p className="text-sm text-muted-foreground">No items in snapshot.</p>
              ) : (
                <div className="space-y-2">
                  {parseSnapshot(saga.basketSnapshotJson).map((item, i) => (
                    <div
                      key={i}
                      className="flex justify-between border-b pb-2 text-sm last:border-0"
                    >
                      <span>
                        {item.productName} × {item.quantity}
                      </span>
                      <span>${(item.unitPrice * item.quantity).toFixed(2)}</span>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      )}

      {!sagaId && (
        <Card>
          <CardContent className="py-12 text-center text-muted-foreground">
            Enter a saga ID to inspect
          </CardContent>
        </Card>
      )}
    </div>
  )
}
