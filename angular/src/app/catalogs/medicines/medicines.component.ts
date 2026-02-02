import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs'; 
import { BaseUnitService } from 'src/app/proxy/base-units';
import { CategoryService } from 'src/app/proxy/categories';
import { DosageFormService } from 'src/app/proxy/dosage-forms';
import { LocationService } from 'src/app/proxy/locations';
import { ManufacturerService } from 'src/app/proxy/manufacturers';
import { MedicineService } from 'src/app/proxy/medicines';
import { MedicineDetailDto, MedicineDto } from 'src/app/proxy/medicines/dtos';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search/search.component';
import { SharedModule } from 'src/app/shared/shared.module';


@Component({
  selector: 'app-medicines',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent],
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
  filterText = '';

  // Dropdown Data
  categories: any[] = [];
  manufacturers: any[] = [];
  units: any[] = [];
  dosageForms: any[] = [];
  countries: any[] = [];

  constructor(
    public readonly list: ListService,
    private medicineService: MedicineService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private categoryService: CategoryService,
    private manufacturerService: ManufacturerService,
    private unitService: BaseUnitService,
    private dosageFormService: DosageFormService,
    private locationService: LocationService,
    private router: Router
  ) {
    this.buildForm();
  }

  ngOnInit(): void {
    this.loadLookups(); 

    const streamCreator = (query) => this.medicineService.getList({ ...query, filter: this.filterText });
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
      countries: this.locationService.getAllCountries(),
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe(res => {
      this.categories = res.cats.items;
      this.manufacturers = res.manus.items;
      this.units = res.units.items;
      this.dosageForms = res.dosages.items;
      this.countries = res.countries.items;
    });
  }

  //ACTIONS
  onSearch(searchValue: string): void {
    this.filterText = searchValue;
    this.list.get();
  }

  viewDetail(id: string): void {
    this.router.navigate(['/catalogs/medicines', id]);
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
      originCountryId: [this.selectedMedicine.originCountryId || null, Validators.required],
      registrationNumber: [this.selectedMedicine.registrationNumber || '', Validators.maxLength(50)],
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
}