import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '', 
    children: [
      {
        path: 'category', 
        loadChildren: () => import('./categories/categories-module').then(m => m.CategoriesModule),
      },
      // {
      //   path: 'category', 
      //   loadChildren: () => import('./categories/medicines-module').then(m => m.MedicinesModule),
      // },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CatalogsRoutingModule { }
