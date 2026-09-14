import { Link, NavLink } from 'react-router-dom'
import { ShoppingCart, Package, ClipboardList, Settings, Bell, Activity } from 'lucide-react'
import { useBasket } from '@/hooks/use-basket'
import { CUSTOMER_ID } from '@/lib/constants'
import { cn } from '@/lib/utils'

const shopLinks = [
  { to: '/shop/products', label: 'Products', icon: Package },
  { to: '/shop/basket', label: 'Basket', icon: ShoppingCart },
  { to: '/shop/orders', label: 'Orders', icon: ClipboardList },
]

const adminLinks = [
  { to: '/admin/products', label: 'Products', icon: Package },
  { to: '/admin/sagas', label: 'Sagas', icon: Activity },
  { to: '/admin/notifications', label: 'Notifications', icon: Bell },
]

export function Navbar() {
  const { data: basket } = useBasket(CUSTOMER_ID)
  const itemCount = basket?.items?.reduce((sum, i) => sum + i.quantity, 0) ?? 0

  return (
    <header className="sticky top-0 z-40 border-b bg-background/95 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-7xl items-center gap-6 px-4">
        {/* Logo */}
        <Link to="/shop/products" className="flex items-center gap-2 font-bold text-lg">
          <ShoppingCart className="h-5 w-5" />
          <span>EcommerceDemo</span>
        </Link>

        {/* Shop section */}
        <nav className="flex items-center gap-1">
          {shopLinks.map((link) => (
            <NavLink
              key={link.to}
              to={link.to}
              className={({ isActive }) =>
                cn(
                  'relative flex items-center gap-1.5 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-secondary text-secondary-foreground'
                    : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
                )
              }
            >
              <link.icon className="h-4 w-4" />
              {link.label}
              {/* Basket item-count badge on the Basket link itself */}
              {link.to === '/shop/basket' && itemCount > 0 && (
                <span className="absolute -right-1 -top-1 flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1 text-xs font-bold text-primary-foreground">
                  {itemCount}
                </span>
              )}
            </NavLink>
          ))}
        </nav>

        <div className="ml-auto" />

        {/* Admin section — visually separated with a divider and section label */}
        <div className="flex items-center gap-1">
          <span className="flex items-center gap-1.5 rounded-md bg-muted px-2.5 py-1.5 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
            <Settings className="h-3.5 w-3.5" />
            Admin
          </span>
          <div className="mx-1 h-6 w-px bg-border" />
          <nav className="flex items-center gap-1">
            {adminLinks.map((link) => (
              <NavLink
                key={link.to}
                to={link.to}
                className={({ isActive }) =>
                  cn(
                    'flex items-center gap-1.5 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                    isActive
                      ? 'bg-secondary text-secondary-foreground'
                      : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
                  )
                }
              >
                <link.icon className="h-4 w-4" />
                {link.label}
              </NavLink>
            ))}
          </nav>
        </div>
      </div>
    </header>
  )
}
