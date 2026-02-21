import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerService } from 'src/app/proxy/customers';
import { CustomerDetailDto } from 'src/app/proxy/customers/dtos';
import { Gender, CustomerType } from 'src/app/proxy/enums/partner';
import { SharedModule } from 'src/app/shared/shared.module';

@Component({
  selector: 'app-customer-detail',
  standalone: true,
  imports: [SharedModule, CommonModule],
  templateUrl: './customer-details.component.html',
  styleUrl: './customer-details.component.scss'
})
export class CustomerDetailsComponent {
  
  isVisible = false;
  customer: CustomerDetailDto;

  Gender = Gender;
  CustomerType = CustomerType;

  constructor(private customerService: CustomerService) {}

  open(id: string) {
    this.customerService.get(id).subscribe(res => {
      this.customer = res;
      this.isVisible = true; 
    });
  }

  close() {
    this.isVisible = false;
    this.customer = null;
  }

  getEnumName(enumObj: any, value: number): string {
    return enumObj[value];
  }
}