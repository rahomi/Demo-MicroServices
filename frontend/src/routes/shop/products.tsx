import { useState } from 'react'
import { useProducts } from '@/hooks/use-products'

import { useAddBasketItem } from '@/hooks/use-basket'
import { CUSTOMER_ID } from '@/lib/constants'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Badge } from '@/components/ui/badge'
import { Package, Plus, RefreshCw } from 'lucide-react'
import type { Product } from '@/lib/types'

export function ProductsPage() {
  const { data: products, isLoading, isError, refetch } = useProducts()
  const addBasketItem = useAddBasketItem(CUSTOMER_ID)
  const [quantities, setQuantities] = useState<Record<string, number>>({})

  const getQty = (id: string) => quantities[id] ?? 1
  const setQty = (id: string, qty: number) => setQuantities((p) => ({ ...p, [id]: qty }))

  const handleAdd = (product: Product) => {
    addBasketItem.mutate({
      productId: product.id,
      productName: product.name,
      unitPrice: product.price,
      quantity: getQty(product.id),
    })
  }

  if (isLoading) {
    return (
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 6 }).map((_, i) => (
          <Card key={i} className="h-48 animate-pulse bg-muted" />
        ))}
      </div>
    )
  }

  if (isError) {
    return (
      <div className="flex flex-col items-center gap-4 py-20">
        <p className="text-muted-foreground">Failed to load products.</p>
        <Button onClick={() => refetch()} variant="outline">
          <RefreshCw className="mr-2 h-4 w-4" /> Retry
        </Button>
      </div>
    )
  }

  if (!products || products.length === 0) {
    return (
      <div className="flex flex-col items-center gap-4 py-20">
        <Package className="h-12 w-12 text-muted-foreground" />
        <p className="text-muted-foreground">No products available</p>
      </div>
    )
  }

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">Product Catalog</h1>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {products.map((product) => (
          <Card key={product.id}>
            <CardHeader>
              <div className="flex items-start justify-between">
                <CardTitle className="text-lg">{product.name}</CardTitle>
                <Badge variant="secondary">{product.category}</Badge>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-2xl font-bold">${product.price.toFixed(2)}</p>
              <div className="flex items-center gap-2">
                <Select
                  value={String(getQty(product.id))}
                  onValueChange={(v) => setQty(product.id, Number(v))}
                >
                  <SelectTrigger className="w-20">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {Array.from({ length: 10 }, (_, i) => i + 1).map((n) => (
                      <SelectItem key={n} value={String(n)}>
                        {n}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <Button
                  onClick={() => handleAdd(product)}
                  disabled={addBasketItem.isPending}
                  className="flex-1"
                >
                  <Plus className="mr-1 h-4 w-4" /> Add to basket
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
