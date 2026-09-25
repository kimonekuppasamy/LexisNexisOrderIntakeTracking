import { inject } from '@angular/core';
import { CanActivateFn, Router, Routes } from '@angular/router';
import { ProductsComponent } from './components/products/products';
import { OrdersComponent } from './components/orders/orders';
import { LoginComponent } from './components/login/login';
import { CartComponent } from './components/cart/cart';
import { CustomerService } from './core/services/customer';

const loggedInGuard: CanActivateFn = () =>
  inject(CustomerService).getCurrentCustomer() != null || inject(Router).createUrlTree(['/login']);

const adminGuard: CanActivateFn = () =>
  inject(CustomerService).getCurrentCustomer()?.isAdmin === true || inject(Router).createUrlTree(['/login']);

export const routes: Routes = [
  {
    path: 'products',
    component: ProductsComponent
  },
   {
    path: 'orders',
    component: OrdersComponent,
    canActivate: [loggedInGuard]
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'cart',
    component:CartComponent
  },
  {
    path: 'orders/admin',
    component: OrdersComponent,
    canActivate: [adminGuard]
  },
  {
    path: '',
    redirectTo: 'products',
    pathMatch: 'full'
  }
];
