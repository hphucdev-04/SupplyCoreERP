import { Routes } from '@angular/router';

export const PARTNERS_ROUTES: Routes = [
    {
        path: 'customers',
        loadComponent: () => import('./customers/customers.component').then(m => m.CustomersComponent),
    },
    {
        path: 'suppliers',
        loadComponent: () => import('./suppliers/suppliers.component').then(m => m.SuppliersComponent),
    }
]