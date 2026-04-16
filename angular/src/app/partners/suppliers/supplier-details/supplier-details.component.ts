import { Component } from '@angular/core';
import { Gender } from 'src/app/proxy/enums/partner/gender.enum';
import { SupplierService } from 'src/app/proxy/suppliers';
import { SupplierDetailDto } from 'src/app/proxy/suppliers/dtos';
import { SharedModule } from 'src/app/shared/shared.module';

@Component({
  selector: 'app-supplier-details',
  imports: [SharedModule],
  templateUrl: './supplier-details.component.html',
  styleUrl: './supplier-details.component.scss'
})
export class SupplierDetailsComponent {
  isVisible = false;
  supplier: SupplierDetailDto;

  Gender = Gender;
  constructor(
    private supplierService: SupplierService
  ) { }

  open(id: string) {
    this.supplierService.get(id).subscribe(res => {
      this.supplier = res;
      this.isVisible = true;
    });
  }

  close() {
    this.isVisible = false;
    this.supplier = null;
  }
  getEnumName(enumObj: any, value: number): string {
    return enumObj[value];
  }
}
