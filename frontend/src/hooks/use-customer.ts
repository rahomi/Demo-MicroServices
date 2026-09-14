import { useQuery } from '@tanstack/react-query'
import { getCustomer } from '@/lib/api'
import { queryKeys } from '@/lib/queryClient'

export function useCustomer() {
  return useQuery({
    queryKey: queryKeys.customer,
    queryFn: getCustomer,
  })
}
