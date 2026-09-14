import type {
  Product,
  CreateProductRequest,
  UpdateProductRequest,
  Basket,
  AddBasketItemRequest,
  Order,
  SubmitOrderRequest,
  CheckoutResponse,
  SagaState,
  Customer,
  NotificationEvent,
} from './types'

/** Fetch wrapper that throws on non-2xx with the response body as the error message. */
async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  })

  if (!res.ok) {
    let detail = ''
    try {
      const body = await res.json()
      detail = body.detail ?? body.title ?? body.message ?? JSON.stringify(body)
    } catch {
      detail = res.statusText
    }
    throw new Error(detail || `Request failed with status ${res.status}`)
  }

  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

// ==================== Products ====================

export function getProducts(): Promise<Product[]> {
  return request<Product[]>('/api/products')
}

export function getProductById(id: string): Promise<Product> {
  return request<Product>(`/api/products/${id}`)
}

export function createProduct(req: CreateProductRequest): Promise<Product> {
  return request<Product>('/api/products', {
    method: 'POST',
    body: JSON.stringify(req),
  })
}

export function updateProduct(id: string, req: UpdateProductRequest): Promise<Product> {
  return request<Product>(`/api/products/${id}`, {
    method: 'PUT',
    body: JSON.stringify(req),
  })
}

export function deleteProduct(id: string): Promise<void> {
  return request<void>(`/api/products/${id}`, { method: 'DELETE' })
}

// ==================== Basket ====================

export function getBasket(customerId: string): Promise<Basket> {
  return request<Basket>(`/api/baskets/${customerId}`)
}

export function addBasketItem(customerId: string, req: AddBasketItemRequest): Promise<Basket> {
  return request<Basket>(`/api/baskets/${customerId}/items`, {
    method: 'POST',
    body: JSON.stringify(req),
  })
}

export function removeBasketItem(customerId: string, productId: string): Promise<Basket> {
  return request<Basket>(`/api/baskets/${customerId}/items/${productId}`, {
    method: 'DELETE',
  })
}

export function checkout(customerId: string): Promise<CheckoutResponse> {
  return request<CheckoutResponse>(`/api/baskets/${customerId}/checkout`, {
    method: 'POST',
  })
}

// ==================== Orders ====================

export function getOrderById(id: string): Promise<Order> {
  return request<Order>(`/api/orders/${id}`)
}

export function getOrdersByCustomer(customerId: string): Promise<Order[]> {
  return request<Order[]>(`/api/orders?customerId=${customerId}`)
}

export function submitOrder(req: SubmitOrderRequest): Promise<Order> {
  return request<Order>('/api/orders', {
    method: 'POST',
    body: JSON.stringify(req),
  })
}

// ==================== Saga ====================

export function getSagaById(id: string): Promise<SagaState> {
  return request<SagaState>(`/api/sagas/${id}`)
}

// ==================== Identity ====================

export function getCustomer(): Promise<Customer> {
  return request<Customer>('/api/identity/customer')
}

// ==================== Notifications ====================

export function getNotifications(): Promise<NotificationEvent[]> {
  return request<NotificationEvent[]>('/api/notifications')
}
