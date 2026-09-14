// ==================== Products ====================

export interface Product {
  id: string
  name: string
  price: number
  category: string
}

export interface CreateProductRequest {
  name: string
  price: number
  category: string
}

export interface UpdateProductRequest {
  id: string
  name: string
  price: number
  category: string
}

// ==================== Basket ====================

export interface BasketItem {
  id: string
  basketId: string
  productId: string
  productName: string
  unitPrice: number
  quantity: number
}

export interface Basket {
  id: string
  customerId: string
  items: BasketItem[]
}

export interface AddBasketItemRequest {
  productId: string
  productName: string
  unitPrice: number
  quantity: number
}

// ==================== Orders ====================

export interface OrderItem {
  id: string
  orderId: string
  productId: string
  productName: string
  unitPrice: number
  quantity: number
}

export interface Order {
  id: string
  customerId: string
  items: OrderItem[]
  total: number
  status: string
  createdAt: string
}

export interface SubmitOrderItem {
  productId: string
  productName: string
  unitPrice: number
  quantity: number
}

export interface SubmitOrderRequest {
  customerId: string
  items: SubmitOrderItem[]
}

// ==================== Checkout / Saga ====================

export interface CheckoutResponse {
  sagaId: string
  state: string
  order: Order | null
  message: string
}

export interface BasketItemSnapshot {
  productId: string
  productName: string
  unitPrice: number
  quantity: number
}

export interface SagaState {
  id: string
  customerId: string
  currentState: string // Started | BasketReserved | OrderCreated | Completed | Compensating | Failed
  basketSnapshotJson: string
  orderId: string | null
  errorMessage: string | null
  createdAt: string
  updatedAt: string
}

// ==================== Identity ====================

export interface Customer {
  id: string
  name: string
  email: string
}

// ==================== Notifications ====================

export interface NotificationEvent {
  eventType: string
  routingKey: string
  payload: string
  receivedAt: string
}
