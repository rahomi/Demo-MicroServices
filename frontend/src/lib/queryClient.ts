import { QueryClient, QueryCache, MutationCache } from '@tanstack/react-query'
import { toast } from 'sonner'

export const queryKeys = {
  products: ['products'] as const,
  product: (id: string) => ['products', id] as const,
  basket: (customerId: string) => ['basket', customerId] as const,
  orders: (customerId: string) => ['orders', customerId] as const,
  order: (id: string) => ['orders', id] as const,
  saga: (id: string) => ['saga', id] as const,
  notifications: ['notifications'] as const,
  customer: ['customer'] as const,
}

export const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      toast.error('Request failed', { description: error.message })
    },
  }),
  mutationCache: new MutationCache({
    onError: (error) => {
      toast.error('Operation failed', { description: error.message })
    },
  }),
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
})
