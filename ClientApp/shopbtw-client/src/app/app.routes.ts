import { Routes } from '@angular/router';
import { ProductsComponent } from './pages/products/products';
import { LoginComponent } from './pages/login/login';
import { CartComponent } from './pages/cart/cart';
import { OrdersComponent } from './pages/orders/orders';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  { path: '', component: ProductsComponent },
  { path: 'login', component: LoginComponent },
  { path: 'cart', component: CartComponent, canActivate: [authGuard] },
  { path: 'orders', component: OrdersComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '' }
];
