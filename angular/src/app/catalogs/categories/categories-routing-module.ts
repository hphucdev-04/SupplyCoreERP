import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Categories } from './categories';

const routes: Routes = [
  { 
    path: '', 
    component: Categories
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CategoriesRoutingModule { }
