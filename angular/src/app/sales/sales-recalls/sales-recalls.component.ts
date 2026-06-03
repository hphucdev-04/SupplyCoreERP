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
import { SalesRecallDto } from 'src/app/proxy/sales-recalls/dtos/models';
import { SalesRecallService } from 'src/app/proxy/sales-recalls/sales-recall.service';
import { MedicineService } from 'src/app/proxy/medicines';
import { ProductBatchService } from 'src/app/proxy/batches';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { MedicineDto } from 'src/app/proxy/medicines/dtos';
import { ProductBatchDto } from 'src/app/proxy/batches/dtos';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { SalesRecallStatus, salesRecallStatusOptions } from 'src/app/proxy/enums/orders/sales-recall-status.enum';
import { RecallLevel, recallLevelOptions } from 'src/app/proxy/enums/orders/recall-level.enum';
import { enumName } from 'src/app/shared/untils/enum.util';

@Component({
  selector: 'app-sales-recalls',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, DropdownSearchComponent],
  providers: [ListService],
  templateUrl: './sales-recalls.component.html',
})
export class SalesRecallsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  data = { items: [], totalCount: 0 } as PagedResultDto<SalesRecallDto>;
  medicines: MedicineDto[] = [];
  batches: ProductBatchDto[] = [];
  warehouses: WarehouseDto[] = [];

  isDrawerOpen = false;
  form: FormGroup;
  isSaving = false;

  filterText = '';
  filterStatus: number = null;
  filterIsOverdue: boolean = null;

  SalesRecallStatus = SalesRecallStatus;
  salesRecallStatusOptions = salesRecallStatusOptions;
  RecallLevel = RecallLevel;
  recallLevelOptions = recallLevelOptions;
  readonly enumName = enumName;

  constructor(
    public readonly list: ListService,
    private recallService: SalesRecallService,
    private medicineService: MedicineService,
    private batchService: ProductBatchService,
    private warehouseService: WarehouseService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadLookups();

    const streamCreator = (query: any) =>
      this.recallService.getList({
        ...query,
        filter: this.filterText,
        status: this.filterStatus,
      });

    this.list.maxResultCount = 10;
    this.list
      .hookToQuery(streamCreator)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        // Tự động kiểm tra overdue trên client (hoặc dùng trường IsOverdue từ backend)
        this.data = res;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLookups() {
    // Load danh sách sản phẩm thuốc để chọn
    this.medicineService
      .getList({ maxResultCount: 1000, skipCount: 0, isActive: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.medicines = res.items));

    // Load danh sách kho nhận hàng thu hồi
    this.warehouseService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.warehouses = res.items));
  }

  onMedicineChange(productId: string) {
    this.form.get('productBatchId').setValue(null);
    if (!productId) {
      this.batches = [];
      return;
    }
    // Load danh sách lô thuốc của sản phẩm được chọn
    this.batchService
      .getList({ productId: productId, maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.batches = res.items));
  }

  onSearch(value: string) {
    this.filterText = value;
    this.list.get();
  }

  onFilterChange() {
    this.list.get();
  }

  viewDetail(id: string) {
    this.router.navigate(['/sales/sales-recalls/details', id]);
  }

  openCreateDrawer() {
    this.form.reset({
      recallDate: new Date().toISOString().split('T')[0],
      level: RecallLevel.Level3, // Mặc định mức 3 (ít nghiêm trọng)
    });
    this.batches = [];
    this.isDrawerOpen = true;
  }

  closeDrawer() {
    this.isDrawerOpen = false;
  }

  save() {
    if (this.form.invalid) return;
    this.isSaving = true;

    this.recallService
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
          this.recallService
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
      recallDecisionNumber: ['', [Validators.required, Validators.maxLength(256)]],
      productId: [null, [Validators.required]],
      productBatchId: [null, [Validators.required]],
      warehouseId: [null, [Validators.required]],
      recallDate: [new Date().toISOString().split('T')[0], [Validators.required]],
      level: [RecallLevel.Level3, [Validators.required]],
      note: ['', [Validators.maxLength(1000)]],
    });
  }
}
