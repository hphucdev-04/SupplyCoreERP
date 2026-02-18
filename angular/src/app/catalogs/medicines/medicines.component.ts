import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil, forkJoin } from 'rxjs'; 
import { BaseUnitService } from 'src/app/proxy/base-units';
import { CategoryService } from 'src/app/proxy/categories';
import { DosageFormService } from 'src/app/proxy/dosage-forms';
import { MedicineStatus, StorageCondition, UsageRoute } from 'src/app/proxy/enums/medicines';
import { ManufacturerService } from 'src/app/proxy/manufacturers';
import { MedicineService } from 'src/app/proxy/medicines';
import { MedicineDetailDto, MedicineDto } from 'src/app/proxy/medicines/dtos';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search/search.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { MedicineDetailComponent } from './medicice-details/medicice-details.component';
import { ImportModalComponent } from 'src/app/shared/components/import-modal/import-modal.component';


@Component({
  selector: 'app-medicines',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, MedicineDetailComponent, ImportModalComponent],
  templateUrl: './medicines.component.html',
  styleUrl: './medicines.component.scss',
  providers: [ListService]
})
export class MedicinesComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  // Data Grid
  data = { items: [], totalCount: 0 } as PagedResultDto<MedicineDto>;
  
  // Drawer & Form State
  isDrawerOpen = false;
  form: FormGroup;
  selectedMedicine = {} as MedicineDetailDto;

  // Filter
  filterText = '';
  filterCategoryId: string = null;
  filterManufacturerId: string = null;
  filterStatus: number = null;

  // Dropdown Data
  categories: any[] = [];
  manufacturers: any[] = [];
  units: any[] = [];
  dosageForms: any[] = [];
  usageRouteOptions: any[] = [];
  storageConditionOptions: any[] = [];
  MedicineStatus = MedicineStatus;

  // State cho Modal Detail
  isDetailModalOpen = false;
  detailId = '';

  // State Modal Import
  isImportOpen = false;
  importFn = (file: File) => {
    const formData = new FormData();
    formData.append('file', file); 
    return this.medicineService.importExcel(formData);
  };

  // 2. Hàm Template: Gọi service -> Trả về Observable Blob
  templateFn = () => this.medicineService.getImportTemplate();

  openImport() {
    this.isImportOpen = true;
  }

  // Hàm callback khi import thành công (Reload lại lưới dữ liệu)
  onImportSuccess() {
    this.list.get();
  }

  @ViewChild('detailModal') detailModal: MedicineDetailComponent;
  constructor(
    public readonly list: ListService,
    private medicineService: MedicineService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private categoryService: CategoryService,
    private manufacturerService: ManufacturerService,
    private unitService: BaseUnitService,
    private dosageFormService: DosageFormService,
  ) {
    this.buildForm();
  }

  ngOnInit(): void {
    this.usageRouteOptions = this.mapEnumToOptions(UsageRoute);
    this.storageConditionOptions = this.mapEnumToOptions(StorageCondition);

    this.loadLookups(); 

   const streamCreator = (query) => this.medicineService.getList({ 
      ...query, 
      filter: this.filterText,
      categoryId: this.filterCategoryId,      
      manufacturerId: this.filterManufacturerId, 
      status: this.filterStatus,              
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

  //LOOKUP DATA
  loadLookups() {
    // Sử dụng forkJoin để load song song các dropdown
    forkJoin({
      cats: this.categoryService.getList({ maxResultCount: 1000 }),
      manus: this.manufacturerService.getList({ maxResultCount: 1000 }),
      units: this.unitService.getList({ maxResultCount: 1000 }),
      dosages: this.dosageFormService.getList({ maxResultCount: 1000 }),
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe(res => {
      this.categories = res.cats.items;
      this.manufacturers = res.manus.items;
      this.units = res.units.items;
      this.dosageForms = res.dosages.items;
    });
  }

  //ACTIONS
  onSearch(searchValue: string): void {
    this.filterText = searchValue;
    this.list.get();
  }

  onFilterChange() {
    this.list.get(); 
  }

  viewDetail(id: string): void {
    this.detailModal.open(id);
  }

  createMedicine(): void {
    this.selectedMedicine = {} as MedicineDetailDto;
    this.buildForm();
    this.isDrawerOpen = true;
  }

  editMedicine(id: string): void {
    this.medicineService.get(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe((res) => {
            this.selectedMedicine = res;
            this.buildForm();
            this.isDrawerOpen = true;
        });
  }

  deleteMedicine(id: string): void {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure')
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.medicineService.delete(id)
            .pipe(takeUntil(this.destroy$))  
            .subscribe(() => {
                this.list.get();
            });
        }
      });
  }

  //FORM HANDLING

  buildForm(): void {
    this.form = this.fb.group({
      code: [this.selectedMedicine.code || '', [Validators.required, Validators.maxLength(50)]],
      name: [this.selectedMedicine.name || '', [Validators.required, Validators.maxLength(255)]],
      categoryId: [this.selectedMedicine.categoryId || null, Validators.required],
      manufacturerId: [this.selectedMedicine.manufacturerId || null, Validators.required],
      baseUnitId: [this.selectedMedicine.baseUnitId || null, Validators.required],
      dosageFormId: [this.selectedMedicine.dosageFormId || null, Validators.required],
      registrationNumber: [this.selectedMedicine.registrationNumber || '', Validators.maxLength(50)],
      usageRoute: [this.selectedMedicine.usageRoute ?? null, Validators.required], 
      storageCondition: [this.selectedMedicine.storageCondition ?? null, Validators.required],
      isPrescriptionDrug: [this.selectedMedicine.isPrescriptionDrug || false],
      isActive: [this.selectedMedicine.isActive !== false] // Default true
    });

    if (!this.selectedMedicine.id) {
      this.form.get('name')?.valueChanges
        .pipe(takeUntil(this.destroy$))
        .subscribe((value) => {
          if (value) {
            const generatedCode = this.generateCode(value);
            this.form.get('code')?.setValue(generatedCode, { emitEvent: false });
          }
        });
    }
  }

  private generateCode(name: string): string {
    if (!name) return '';

    const normalizedName = name
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/đ/g, 'd')
      .replace(/Đ/g, 'D')
      .replace(/[^a-zA-Z0-9 ]/g, '')
      .trim()
      .replace(/\s+/g, '_')
      .toUpperCase();

    const randomHash = Math.random()
      .toString(36)
      .substring(2, 6)
      .toUpperCase();

    return `MED_${normalizedName}_${randomHash}`; 
  }

  private mapEnumToOptions(enumType: any): any[] {
  return Object.keys(enumType)
    .filter(key => !isNaN(Number(key)))
    .map(key => ({
      value: Number(key),
      name: enumType[key]   
    }));
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
    this.form.reset();
  }

  save(): void {
    if (this.form.invalid) return;

    const request = this.selectedMedicine.id
      ? this.medicineService.update(this.selectedMedicine.id, this.form.value)
      : this.medicineService.create(this.form.value);

    request
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
            this.closeDrawer();
            this.list.get();
        });
  }
  exportToExcel(): void {
    this.medicineService.getListAsExcelFile({
      filter: this.filterText,
      categoryId: this.filterCategoryId,
      manufacturerId: this.filterManufacturerId,
      status: this.filterStatus,
      maxResultCount: 1000,
    }).subscribe((fileResult: any) => {
       // Logic tải file xuống trình duyệt
       this.downloadBlob(fileResult, `Medicines_Export_${new Date().getTime()}.xlsx`);
    });
  }
  private downloadBlob(blob: Blob, fileName: string) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }
  approveMedicine(id: string): void {
    this.confirmation.warn('::AreYouSureToApprove', '::Approve').subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
            this.medicineService.approve(id).subscribe(() => this.list.get());
        }
    });
  }

  rejectMedicine(id: string): void {
    this.confirmation.warn('::AreYouSureToReject', '::Reject').subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
            this.medicineService.reject(id).subscribe(() => this.list.get());
        }
    });
  }

  onToggleActive(row: MedicineDto, event: any): void {
    event.stopPropagation();
    this.confirmation.warn(
        row.isActive ? '::AreYouSureToDeactivate' : '::AreYouSureToActivate',
        '::Confirm'
    ).subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
            this.medicineService.toggleActive(row.id).subscribe(() => this.list.get());
        } else {
            event.target.checked = row.isActive; 
        }
    });
  }
}