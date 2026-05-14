import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { CustomerService } from 'src/app/proxy/customers';
import { CustomerDto, CustomerDetailDto, CreateUpdateCustomerDto, GetCustomerListDto } from 'src/app/proxy/customers/dtos';
import { Gender, CustomerType, genderOptions, customerTypeOptions } from 'src/app/proxy/enums/partner';
import { LocationService } from 'src/app/proxy/locations';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search-component/search.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { CurrencyFormatDirective } from 'src/app/shared/directives/currency-format.directive';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, CurrencyFormatDirective],
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.scss',
  providers: [ListService]
})
export class CustomersComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  // Data Grid
  data = { items: [], totalCount: 0 } as PagedResultDto<CustomerDto>;

  // Drawer & Form State
  isDrawerOpen = false;
  form: FormGroup;
  selectedCustomer = {} as CustomerDetailDto;

  // Filter
  filterText = '';
  filterIsActive: boolean | null = null;

  // Dropdown Data
  countries: any[] = [];
  cities: any[] = [];
  areas: any[] = [];
  genders = genderOptions
  customerTypes = customerTypeOptions

  Gender = Gender;
  CustomerType = CustomerType;

  constructor(
    public readonly list: ListService,
    private customerService: CustomerService,
    private locationService: LocationService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.buildForm();
    this.loadInitialLookups();
    const streamCreator = (query: GetCustomerListDto) => this.customerService.getList({
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

  loadInitialLookups() {
    this.locationService.getAllCountries()
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.countries = res.items;
      });
  }

  onCountryChange() {
    const countryId = this.form.get('countryId')?.value;
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
    this.router.navigate(['/partner/customers/details', id]);
  }

  createCustomer(): void {
    this.selectedCustomer = {} as CustomerDto;
    this.cities = [];
    this.areas = [];
    this.form.reset({
      debtLimit: 0,
      paymentTermDays: 0,
      isActive: true,
    });
    this.isDrawerOpen = true;
  }

  editCustomer(id: string): void {
    this.customerService.get(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => {
        this.selectedCustomer = res;

        // Patch data vào form (tách biệt với buildForm)
        this.form.patchValue(res);

        // Load cascading dropdowns nếu có sẵn
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


  deleteCustomer(id: string): void {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure')
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.customerService.delete(id)
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

  onToggleActive(row: CustomerDto, event: any): void {
    event.stopPropagation();
    this.confirmation.warn(
      row.isActive ? '::AreYouSureToDeactivate' : '::AreYouSureToActivate',
      '::Confirm'
    ).subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.customerService.toggleActive(row.id).subscribe(() => this.list.get());
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
      phoneNumber: ['', Validators.maxLength(20)],
      email: ['', [Validators.email, Validators.maxLength(128)]],
      representativeName: ['', Validators.maxLength(255)],
      gender: [null],
      type: [null, Validators.required],
      taxCode: ['', Validators.maxLength(50)],
      note: ['', Validators.maxLength(1000)],
      debtLimit: [0, [Validators.required, Validators.min(0)]],
      paymentTermDays: [0, [Validators.required, Validators.min(0)]],
      countryId: [null],
      cityId: [null],
      areaId: [null],
      address: ['', Validators.maxLength(500)],
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

    const payload = this.form.getRawValue() as CreateUpdateCustomerDto;
    const request = this.selectedCustomer?.id
      ? this.customerService.update(this.selectedCustomer.id, payload)
      : this.customerService.create(payload);

    request
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.closeDrawer();
        this.list.get();
        this.toaster.success(
          this.selectedCustomer?.id ? '::UpdateSuccess' : '::CreateSuccess', '::Success'
        );
      });
  }
}