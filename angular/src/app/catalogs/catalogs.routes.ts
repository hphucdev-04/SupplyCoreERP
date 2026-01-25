import { Routes } from '@angular/router';

export const CATALOGS_ROUTES: Routes = [
  {
    path: 'categories',
    loadComponent: () => import('./categories/categories.component').then(m => m.CategoriesComponent),
  },
  // {
  //   path: 'medicines',
  //   loadComponent: () => import('./medicines/medicines.component').then(m => m.MedicinesComponent),
  // },
  // ... Thêm route catalog/...
];