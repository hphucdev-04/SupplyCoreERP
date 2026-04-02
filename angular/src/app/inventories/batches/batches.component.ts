import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ProductBatchService } from 'src/app/proxy/batches';
import { ProductBatchDto } from 'src/app/proxy/batches/dtos';
import { MedicineService } from 'src/app/proxy/medicines';
import { MedicineDto } from 'src/app/proxy/medicines/dtos';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search/search.component';
import { enumName } from 'src/app/shared/utils/enum.util';
import { BatchQAStatus, batchQAStatusOptions } from 'src/app/proxy/enums/warehouses'; // Đảm bảo import đúng đường dẫn enum

@Component({
  selector: 'app-batches',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent],
  providers: [ListService],
  templateUrl: './batches.component.html',
  styleUrls: ['./batches.component.scss']
})
export class BatchesComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  data = { items: [], totalCount: 0 } as PagedResultDto<ProductBatchDto>;
  medicines: MedicineDto[] = [];

  // Drawer state
  isDrawerOpen = false;
  form: FormGroup;
  selectedBatch: ProductBatchDto | null = null;

  // Filters
  filterText = '';
  filterMedicineId: string = null;
  filterStatus: number = null;

  // Enum Expose
  BatchQAStatus = BatchQAStatus;
  batchStatusOptions = batchQAStatusOptions;
  readonly enumName = enumName;

  constructor(
    public readonly list: ListService,
    private batchService: ProductBatchService,
    private medicineService: MedicineService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadMedicines();

    const streamCreator = (query: any) => this.batchService.getList({
      ...query,
      filter: this.filterText,
      productId: this.filterMedicineId,
      status: this.filterStatus
    });

    this.list.maxResultCount = 10;
    this.list.hookToQuery(streamCreator)
      .pipe(takeUntil(this.destroy$))
      .subscribe((response) => {
        this.data = response;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadMedicines() {
    this.medicineService.getList({ maxResultCount: 1000 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.medicines = res.items;
      });
  }

  // ==========================================
  // ACTIONS & FILTERS
  // ==========================================
  onSearch(searchValue: string): void {
    this.filterText = searchValue;
    this.list.get();
  }

  onFilterChange() {
    this.list.get();
  }

  // ==========================================
  // CRUD
  // ==========================================
  buildForm() {
    this.form = this.fb.group({
      productId: [null, [Validators.required]],
      batchNumber: ['', [Validators.required, Validators.maxLength(50)]],
      manufacturingDate: [null, [Validators.required]],
      expiryDate: [null, [Validators.required]]
    });
  }

  openCreateDrawer() {
    this.selectedBatch = null;
    this.form.reset();
    this.isDrawerOpen = true;
  }

  editBatch(id: string) {
    this.batchService.get(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => {
        this.selectedBatch = res;
        this.form.patchValue({
          ...res,
          // Format ngày để HTML input type="date" có thể đọc được (YYYY-MM-DD)
          manufacturingDate: res.manufacturingDate ? new Date(res.manufacturingDate).toISOString().split('T')[0] : null,
          expiryDate: res.expiryDate ? new Date(res.expiryDate).toISOString().split('T')[0] : null
        });
        this.isDrawerOpen = true;
      });
  }

  closeDrawer() {
    this.isDrawerOpen = false;
    this.form.reset();
  }

  save() {
    if (this.form.invalid) return;

    // Validate nhanh NSX và HSD
    const mfg = new Date(this.form.value.manufacturingDate);
    const exp = new Date(this.form.value.expiryDate);
    if (exp <= mfg) {
      this.toaster.error('::ExpiryDateMustBeGreaterThanManufacturingDate', '::Error');
      return;
    }

    const request = this.selectedBatch?.id
      ? this.batchService.update(this.selectedBatch.id, this.form.value)
      : this.batchService.create(this.form.value);

    request.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.closeDrawer();
      this.list.get();
      this.toaster.success(this.selectedBatch?.id ? '::UpdateSuccess' : '::CreateSuccess', '::Success');
    });
  }

  deleteBatch(id: string, batchNumber: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure', {
      messageLocalizationParams: [batchNumber]
    }).subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.batchService.delete(id)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.list.get();
            this.toaster.success('::DeleteSuccess', '::Success');
          });
      }
    });
  }

  // ==========================================
  // QA WORKFLOW (Kiểm định Lô)
  // ==========================================
  approveBatch(id: string) {
    this.confirmation.info('::AreYouSureToApprove', '::Approve').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.batchService.approveQA(id).subscribe(() => {
          this.list.get();
          this.toaster.success('::ApproveSuccess', '::Success');
        });
      }
    });
  }

  rejectBatch(id: string) {
    this.confirmation.error('::AreYouSureToReject', '::Reject').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.batchService.rejectQA(id).subscribe(() => {
          this.list.get();
          this.toaster.success('::RejectSuccess', '::Success');
        });
      }
    });
  }
}