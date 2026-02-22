import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil, forkJoin } from 'rxjs'; 
import { CustomerService } from 'src/app/proxy/customers';
import { CustomerDto, CustomerDetailDto, CreateUpdateCustomerDto, GetCustomerListDto } from 'src/app/proxy/customers/dtos';
import { Gender, CustomerType } from 'src/app/proxy/enums/partner';
import { LocationService } from 'src/app/proxy/locations';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search/search.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { CustomerDetailsComponent } from './customer-details/customer-details.component';
import { CurrencyFormatDirective } from 'src/app/shared/directives/currency-format.directive';


@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, CustomerDetailsComponent, CurrencyFormatDirective],
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
  genders: any[] = [];
  customerTypes: any[] = [];

  Gender = Gender;
  CustomerType = CustomerType;

  // Detail Modal
  @ViewChild('detailModal') detailModal: CustomerDetailsComponent;

  constructor(
    public readonly list: ListService,
    private customerService: CustomerService,
    private locationService: LocationService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ) {
    this.buildForm();
  }

  ngOnInit(): void {
    this.genders = this.mapEnumToOptions(Gender);
    this.customerTypes = this.mapEnumToOptions(CustomerType);
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

  private mapEnumToOptions(enumType: any): any[] {
    return Object.keys(enumType)
      .filter(key => !isNaN(Number(key)))
      .map(key => ({
        value: Number(key),
        name: enumType[key]   
      }));
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
    this.detailModal.open(id);
  }

  createCustomer(): void {
    this.selectedCustomer = {} as CustomerDetailDto;
    this.buildForm();
    this.cities = [];
    this.areas = [];
    this.isDrawerOpen = true;
  }

  editCustomer(id: string): void {
    this.customerService.get(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe((res) => {
            this.selectedCustomer = res;
            this.buildForm();
            
            if (res.countryId) {
                this.locationService.getCitiesByCountry(res.countryId).subscribe(cityRes => {
                    this.cities = cityRes.items;
                });
            } else { this.cities = []; }

            if (res.cityId) {
                this.locationService.getAreasByCity(res.cityId).subscribe(areaRes => {
                    this.areas = areaRes.items;
                });
            } else { this.areas = []; }

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

  getEnumName(enumObj: any, value: number): string {
    return enumObj[value];
  }

  // --- FORM HANDLING ---
  buildForm(): void {

    this.form = this.fb.group({
      code: [this.selectedCustomer.code || '', [Validators.required, Validators.maxLength(50)]],
      name: [this.selectedCustomer.name || '', [Validators.required, Validators.maxLength(255)]],
      phoneNumber: [this.selectedCustomer.phoneNumber || '', Validators.maxLength(20)],
      email: [this.selectedCustomer.email || '', [Validators.email, Validators.maxLength(128)]],
      representativeName: [this.selectedCustomer.representativeName || '', Validators.maxLength(255)],
      gender: [this.selectedCustomer.gender ?? null],
      type: [this.selectedCustomer.type ?? null, Validators.required],
      taxCode: [this.selectedCustomer.taxCode || '', Validators.maxLength(50)],

      note: [this.selectedCustomer.note || '', Validators.maxLength(1000)],
      debtLimit: [this.selectedCustomer.debtLimit || 0, [Validators.required, Validators.min(0)]],
      paymentTermDays: [this.selectedCustomer.paymentTermDays || 0, [Validators.required, Validators.min(0)]],
      
      countryId: [this.selectedCustomer.countryId || null],
      cityId: [this.selectedCustomer.cityId || null],
      areaId: [this.selectedCustomer.areaId || null],
      address: [this.selectedCustomer.address || '', Validators.maxLength(500)],
      isActive: [this.selectedCustomer.isActive !== false] // Default true
    });
  }

  generateCode(): void {
    const name = this.form.get('name')?.value || '';

    // Normalize tiếng Việt
    const normalized = name
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/đ/g, 'd').replace(/Đ/g, 'D')
      .replace(/[^a-zA-Z0-9 ]/g, '')
      .trim();

    // Lấy chữ cái đầu mỗi từ, tối đa 8 ký tự
    // "Cong ty Co phan Duoc pham Imexpharm" -> "CTCPDPI" (7 ký tự)
    const initials = normalized
      .split(/\s+/)
      .map(word => word.charAt(0))
      .join('')
      .toUpperCase()
      .substring(0, 8);

    const array = new Uint32Array(1);
    crypto.getRandomValues(array);
    const cryptoHex = array[0].toString(16).toUpperCase().padStart(8, '0');

    const code = initials
      ? `CUS_${initials}_${cryptoHex}`   // CUS_CTCPDPI_3F2A1B4C
      : `CUS_${cryptoHex}`;

    this.form.get('code')?.setValue(code);
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
    this.form.reset();
  }

  save(): void {
    if (this.form.invalid) return;
    const payload = this.form.getRawValue() as CreateUpdateCustomerDto;
    const request = this.selectedCustomer.id
      ? this.customerService.update(this.selectedCustomer.id, payload)
      : this.customerService.create(payload);

    request
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
            this.closeDrawer();
            this.list.get();
            this.toaster.success(
              this.selectedCustomer.id ? '::UpdateSuccess' : '::CreateSuccess', '::Success'
            );
        });
  }
}