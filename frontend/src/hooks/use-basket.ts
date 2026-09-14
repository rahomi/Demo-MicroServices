import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import {
  getBasket,
  addBasketItem,
  removeBasketItem,
  checkout,
} from '@/lib/api'
import { queryKeys } from '@/lib/queryClient'
import type { AddBasketItemRequest } from '@/lib/types'


export function useBasket(customerId: string) {
  return useQuery({
    queryKey: queryKeys.basket(customerId),
    queryFn: () => getBasket(customerId),
  })
}

export function useAddBasketItem(customerId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (req: AddBasketItemRequest) => addBasketItem(customerId, req),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.basket(customerId) })
      toast.success('Added to basket')
    },
  })
}

export function useRemoveBasketItem(customerId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (productId: string) => removeBasketItem(customerId, productId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.basket(customerId) })
      toast.success('Item removed')
    },
  })
}

export function useCheckout(customerId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => checkout(customerId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.orders(customerId) })

      qc.invalidateQueries({ queryKey: queryKeys.basket(customerId) })
    },
    onError: (error: Error) => {
      // 502 — saga failed, basket restored by compensation
      qc.invalidateQueries({ queryKey: queryKeys.basket(customerId) })
      toast.error('Checkout saga failed — basket restored', {
        description: error.message,
      })
    },
  })
}
