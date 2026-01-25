import { Routes } from '@angular/router';

export const CATALOGS_ROUTES: Routes = [
  {
    path: 'categories',
    loadChildren: () => import('./categories/categories.routes').then(m => m.CATEGORIES_ROUTES),
  },
  // {
  //   path: 'medicines',
  //   loadChildren: () => import('./medicines/medicines.routes').then(m => m.MEDICINES_ROUTES),
  // },
  // ... các routes khác
];