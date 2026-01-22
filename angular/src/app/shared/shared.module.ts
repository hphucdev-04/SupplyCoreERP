import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoreModule } from '@abp/ng.core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageModule } from '@abp/ng.components/page';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
 imports: [
    CommonModule,
    ThemeSharedModule,
    CoreModule,
    FormsModule,
    ReactiveFormsModule,
    PageModule,
    NgbModule,
    NgxDatatableModule 
  ],
  exports: [
    CommonModule,
    ThemeSharedModule,
    CoreModule, 
    FormsModule,
    ReactiveFormsModule,
    PageModule,
    NgbModule,
    NgxDatatableModule 
  ]
})
export class SharedModule { }