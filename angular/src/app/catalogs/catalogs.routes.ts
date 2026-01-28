import { Routes } from '@angular/router';

export const CATALOGS_ROUTES: Routes = [
  {
    path: 'categories',
    loadComponent: () => import('./categories/categories.component').then(m => m.CategoriesComponent),
  },
  {
    path: 'units',
    loadComponent: () => import('./units/units.component').then(m => m.UnitsComponent),
  },
  {
    path: 'dosageforms',
    loadComponent: () => import('./dosageforms/dosageforms.component').then(m => m.DosageformsComponent),
  },
  // ... Thêm route catalog/...
];