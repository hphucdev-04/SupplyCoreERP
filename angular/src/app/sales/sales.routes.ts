import { Routes } from '@angular/router';

export const SALES_ROUTES: Routes = [
  {
    path: 'sales-orders',
    loadComponent: () =>
      import('./sales-orders/sales-orders.component').then(m => m.SalesOrdersComponent),
  },
  {
    path: 'sales-orders/details/:id',
    loadComponent: () =>
      import('./sales-orders/sales-order-details/sales-order-details.component').then(
        m => m.SalesOrderDetailsComponent,
      ),
  },
  {
    path: 'sales-recalls',
    loadComponent: () =>
      import('./sales-recalls/sales-recalls.component').then(m => m.SalesRecallsComponent),
  },
  {
    path: 'sales-recalls/details/:id',
    loadComponent: () =>
      import('./sales-recalls/sales-recall-details/sales-recall-details.component').then(
        m => m.SalesRecallDetailsComponent,
      ),
  },
];
