import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationPipe } from '@abp/ng.core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@NgModule({
  imports: [
    CommonModule,
    LocalizationPipe,
    FormsModule,
    ReactiveFormsModule,
  ],
  exports: [
    CommonModule,
    LocalizationPipe,
    FormsModule,
    ReactiveFormsModule,
  ]
})
export class SharedModule { }