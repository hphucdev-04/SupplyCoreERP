import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil, forkJoin } from 'rxjs'; 
import { BaseUnitService } from 'src/app/proxy/base-units';
import { CategoryService } from 'src/app/proxy/categories';
import { DosageFormService } from 'src/app/proxy/dosage-forms';
import { MedicineStatus, StorageCondition, UsageRoute } from 'src/app/proxy/enums/medicines';
import { ManufacturerService } from 'src/app/proxy/manufacturers';
import { MedicineService } from 'src/app/proxy/medicines';
import { CreateUpdateMedicineDto, GetMedicineListDto, MedicineDetailDto, MedicineDto } from 'src/app/proxy/medicines/dtos';
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
  
  //Data
  data = { items: [], totalCount: 0 } as PagedResultDto<MedicineDto>;
  
  //Drawer state
  isDrawerOpen = false;
  form: FormGroup;
  selectedMedicine = {} as MedicineDetailDto;

  //Filter
  filterText = '';
  filterCategoryId: string = null;
  filterManufacturerId: string = null;
  filterStatus: number = null;

  //Dropdown Data
  categories: any[] = [];
  manufacturers: any[] = [];
  units: any[] = [];
  dosageForms: any[] = [];
  usageRouteOptions: any[] = [];
  storageConditionOptions: any[] = [];
  MedicineStatus = MedicineStatus;

  //Modal detail state
  isDetailModalOpen = false;
  detailId = '';

  //Modal import state
  isImportOpen = false;
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
    private toaster: ToasterService,
  ) {
    this.buildForm();
  }

  ngOnInit(): void {
    this.usageRouteOptions = this.mapEnumToOptions(UsageRoute);
    this.storageConditionOptions = this.mapEnumToOptions(StorageCondition);

    this.loadLookups(); 

   const streamCreator = (query: GetMedicineListDto) => this.medicineService.getList({ 
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

  //Action
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
                this.toaster.success('::DeleteSuccess', ':Success')
            });
        }
      });
  }

  //Form Handling
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
  }

  //Auto generate code function
  generateCode(): void {
  const name = this.form.get('name')?.value || '';
  
  const normalized = name
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd').replace(/Đ/g, 'D')
    .replace(/[^a-zA-Z0-9 ]/g, '')
    .trim()
    .replace(/\s+/g, '_')
    .toUpperCase()
    .substring(0, 20);

  // Crypto random — 4 bytes -> 8 ký tự hex, đủ 4 tỷ khả năng
  const array = new Uint32Array(1);
  crypto.getRandomValues(array);
  const cryptoHex = array[0].toString(16).toUpperCase().padStart(8, '0');

  const code = normalized
    ? `MED_${normalized}_${cryptoHex}`   // MED_PANADOL_3F2A1B4C
    : `MED_${cryptoHex}`;

  this.form.get('code')?.setValue(code);
}

  //Map enum to dropdown
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
    const payload = this.form.getRawValue() as CreateUpdateMedicineDto
    const request = this.selectedMedicine.id
      ? this.medicineService.update(this.selectedMedicine.id, payload)
      : this.medicineService.create(payload);

    request
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
            this.closeDrawer();
            this.list.get();
            this.toaster.success(
              this.selectedMedicine.id? '::UpdateSuccess' : '::CreateSuccess', '::Success'
            );
        });
  }

  //Export
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
       this.toaster.success('::ExportSuccess', '::Success');
    });
  }

  // Download file function
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

  //Import
  importFn = (file: File) => {
    const formData = new FormData();
    formData.append('file', file); 
    return this.medicineService.importExcel(formData);
  };

  //Template: Gọi service -> Trả về Observable Blob
  templateFn = () => this.medicineService.getImportTemplate();

  openImport() {
    this.isImportOpen = true;
  }

  //Callback khi import thành công 
  onImportSuccess() {
    this.list.get();
  }

  approveMedicine(id: string): void {
    this.confirmation.warn('::AreYouSureToApprove', '::Approve').subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
            this.medicineService.approve(id).subscribe(() => this.list.get());
            this.toaster.success('::ApproveSuccess', '::Success')
        }
    });
  }
  
  rejectMedicine(id: string): void {
    this.confirmation.warn('::AreYouSureToReject', '::Reject').subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
            this.medicineService.reject(id).subscribe(() => this.list.get());
            this.toaster.success('::RejectSuccess', '::Success')
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
            this.toaster.success(
              row.isActive? '::DeactivateSuccessfully' : '::ActivateSuccessfully', '::Success'
            );
        } else {
            this.toaster.error('::Error');
            event.target.checked = row.isActive; 
        }
    });
  }
}