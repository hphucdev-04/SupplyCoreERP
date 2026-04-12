import { Routes } from '@angular/router';

export const ORDER_ROUTES: Routes = [
    {
        path: 'saleorders',
        loadComponent: () => import('./saleorders/saleorders.component').then(m => m.SalesOrdersComponent)
    },
    {
        path: 'purchaseorders',
        loadComponent: () => import('./purchaseorders/purchaseorders.component').then(m => m.PurchaseOrdersComponent)
    }
];