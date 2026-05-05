import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { SupplierService } from 'src/app/proxy/suppliers';
import { SupplierDto, SupplierDetailDto, CreateUpdateSupplierDto, GetSupplierListDto } from 'src/app/proxy/suppliers/dtos';
import { LocationService } from 'src/app/proxy/locations';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search-component/search.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { Gender, genderOptions } from 'src/app/proxy/enums/partner/gender.enum';
import { CurrencyFormatDirective } from 'src/app/shared/directives/currency-format.directive';
import { Router } from '@angular/router';

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, CurrencyFormatDirective],
  templateUrl: './suppliers.component.html',
  styleUrl: './suppliers.component.scss',
  providers: [ListService]
})
export class SuppliersComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data Grid
  data = { items: [], totalCount: 0 } as PagedResultDto<SupplierDto>;

  // Drawer & Form State
  isDrawerOpen = false;
  form: FormGroup;
  selectedSupplier = {} as SupplierDetailDto;

  // Filter
  filterText = '';
  filterIsActive: boolean | null = null;

  // Dropdown Data cho Địa chỉ
  countries: any[] = [];
  cities: any[] = [];
  areas: any[] = [];
  gender = genderOptions

  //Enum
  Gender = Gender;

  constructor(
    public readonly list: ListService,
    private supplierService: SupplierService,
    private locationService: LocationService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.buildForm();
    this.loadInitialLookups();
    const streamCreator = (query: GetSupplierListDto) => this.supplierService.getList({
      ...query,
      filter: this.filterText,
      isActive: this.filterIsActive
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

  // Khởi tạo ban đầu: Chỉ load danh sách Quốc gia
  loadInitialLookups() {
    this.locationService.getAllCountries()
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.countries = res.items;
      });
  }


  // Dropdown phụ thuộc
  onCountryChange() {
    const countryId = this.form.get('countryId')?.value;

    // Reset City và Area khi Country đổi
    this.cities = [];
    this.areas = [];
    this.form.patchValue({ cityId: null, areaId: null });

    if (countryId && countryId !== 'null') {
      this.locationService.getCitiesByCountry(countryId).subscribe(res => {
        this.cities = res.items;
      });
    }
  }

  onCityChange() {
    const cityId = this.form.get('cityId')?.value;

    // Reset Area khi City đổi
    this.areas = [];
    this.form.patchValue({ areaId: null });

    if (cityId && cityId !== 'null') {
      this.locationService.getAreasByCity(cityId).subscribe(res => {
        this.areas = res.items;
      });
    }
  }

  // --- ACTIONS ---
  onSearch(searchValue: string): void {
    this.filterText = searchValue;
    this.list.get();
  }

  onFilterChange() {
    this.list.get();
  }

  viewDetail(id: string): void {
    this.router.navigate(['/partner/suppliers/details', id]);
  }

  createSupplier(): void {
    this.selectedSupplier = {} as SupplierDetailDto;
    this.cities = [];
    this.areas = [];
    this.form.reset({
      debtLimit: 0,
      paymentTermDays: 0,
      isActive: true,
    });
    this.isDrawerOpen = true;
  }

  editSupplier(id: string): void {
    this.supplierService.get(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => {
        this.selectedSupplier = res;
        this.form.patchValue(res);

        if (res.countryId) {
          this.locationService.getCitiesByCountry(res.countryId)
            .pipe(takeUntil(this.destroy$))
            .subscribe(cityRes => {
              this.cities = cityRes.items;
            });
        } else {
          this.cities = [];
        }

        if (res.cityId) {
          this.locationService.getAreasByCity(res.cityId)
            .pipe(takeUntil(this.destroy$))
            .subscribe(areaRes => {
              this.areas = areaRes.items;
            });
        } else {
          this.areas = [];
        }

        this.isDrawerOpen = true;
      });
  }

  deleteSupplier(id: string): void {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure')
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.supplierService.delete(id)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.list.get();
                this.toaster.success('::DeleteSuccess', '::Success');
              },
              error: (err) => {
                this.toaster.error(err.error?.error?.message || '::Error');
              }
            });
        }
      });
  }

  onToggleActive(row: SupplierDto, event: any): void {
    event.stopPropagation();
    this.confirmation.warn(
      row.isActive ? '::AreYouSureToDeactivate' : '::AreYouSureToActivate',
      '::Confirm'
    ).subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.supplierService.toggleActive(row.id).subscribe(() => this.list.get());
        this.toaster.success(
          row.isActive ? '::DeactivateSuccessfully' : '::ActivateSuccessfully', '::Success'
        );
      } else {
        event.target.checked = row.isActive;
      }
    });
  }

  // --- FORM HANDLING ---
  buildForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      taxCode: ['', Validators.maxLength(50)],
      phoneNumber: ['', Validators.maxLength(20)],
      email: ['', [Validators.email, Validators.maxLength(128)]],
      representativeName: ['', Validators.maxLength(255)],
      gender: [null],
      debtLimit: [0, [Validators.required, Validators.min(0)]],
      paymentTermDays: [0, [Validators.required, Validators.min(0)]],
      countryId: [null],
      cityId: [null],
      areaId: [null],
      address: ['', Validators.maxLength(500)],
      note: ['', Validators.maxLength(1000)],
      isActive: [true],
    });
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
    this.form.reset({
      debtLimit: 0,
      paymentTermDays: 0,
      isActive: true,
    });
  }

  save(): void {
    if (this.form.invalid) return;
    const payload = this.form.getRawValue() as CreateUpdateSupplierDto;
    const request = this.selectedSupplier.id
      ? this.supplierService.update(this.selectedSupplier.id, payload)
      : this.supplierService.create(payload);

    request
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.closeDrawer();
        this.list.get();
        this.toaster.success(
          this.selectedSupplier.id ? '::UpdateSuccess' : '::CreateSuccess', '::Success'
        );
      });
  }
}