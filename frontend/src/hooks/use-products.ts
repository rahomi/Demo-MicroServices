import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import {
  getProducts,
  createProduct,
  updateProduct,
  deleteProduct,
} from '@/lib/api'
import { queryKeys } from '@/lib/queryClient'
import type { CreateProductRequest, UpdateProductRequest } from '@/lib/types'

export function useProducts() {
  return useQuery({
    queryKey: queryKeys.products,
    queryFn: getProducts,
  })
}

export function useCreateProduct() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (req: CreateProductRequest) => createProduct(req),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.products })
      toast.success('Product created')
    },
  })
}

export function useUpdateProduct() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req: UpdateProductRequest }) =>
      updateProduct(id, req),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.products })
      toast.success('Product updated')
    },
  })
}

export function useDeleteProduct() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => deleteProduct(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.products })
      toast.success('Product deleted')
    },
  })
}
