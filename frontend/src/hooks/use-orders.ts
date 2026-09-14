import { useQuery } from '@tanstack/react-query'
import { getOrderById, getOrdersByCustomer } from '@/lib/api'
import { queryKeys } from '@/lib/queryClient'

export function useOrdersByCustomer(customerId: string) {
  return useQuery({
    queryKey: queryKeys.orders(customerId),
    queryFn: () => getOrdersByCustomer(customerId),
  })
}

export function useOrder(id: string) {
  return useQuery({
    queryKey: queryKeys.order(id),
    queryFn: () => getOrderById(id),
    enabled: !!id,
  })
}
