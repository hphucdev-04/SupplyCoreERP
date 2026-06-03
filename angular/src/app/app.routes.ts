import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
    canActivate: [authGuard],
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
  {
    path: 'catalog',
    loadChildren: () => import('./catalogs/catalogs.routes').then(m => m.CATALOGS_ROUTES),
  },
  {
    path: 'partner',
    loadChildren: () => import('./partners/partner.routes').then(m => m.PARTNERS_ROUTES),
  },
  {
    path: 'inventory',
    loadChildren: () => import('./inventories/inventories.routes').then(m => m.INVENTORIES_ROUTES),
  },
  {
    path: 'procurement',
    loadChildren: () => import('./procurement/procurement.routes').then(m => m.PROCUREMENT_ROUTES),
  },
  {
    path: 'sales',
    loadChildren: () => import('./sales/sales.routes').then(m => m.SALES_ROUTES),
  }
];
