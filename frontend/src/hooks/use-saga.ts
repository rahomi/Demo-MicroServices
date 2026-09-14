import { useQuery } from '@tanstack/react-query'
import { getSagaById } from '@/lib/api'
import { queryKeys } from '@/lib/queryClient'

const TERMINAL_STATES = ['Completed', 'Failed']

export function useSagaState(sagaId: string | null) {
  return useQuery({
    queryKey: sagaId ? queryKeys.saga(sagaId) : ['saga', 'none'],
    queryFn: () => getSagaById(sagaId!),
    enabled: !!sagaId,
    refetchInterval: (query) => {
      const state = query.state.data?.currentState
      if (state && TERMINAL_STATES.includes(state)) return false
      return 2000
    },
  })
}
