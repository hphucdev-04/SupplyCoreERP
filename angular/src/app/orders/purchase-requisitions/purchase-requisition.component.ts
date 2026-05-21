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
import { PurchaseRequisitionDto } from 'src/app/proxy/purchase-requisitions/dtos';
import { PurchaseRequisitionService } from 'src/app/proxy/purchase-requisitions';
import {
  PurchaseRequisitionStatus,
  purchaseRequisitionStatusOptions,
} from 'src/app/proxy/enums/orders/purchase-requisition-status.enum';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { enumName } from 'src/app/shared/untils/enum.util';
import { DropdownSearchComponent } from 'src/app/shared/components/dropdownsearch-component/dropdown-search.component';

@Component({
  selector: 'app-purchase-requisition',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, DropdownSearchComponent],
  providers: [ListService],
  templateUrl: './purchase-requisition.component.html',
})
export class PurchaseRequisitionComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  data = { items: [], totalCount: 0 } as PagedResultDto<PurchaseRequisitionDto>;

  isDrawerOpen = false;
  form: FormGroup;
  isSaving = false;

  filterText = '';
  filterStatus: number = null;

  warehouses: WarehouseDto[] = [];
  PurchaseRequisitionStatus = PurchaseRequisitionStatus;
  purchaseRequisitionStatusOptions = purchaseRequisitionStatusOptions;
  readonly enumName = enumName;

  constructor(
    public readonly list: ListService,
    private prService: PurchaseRequisitionService,
    private warehouseService: WarehouseService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadWarehouses();

    const streamCreator = (query: any) =>
      this.prService.getList({
        ...query,
        filter: this.filterText,
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

  loadWarehouses() {
    this.warehouseService.getList({ maxResultCount: 1000, skipCount: 0 }).subscribe(res => {
      this.warehouses = res.items;
    });
  }

  onSearch(value: string) {
    this.filterText = value;
    this.list.get();
  }

  onFilterChange() {
    this.list.get();
  }

  goBack() {
    this.router.navigate(['/order/purchaseorders']);
  }

  viewDetail(id: string) {
    this.router.navigate(['/order/purchaserequisitions/details', id]);
  }

  delete(id: string, code: string) {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure', { messageLocalizationParams: [code] })
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.prService
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
      warehouseId: [null, [Validators.required]],
      requestedDate: [new Date().toISOString().split('T')[0], [Validators.required]],
      requiredDate: [null],
      note: ['', [Validators.maxLength(1000)]],
    });
  }

  openCreateDrawer() {
    this.form.reset({
      warehouseId: this.warehouses.length > 0 ? this.warehouses[0].id : null,
      requestedDate: new Date().toISOString().split('T')[0],
      requiredDate: null,
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
    this.prService
      .create(this.form.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.isSaving = false;
          this.closeDrawer();
          this.list.get();
          this.toaster.success('::CreateSuccess', '::Success');
          this.viewDetail(res.id);
        },
        error: () => (this.isSaving = false),
      });
  }

  statusClass(status: PurchaseRequisitionStatus): string {
    const map: Record<number, string> = {
      [PurchaseRequisitionStatus.Draft]: 'ph-badge--neutral',
      [PurchaseRequisitionStatus.PendingApproval]: 'ph-badge--pending',
      [PurchaseRequisitionStatus.Approved]: 'ph-badge--info',
      [PurchaseRequisitionStatus.Rejected]: 'ph-badge--rejected',
      [PurchaseRequisitionStatus.PartialOrdered]: 'ph-badge--pending',
      [PurchaseRequisitionStatus.Ordered]: 'ph-badge--approved',
      [PurchaseRequisitionStatus.Canceled]: 'ph-badge--rejected',
    };
    return map[status] ?? 'ph-badge--neutral';
  }

  statusIcon(status: PurchaseRequisitionStatus): string {
    const map: Record<number, string> = {
      [PurchaseRequisitionStatus.Draft]: 'fa-pencil',
      [PurchaseRequisitionStatus.PendingApproval]: 'fa-clock-o',
      [PurchaseRequisitionStatus.Approved]: 'fa-check',
      [PurchaseRequisitionStatus.Rejected]: 'fa-times-circle',
      [PurchaseRequisitionStatus.PartialOrdered]: 'fa-hourglass-half',
      [PurchaseRequisitionStatus.Ordered]: 'fa-check-circle',
      [PurchaseRequisitionStatus.Canceled]: 'fa-ban',
    };
    return map[status] ?? 'fa-circle';
  }
}
