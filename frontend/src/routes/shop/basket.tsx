import { useNavigate } from 'react-router-dom'
import { useBasket, useRemoveBasketItem, useCheckout } from '@/hooks/use-basket'
import { CUSTOMER_ID } from '@/lib/constants'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Trash2, ShoppingCart, RefreshCw, ArrowRight } from 'lucide-react'
import type { CheckoutResponse } from '@/lib/types'

export function BasketPage() {
  const navigate = useNavigate()
  const { data: basket, isLoading, isError, refetch } = useBasket(CUSTOMER_ID)
  const removeItem = useRemoveBasketItem(CUSTOMER_ID)
  const checkoutMut = useCheckout(CUSTOMER_ID)

  const items = basket?.items ?? []
  const total = items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0)

  const handleCheckout = () => {
    checkoutMut.mutate(undefined, {
      onSuccess: (data: CheckoutResponse) => {
        navigate('/shop/checkout', {
          state: { sagaId: data.sagaId, state: data.state, order: data.order, message: data.message },
        })
      },
    })
  }

  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <Card key={i} className="h-20 animate-pulse bg-muted" />
        ))}
      </div>
    )
  }

  if (isError) {
    return (
      <div className="flex flex-col items-center gap-4 py-20">
        <p className="text-muted-foreground">Failed to load basket.</p>
        <Button onClick={() => refetch()} variant="outline">
          <RefreshCw className="mr-2 h-4 w-4" /> Retry
        </Button>
      </div>
    )
  }

  if (items.length === 0) {
    return (
      <div className="flex flex-col items-center gap-4 py-20">
        <ShoppingCart className="h-12 w-12 text-muted-foreground" />
        <p className="text-muted-foreground">Your basket is empty</p>
        <Button onClick={() => navigate('/shop/products')}>Browse products</Button>
      </div>
    )
  }

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">Shopping Basket</h1>
      <div className="space-y-3">
        {items.map((item) => (
          <Card key={item.id}>
            <CardContent className="flex items-center justify-between py-4">
              <div className="space-y-1">
                <p className="font-medium">{item.productName}</p>
                <p className="text-sm text-muted-foreground">
                  ${item.unitPrice.toFixed(2)} × {item.quantity}
                </p>
              </div>
              <div className="flex items-center gap-4">
                <p className="font-semibold">${(item.unitPrice * item.quantity).toFixed(2)}</p>
                <Button
                  size="icon"
                  variant="ghost"
                  onClick={() => removeItem.mutate(item.productId)}
                  disabled={removeItem.isPending}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Card className="mt-6">
        <CardHeader>
          <CardTitle className="flex items-center justify-between text-lg">
            <span>Total</span>
            <span>${total.toFixed(2)}</span>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <Button
            onClick={handleCheckout}
            disabled={checkoutMut.isPending}
            className="w-full"
            size="lg"
          >
            {checkoutMut.isPending ? 'Processing...' : 'Checkout'}
            {!checkoutMut.isPending && <ArrowRight className="ml-2 h-4 w-4" />}
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}
