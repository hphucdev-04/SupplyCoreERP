import { Routes } from '@angular/router';

export const PROCUREMENT_ROUTES: Routes = [
  {
    path: 'purchase-orders',
    loadComponent: () =>
      import('./purchase-orders/purchase-orders.component').then(m => m.PurchaseOrdersComponent),
  },
  {
    path: 'purchase-orders/details/:id',
    loadComponent: () =>
      import('./purchase-orders/purchaseorder-details/purchase-order-details.component').then(
        m => m.PurchaseOrderDetailsComponent,
      ),
  },
  {
    path: 'purchase-requisitions',
    loadComponent: () =>
      import('../procurement/purchase-requisitions/purchase-requisition.component').then(
        m => m.PurchaseRequisitionComponent,
      ),
  },
  {
    path: 'purchase-requisitions/details/:id',
    loadComponent: () =>
      import('../procurement/purchase-requisitions/purchaserequisition-details/purchase-requisition-details.component').then(
        m => m.PurchaseRequisitionDetailsComponent,
      ),
  },
  {
    path: 'purchase-returns',
    loadComponent: () =>
      import('../procurement/purchase-returns/purchase-returns.component').then(
        m => m.PurchaseReturnsComponent,
      ),
  },
  {
    path: 'purchase-returns/details/:id',
    loadComponent: () =>
      import('../procurement/purchase-returns/purchase-return-details/purchase-return-details.component').then(
        m => m.PurchaseReturnDetailsComponent,
      ),
  },
  {
    path: 'purchase-return-requests',
    loadComponent: () =>
      import('./purchase-return-requests/purchase-return-requests.component').then(
        m => m.PurchaseReturnRequestsComponent,
      ),
  },
  {
    path: 'purchase-return-requests/details/:id',
    loadComponent: () =>
      import('./purchase-return-requests/details/purchase-return-request-details.component').then(
        m => m.PurchaseReturnRequestDetailsComponent,
      ),
  },
];
