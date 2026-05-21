import { Routes } from '@angular/router';

export const ORDER_ROUTES: Routes = [
  {
    path: 'saleorders',
    loadComponent: () =>
      import('./saleorders/saleorders.component').then(m => m.SalesOrdersComponent),
  },
  {
    path: 'saleorders/details/:id',
    loadComponent: () =>
      import('./saleorders/saleorder-details/saleorder-details.component').then(
        m => m.SaleOrderDetailsComponent,
      ),
  },
  {
    path: 'purchaseorders',
    loadComponent: () =>
      import('./purchaseorders/purchaseorders.component').then(m => m.PurchaseOrdersComponent),
  },
  {
    path: 'purchaseorders/details/:id',
    loadComponent: () =>
      import('./purchaseorders/purchaseorder-details/purchaseorder-details.component').then(
        m => m.PurchaseOrderDetailsComponent,
      ),
  },
  {
    path: 'purchaserequisitions',
    loadComponent: () =>
      import('./purchase-requisitions/purchase-requisition.component').then(
        m => m.PurchaseRequisitionComponent,
      ),
  },
  {
    path: 'purchaserequisitions/details/:id',
    loadComponent: () =>
      import('./purchase-requisitions/purchaserequisition-details/purchase-requisition-details.component').then(
        m => m.PurchaseRequisitionDetailsComponent,
      ),
  },
];
