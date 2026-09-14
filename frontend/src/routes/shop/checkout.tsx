import { useLocation, Link } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { CheckCircle2, ShoppingBag, ArrowLeft } from 'lucide-react'
import type { Order } from '@/lib/types'


interface CheckoutLocationState {
  sagaId?: string
  state?: string
  order?: Order | null
  message?: string
}

export function CheckoutPage() {
  const location = useLocation()
  const data = (location.state ?? null) as CheckoutLocationState | null

  if (!data) {
    return (
      <div className="flex flex-col items-center gap-4 py-20">
        <ShoppingBag className="h-12 w-12 text-muted-foreground" />
        <p className="text-muted-foreground">No recent checkout</p>
        <Button asChild>
          <Link to="/shop/basket">Go to basket</Link>
        </Button>
      </div>
    )
  }

  const order = data.order

  if (!order) {
    return (
      <div className="flex flex-col items-center gap-4 py-20">
        <p className="text-lg font-semibold text-destructive">Checkout failed</p>
        <p className="text-muted-foreground">{data.message ?? 'Saga did not complete.'}</p>
        <Button asChild variant="outline">
          <Link to="/shop/basket">Back to basket</Link>
        </Button>
      </div>
    )
  }

  const total = order.items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0)

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center gap-3">
        <CheckCircle2 className="h-8 w-8 text-green-500" />
        <h1 className="text-2xl font-bold">Checkout Complete</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between text-lg">
            <span>Saga State</span>
            <Badge className="bg-green-600 text-white">{data.state ?? 'Completed'}</Badge>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Saga ID</p>
          <p className="mb-4 font-mono text-sm">{data.sagaId}</p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Order Summary</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Order ID</span>
            <span className="font-mono">{order.id.slice(0, 8)}</span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Status</span>
            <Badge variant="secondary">{order.status}</Badge>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Created</span>
            <span>{new Date(order.createdAt).toLocaleString()}</span>
          </div>
          <div className="border-t pt-3">
            {order.items.map((item) => (
              <div key={item.id} className="flex justify-between py-1 text-sm">
                <span>
                  {item.productName} × {item.quantity}
                </span>
                <span>${(item.unitPrice * item.quantity).toFixed(2)}</span>
              </div>
            ))}
          </div>
          <div className="flex justify-between border-t pt-3 font-semibold">
            <span>Total</span>
            <span>${total.toFixed(2)}</span>
          </div>
        </CardContent>
      </Card>

      <div className="flex gap-3">
        <Button asChild variant="outline">
          <Link to="/shop/orders">View order in history</Link>
        </Button>
        <Button asChild>
          <Link to="/shop/products">
            <ArrowLeft className="mr-2 h-4 w-4" /> Back to products
          </Link>
        </Button>
      </div>
    </div>
  )
}
