import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { CustomerService } from 'src/app/proxy/customers';
import { CustomerDetailDto } from 'src/app/proxy/customers/dtos';
import { Gender, CustomerType } from 'src/app/proxy/enums/partner';
import { SharedModule } from 'src/app/shared/shared.module';
import { enumName } from 'src/app/shared/untils/enum.util';

@Component({
  selector: 'app-customer-detail',
  standalone: true,
  imports: [SharedModule, CommonModule],
  templateUrl: './customer-details.component.html',
  styleUrl: './customer-details.component.scss',
})
export class CustomerDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private readonly ROUTE_NAME = '::Menu:CustomerDetails';

  loading = true;
  customer: CustomerDetailDto;

  Gender = Gender;
  CustomerType = CustomerType;
  readonly enumName = enumName;

  constructor(
    private customerService: CustomerService,
    private route: ActivatedRoute,
    private router: Router,
    private routesService: RoutesService,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadData(id);
    } else {
      this.goBack();
    }
  }

  ngOnDestroy(): void {
    this.routesService.remove([this.ROUTE_NAME]);
    this.destroy$.next();
    this.destroy$.complete();
  }

  goBack(): void {
    this.router.navigate(['/partner/customers']);
  }

  private loadData(id: string) {
    this.loading = true;
    this.customerService
      .get(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.customer = res;
          this.loading = false;

          this.routesService.add([
            {
              path: `/partner/customers/details/${this.customer.id}`,
              name: this.ROUTE_NAME,
              parentName: '::Menu:Customers',
              iconClass: 'fas fa-user',
              layout: eLayoutType.application,
            },
          ]);
        },
        error: () => this.goBack(),
      });
  }
}
