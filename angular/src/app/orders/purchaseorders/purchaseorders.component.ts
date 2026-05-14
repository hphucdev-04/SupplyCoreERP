import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search-component/search.component';
import { PurchaseOrderDto } from 'src/app/proxy/purchase-orders/dtos';
import { PurchaseOrderService } from 'src/app/proxy/purchase-orders';
import { SupplierService } from 'src/app/proxy/suppliers';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { SupplierDto } from 'src/app/proxy/suppliers/dtos';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { PurchaseOrderStatus, purchaseOrderStatusOptions } from 'src/app/proxy/enums/orders/purchase-order-status.enum';
import { enumName } from 'src/app/shared/untils/enum.util';

@Component({
  selector: 'app-purchase-orders',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent],
  providers: [ListService],
  templateUrl: './purchaseorders.component.html',
})
export class PurchaseOrdersComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  data = { items: [], totalCount: 0 } as PagedResultDto<PurchaseOrderDto>;
  suppliers: SupplierDto[] = [];
  warehouses: WarehouseDto[] = [];

  isDrawerOpen = false;
  form: FormGroup;
  isSaving = false;

  filterText = '';
  filterSupplierId: string = null;
  filterWarehouseId: string = null;
  filterStatus: number = null;

  PurchaseOrderStatus = PurchaseOrderStatus;
  purchaseOrderStatusOptions = purchaseOrderStatusOptions;
  readonly enumName = enumName;

  constructor(
    public readonly list: ListService,
    private poService: PurchaseOrderService,
    private supplierService: SupplierService,
    private warehouseService: WarehouseService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.buildForm();
    this.loadLookups();

    const streamCreator = (query: any) =>
      this.poService.getList({
        ...query,
        filter: this.filterText,
        supplierId: this.filterSupplierId,
        warehouseId: this.filterWarehouseId,
        status: this.filterStatus,
      });

    this.list.maxResultCount = 10;
    this.list
      .hookToQuery(streamCreator)
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => (this.data = res));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLookups() {
    this.supplierService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => (this.suppliers = res.items));

    this.warehouseService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => (this.warehouses = res.items));
  }

  onSearch(value: string) {
    this.filterText = value;
    this.list.get();
  }

  onFilterChange() {
    this.list.get();
  }

  viewDetail(id: string) {
    this.router.navigate(['/order/purchaseorders/details', id]);
  }

  onOrderSaved() {
    this.list.get();
  }

  delete(id: string, code: string) {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure', { messageLocalizationParams: [code] })
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.poService
            .delete(id)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
              this.list.get();
              this.toaster.success('::DeleteSuccess', '::Success');
            });
        }
      });
  }

  buildForm() {
    this.form = this.fb.group({
      supplierId: [null, [Validators.required]],
      warehouseId: [null, [Validators.required]],
      orderDate: [new Date().toISOString().split('T')[0], [Validators.required]],
      expectedDeliveryDate: [null],
      dueDate: [{ value: null, disabled: false }],
      note: ['', [Validators.maxLength(1000)]],
    });

    // Theo dõi thay đổi của supplierId và orderDate để tính DueDate
    this.form.get('supplierId').valueChanges.pipe(takeUntil(this.destroy$)).subscribe(id => {
      this.calculateDueDate();
    });

    this.form.get('orderDate').valueChanges.pipe(takeUntil(this.destroy$)).subscribe(date => {
      this.calculateDueDate();
    });
  }

  calculateDueDate() {
    const supplierId = this.form.get('supplierId').value;
    const orderDateStr = this.form.get('orderDate').value;

    if (!supplierId || !orderDateStr) {
      this.form.get('dueDate').setValue(null);
      return;
    }

    // Lấy thông tin chi tiết nhà cung cấp để có paymentTermDays
    this.supplierService.get(supplierId).subscribe(supplier => {
      const days = supplier.paymentTermDays || 0;
      const orderDate = new Date(orderDateStr);
      if (days > 0) {
        orderDate.setDate(orderDate.getDate() + days);
        this.form.get('dueDate').setValue(orderDate.toISOString().split('T')[0]);
      } else {
        this.form.get('dueDate').setValue(orderDateStr);
      }
    });
  }

  openCreateDrawer() {
    this.form.reset({
      supplierId: null,
      warehouseId: this.warehouses[0]?.id ?? null,
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
    this.poService
      .create(this.form.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (newOrder) => {
          this.isSaving = false;
          this.closeDrawer();
          this.list.get();
          this.toaster.success('::CreateSuccess', '::Success');
          this.viewDetail(newOrder.id);
        },
        error: () => (this.isSaving = false),
      });
  }

  statusClass(status: PurchaseOrderStatus): string {
    const map: Record<number, string> = {
      [PurchaseOrderStatus.Draft]: 'badge-secondary',
      [PurchaseOrderStatus.PendingApproval]: 'badge-warning',
      [PurchaseOrderStatus.Approved]: 'badge-info',
      [PurchaseOrderStatus.Receiving]: 'badge-primary',
      [PurchaseOrderStatus.Completed]: 'badge-success',
      [PurchaseOrderStatus.Canceled]: 'badge-danger',
    };
    return map[status] ?? 'badge-secondary';
  }

  statusIcon(status: PurchaseOrderStatus): string {
    const map: Record<number, string> = {
      [PurchaseOrderStatus.Draft]: 'fa-pencil',
      [PurchaseOrderStatus.PendingApproval]: 'fa-clock-o',
      [PurchaseOrderStatus.Approved]: 'fa-check',
      [PurchaseOrderStatus.Receiving]: 'fa-truck',
      [PurchaseOrderStatus.Completed]: 'fa-check-circle',
      [PurchaseOrderStatus.Canceled]: 'fa-times-circle',
    };
    return map[status] ?? 'fa-circle';
  }
}