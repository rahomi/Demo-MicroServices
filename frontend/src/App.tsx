import { Routes, Route, Navigate } from 'react-router-dom'
import { Layout } from '@/components/shared/Layout'
import { ProductsPage } from '@/routes/shop/products'
import { BasketPage } from '@/routes/shop/basket'
import { CheckoutPage } from '@/routes/shop/checkout'
import { OrdersPage } from '@/routes/shop/orders'
import { AdminProductsPage } from '@/routes/admin/products'
import { SagasPage } from '@/routes/admin/sagas'
import { NotificationsPage } from '@/routes/admin/notifications'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Navigate to="/shop/products" replace />} />
        <Route path="/shop/products" element={<ProductsPage />} />
        <Route path="/shop/basket" element={<BasketPage />} />
        <Route path="/shop/checkout" element={<CheckoutPage />} />
        <Route path="/shop/orders" element={<OrdersPage />} />
        <Route path="/admin/products" element={<AdminProductsPage />} />
        <Route path="/admin/sagas" element={<SagasPage />} />
        <Route path="/admin/notifications" element={<NotificationsPage />} />
        <Route path="*" element={<Navigate to="/shop/products" replace />} />
      </Route>
    </Routes>
  )
}
