import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useOrdersByCustomer } from '@/hooks/use-orders'
import { CUSTOMER_ID } from '@/lib/constants'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { ChevronDown, ChevronUp, ClipboardList, RefreshCw } from 'lucide-react'
import type { Order } from '@/lib/types'

function statusVariant(status: string) {
  switch (status.toLowerCase()) {
    case 'submitted':
      return 'bg-blue-600 text-white'
    case 'cancelled':
      return 'bg-red-600 text-white'
    case 'completed':
      return 'bg-green-600 text-white'
    default:
      return ''
  }
}

export function OrdersPage() {
  const { data: orders, isLoading, isError, refetch } = useOrdersByCustomer(CUSTOMER_ID)
  const [expanded, setExpanded] = useState<Set<string>>(new Set())

  const toggle = (id: string) =>
    setExpanded((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })

  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <Card key={i} className="h-24 animate-pulse bg-muted" />
        ))}
      </div>
    )
  }

  if (isError) {
    return (
      <div className="flex flex-col items-center gap-4 py-20">
        <p className="text-muted-foreground">Failed to load orders.</p>
        <Button onClick={() => refetch()} variant="outline">
          <RefreshCw className="mr-2 h-4 w-4" /> Retry
        </Button>
      </div>
    )
  }

  if (!orders || orders.length === 0) {
    return (
      <div className="flex flex-col items-center gap-4 py-20">
        <ClipboardList className="h-12 w-12 text-muted-foreground" />
        <p className="text-muted-foreground">No orders yet</p>
        <Button asChild>
          <Link to="/shop/products">Browse products</Link>
        </Button>
      </div>
    )
  }

  const sorted = [...orders].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  )

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">Order History</h1>
      <div className="space-y-3">
        {sorted.map((order: Order) => {
          const isOpen = expanded.has(order.id)
          return (
            <Card key={order.id}>
              <CardHeader>
                <button
                  onClick={() => toggle(order.id)}
                  className="flex w-full items-center justify-between text-left"
                >
                  <div className="space-y-1">
                    <CardTitle className="text-base font-mono">
                      #{order.id.slice(0, 8)}
                    </CardTitle>
                    <p className="text-sm text-muted-foreground">
                      {new Date(order.createdAt).toLocaleString()}
                    </p>
                  </div>
                  <div className="flex items-center gap-3">
                    <Badge className={statusVariant(order.status)}>{order.status}</Badge>
                    <span className="font-semibold">${order.total.toFixed(2)}</span>
                    {isOpen ? (
                      <ChevronUp className="h-4 w-4 text-muted-foreground" />
                    ) : (
                      <ChevronDown className="h-4 w-4 text-muted-foreground" />
                    )}
                  </div>
                </button>
              </CardHeader>
              {isOpen && (
                <CardContent className="space-y-2 border-t pt-4">
                  {order.items.map((item) => (
                    <div key={item.id} className="flex justify-between text-sm">
                      <span>
                        {item.productName} × {item.quantity}
                      </span>
                      <span>${(item.unitPrice * item.quantity).toFixed(2)}</span>
                    </div>
                  ))}
                  <div className="flex justify-between border-t pt-2 font-semibold">
                    <span>Total</span>
                    <span>${order.total.toFixed(2)}</span>
                  </div>
                </CardContent>
              )}
            </Card>
          )
        })}
      </div>
    </div>
  )
}
