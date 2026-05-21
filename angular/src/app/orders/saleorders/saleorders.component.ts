import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search-component/search.component';
import { SalesOrderDto } from 'src/app/proxy/sales-orders/dtos';
import { SalesOrderService } from 'src/app/proxy/sales-orders';
import { CustomerService } from 'src/app/proxy/customers';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { CustomerDto } from 'src/app/proxy/customers/dtos';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import {
  SalesOrderStatus,
  salesOrderStatusOptions,
} from 'src/app/proxy/enums/orders/sales-order-status.enum';
import { enumName } from 'src/app/shared/untils/enum.util';
import { Router } from '@angular/router';

import { DropdownSearchComponent } from 'src/app/shared/components/dropdownsearch-component/dropdown-search.component';

@Component({
  selector: 'app-sales-orders',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, DropdownSearchComponent],
  providers: [ListService],
  templateUrl: './saleorders.component.html',
})
export class SalesOrdersComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  data = { items: [], totalCount: 0 } as PagedResultDto<SalesOrderDto>;
  customers: CustomerDto[] = [];
  warehouses: WarehouseDto[] = [];

  isDrawerOpen = false;
  form: FormGroup;
  isSaving = false;

  filterText = '';
  filterCustomerId: string = null;
  filterWarehouseId: string = null;
  filterStatus: number = null;

  SalesOrderStatus = SalesOrderStatus;
  salesOrderStatusOptions = salesOrderStatusOptions;
  readonly enumName = enumName;

  constructor(
    public readonly list: ListService,
    private soService: SalesOrderService,
    private customerService: CustomerService,
    private warehouseService: WarehouseService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadLookups();

    const streamCreator = (query: any) =>
      this.soService.getList({
        ...query,
        filter: this.filterText,
        customerId: this.filterCustomerId,
        warehouseId: this.filterWarehouseId,
        status: this.filterStatus,
      });

    this.list.maxResultCount = 10;
    this.list
      .hookToQuery(streamCreator)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.data = res));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLookups() {
    this.customerService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.customers = res.items));

    this.warehouseService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.warehouses = res.items));
  }

  onSearch(value: string) {
    this.filterText = value;
    this.list.get();
  }

  onFilterChange() {
    this.list.get();
  }

  viewDetail(id: string) {
    this.router.navigate(['/order/saleorders/details', id]);
  }

  onOrderSaved() {
    this.list.get();
  }

  delete(id: string, code: string) {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure', { messageLocalizationParams: [code] })
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.soService
            .delete(id)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => this.list.get());
        }
      });
  }

  buildForm() {
    this.form = this.fb.group({
      customerId: [null, [Validators.required]],
      warehouseId: [null, [Validators.required]],
      orderDate: [new Date().toISOString().split('T')[0], [Validators.required]],
      expectedDeliveryDate: [null],
      dueDate: [null],
      note: ['', [Validators.maxLength(1000)]],
    });
  }

  openCreateDrawer() {
    this.form.reset({
      customerId: null,
      warehouseId: null,
      orderDate: new Date().toISOString().split('T')[0],
      expectedDeliveryDate: null,
      dueDate: null,
      note: '',
    });
    this.isDrawerOpen = true;
  }

  closeDrawer() {
    this.isDrawerOpen = false;
  }

  save() {
    if (this.form.invalid) return;
    this.isSaving = true;
    this.soService
      .create(this.form.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: newOrder => {
          this.isSaving = false;
          this.closeDrawer();
          this.list.get();
          this.viewDetail(newOrder.id);
        },
        error: () => (this.isSaving = false),
      });
  }

  statusClass(status: SalesOrderStatus): string {
    const map: Record<number, string> = {
      [SalesOrderStatus.Draft]: 'ph-badge--neutral',
      [SalesOrderStatus.PendingApproval]: 'ph-badge--pending',
      [SalesOrderStatus.Approved]: 'ph-badge--info',
      [SalesOrderStatus.Delivering]: 'ph-badge--primary',
      [SalesOrderStatus.Completed]: 'ph-badge--approved',
      [SalesOrderStatus.Canceled]: 'ph-badge--rejected',
    };
    return map[status] ?? 'ph-badge--neutral';
  }

  statusIcon(status: SalesOrderStatus): string {
    const map: Record<number, string> = {
      [SalesOrderStatus.Draft]: 'fa-pencil',
      [SalesOrderStatus.PendingApproval]: 'fa-clock-o',
      [SalesOrderStatus.Approved]: 'fa-check',
      [SalesOrderStatus.Delivering]: 'fa-road',
      [SalesOrderStatus.Completed]: 'fa-check-circle',
      [SalesOrderStatus.Canceled]: 'fa-times-circle',
    };
    return map[status] ?? 'fa-circle';
  }
}
