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
import { DropdownSearchComponent } from 'src/app/shared/components/dropdownsearch-component/dropdown-search.component';
import { PurchaseReturnDto } from 'src/app/proxy/purchase-returns/dtos/models';
import { PurchaseReturnService } from 'src/app/proxy/purchase-returns/purchase-return.service';
import { SupplierService } from 'src/app/proxy/suppliers';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { PurchaseOrderService } from 'src/app/proxy/purchase-orders';
import { SupplierDto } from 'src/app/proxy/suppliers/dtos';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { PurchaseOrderDto } from 'src/app/proxy/purchase-orders/dtos';
import { PurchaseReturnStatus, purchaseReturnStatusOptions } from 'src/app/proxy/enums/orders/purchase-return-status.enum';
import { PurchaseReturnType, purchaseReturnTypeOptions } from 'src/app/proxy/enums/orders/purchase-return-type.enum';
import { enumName } from 'src/app/shared/untils/enum.util';

@Component({
  selector: 'app-purchase-returns',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, DropdownSearchComponent],
  providers: [ListService],
  templateUrl: './purchase-returns.component.html',
})
export class PurchaseReturnsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  data = { items: [], totalCount: 0 } as PagedResultDto<PurchaseReturnDto>;
  suppliers: SupplierDto[] = [];
  warehouses: WarehouseDto[] = [];
  purchaseOrders: PurchaseOrderDto[] = [];

  isDrawerOpen = false;
  form: FormGroup;
  isSaving = false;

  filterText = '';
  filterSupplierId: string = null;
  filterWarehouseId: string = null;
  filterStatus: number = null;

  PurchaseReturnStatus = PurchaseReturnStatus;
  purchaseReturnStatusOptions = purchaseReturnStatusOptions;
  PurchaseReturnType = PurchaseReturnType;
  purchaseReturnTypeOptions = purchaseReturnTypeOptions;
  readonly enumName = enumName;

  constructor(
    public readonly list: ListService,
    private returnService: PurchaseReturnService,
    private supplierService: SupplierService,
    private warehouseService: WarehouseService,
    private poService: PurchaseOrderService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadLookups();

    const streamCreator = (query: any) =>
      this.returnService.getList({
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
      .subscribe(res => (this.data = res));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLookups() {
    this.supplierService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.suppliers = res.items));

    this.warehouseService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.warehouses = res.items));
  }

  onSupplierChange(supplierId: string) {
    this.form.get('purchaseOrderId').setValue(null);
    if (!supplierId) {
      this.purchaseOrders = [];
      return;
    }
    // Load danh sách PurchaseOrders đã duyệt của Supplier để liên kết
    this.poService
      .getList({ maxResultCount: 1000, skipCount: 0, supplierId: supplierId } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        // Chỉ cho phép trả những PO đã được Approved hoặc Complete (đã nhận hàng)
        const list = res.items.filter(po => po.status === 3 || po.status === 5);
        this.purchaseOrders = list.map(po => {
          const dateStr = po.orderDate ? new Date(po.orderDate).toLocaleDateString('vi-VN') : '';
          return {
            ...po,
            displayName: dateStr ? `${po.code} (${dateStr})` : po.code
          } as any;
        });
      });
  }

  onSearch(value: string) {
    this.filterText = value;
    this.list.get();
  }

  onFilterChange() {
    this.list.get();
  }

  viewDetail(id: string) {
    this.router.navigate(['/procurement/purchase-returns/details', id]);
  }

  openCreateDrawer() {
    this.form.reset({
      returnDate: new Date().toISOString().split('T')[0],
      returnType: PurchaseReturnType.Commercial,
    });
    this.purchaseOrders = [];
    this.isDrawerOpen = true;
  }

  closeDrawer() {
    this.isDrawerOpen = false;
  }

  save() {
    if (this.form.invalid) return;
    this.isSaving = true;

    this.returnService
      .create(this.form.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.isSaving = false;
          this.closeDrawer();
          this.toaster.success('::CreateSuccess', '::Success');
          // Chuyển hướng ngay tới trang Chi tiết để thêm Line
          this.viewDetail(res.id);
        },
        error: () => {
          this.isSaving = false;
        },
      });
  }

  delete(id: string, code: string) {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure', { messageLocalizationParams: [code] })
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.returnService
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
      purchaseOrderId: [null, [Validators.required]],
      warehouseId: [null, [Validators.required]],
      returnType: [PurchaseReturnType.Commercial, [Validators.required]],
      returnDate: [new Date().toISOString().split('T')[0], [Validators.required]],
      note: ['', [Validators.maxLength(1000)]],
    });
  }

  statusClass(status: PurchaseReturnStatus): string {
    const map: Record<number, string> = {
      [PurchaseReturnStatus.Draft]: 'ph-badge--neutral',
      [PurchaseReturnStatus.PendingApproval]: 'ph-badge--pending',
      [PurchaseReturnStatus.Approved]: 'ph-badge--info',
      [PurchaseReturnStatus.Returning]: 'ph-badge--primary',
      [PurchaseReturnStatus.Completed]: 'ph-badge--approved',
      [PurchaseReturnStatus.Rejected]: 'ph-badge--rejected',
    };
    return map[status] ?? 'ph-badge--neutral';
  }

  statusIcon(status: PurchaseReturnStatus): string {
    const map: Record<number, string> = {
      [PurchaseReturnStatus.Draft]: 'fa-pencil',
      [PurchaseReturnStatus.PendingApproval]: 'fa-clock-o',
      [PurchaseReturnStatus.Approved]: 'fa-check',
      [PurchaseReturnStatus.Returning]: 'fa-truck',
      [PurchaseReturnStatus.Completed]: 'fa-check-circle',
      [PurchaseReturnStatus.Rejected]: 'fa-times-circle',
    };
    return map[status] ?? 'fa-circle';
  }
}
