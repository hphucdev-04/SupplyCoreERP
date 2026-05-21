import { Routes } from '@angular/router';

export const PARTNERS_ROUTES: Routes = [
  {
    path: 'customers',
    loadComponent: () => import('./customers/customers.component').then(m => m.CustomersComponent),
  },
  {
    path: 'customers/details/:id',
    loadComponent: () =>
      import('./customers/customer-details/customer-details.component').then(
        m => m.CustomerDetailsComponent,
      ),
  },
  {
    path: 'suppliers',
    loadComponent: () => import('./suppliers/suppliers.component').then(m => m.SuppliersComponent),
  },
  {
    path: 'suppliers/details/:id',
    loadComponent: () =>
      import('./suppliers/supplier-details/supplier-details.component').then(
        m => m.SupplierDetailsComponent,
      ),
  },
];
