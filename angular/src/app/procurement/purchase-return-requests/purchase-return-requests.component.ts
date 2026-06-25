import { Component, inject, OnInit, OnDestroy } from '@angular/core';
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
import { PurchaseReturnRequestDto } from 'src/app/proxy/purchase-return-requests/dtos/models';
import { PurchaseReturnRequestService } from 'src/app/proxy/purchase-return-requests/purchase-return-request.service';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import {
  PurchaseReturnRequestStatus,
  purchaseReturnRequestStatusOptions,
} from 'src/app/proxy/enums/orders/purchase-return-request-status.enum';
import { enumName } from 'src/app/shared/untils/enum.util';

@Component({
  selector: 'app-purchase-return-requests',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, DropdownSearchComponent],
  providers: [ListService],
  templateUrl: './purchase-return-requests.component.html',
})
export class PurchaseReturnRequestsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  public readonly list = inject(ListService);
  private readonly requestService = inject(PurchaseReturnRequestService);
  private readonly warehouseService = inject(WarehouseService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  data = { items: [], totalCount: 0 } as PagedResultDto<PurchaseReturnRequestDto>;
  warehouses: WarehouseDto[] = [];

  isDrawerOpen = false;
  form: FormGroup;
  isSaving = false;

  filterText = '';
  filterWarehouseId: string = null;
  filterStatus: number = null;

  PurchaseReturnRequestStatus = PurchaseReturnRequestStatus;
  purchaseReturnRequestStatusOptions = purchaseReturnRequestStatusOptions;
  readonly enumName = enumName;

  ngOnInit(): void {
    this.buildForm();
    this.loadLookups();

    const streamCreator = (query: any) =>
      this.requestService.getList({
        ...query,
        filter: this.filterText,
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
    this.router.navigate(['/procurement/purchase-return-requests/details', id]);
  }

  openCreateDrawer() {
    this.form.reset({
      requestDate: new Date().toISOString().split('T')[0],
    });
    this.isDrawerOpen = true;
  }

  closeDrawer() {
    this.isDrawerOpen = false;
  }

  save() {
    if (this.form.invalid) return;
    this.isSaving = true;

    this.requestService
      .create(this.form.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.isSaving = false;
          this.closeDrawer();
          this.toaster.success('::CreateSuccess', '::Success');
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
          this.requestService
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
      requestDate: [new Date().toISOString().split('T')[0], [Validators.required]],
      note: ['', [Validators.maxLength(1000)]],
    });
  }

  statusClass(status: PurchaseReturnRequestStatus): string {
    const map: Record<number, string> = {
      [PurchaseReturnRequestStatus.Draft]: 'ph-badge--neutral',
      [PurchaseReturnRequestStatus.PendingApproval]: 'ph-badge--pending',
      [PurchaseReturnRequestStatus.Approved]: 'ph-badge--info',
      [PurchaseReturnRequestStatus.Rejected]: 'ph-badge--rejected',
      [PurchaseReturnRequestStatus.Processed]: 'ph-badge--approved',
    };
    return map[status] ?? 'ph-badge--neutral';
  }

  statusIcon(status: PurchaseReturnRequestStatus): string {
    const map: Record<number, string> = {
      [PurchaseReturnRequestStatus.Draft]: 'fa-pencil',
      [PurchaseReturnRequestStatus.PendingApproval]: 'fa-clock-o',
      [PurchaseReturnRequestStatus.Approved]: 'fa-check',
      [PurchaseReturnRequestStatus.Rejected]: 'fa-times-circle',
      [PurchaseReturnRequestStatus.Processed]: 'fa-check-circle',
    };
    return map[status] ?? 'fa-circle';
  }
}
