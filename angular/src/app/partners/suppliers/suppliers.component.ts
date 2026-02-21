import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs'; 
import { SupplierService } from 'src/app/proxy/suppliers';
import { SupplierDto, SupplierDetailDto, CreateUpdateSupplierDto, GetSupplierListDto } from 'src/app/proxy/suppliers/dtos';
import { LocationService } from 'src/app/proxy/locations'; 
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search/search.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { SupplierDetailsComponent } from './supplier-details/supplier-details.component';
import { Gender } from 'src/app/proxy/enums/partner/gender.enum';
import { CurrencyFormatDirective } from 'src/app/shared/directives/currency-format.directive';

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, SupplierDetailsComponent, CurrencyFormatDirective],
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
  genders: any[] = [];

  //Enum
  Gender = Gender;
  // Detail Modal
  @ViewChild('detailModal') detailModal: SupplierDetailsComponent;

  constructor(
    public readonly list: ListService,
    private supplierService: SupplierService,
    private locationService: LocationService, 
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ) {
    this.buildForm();
  }
  
  ngOnInit(): void {
    this.genders = this.mapEnumToOptions(Gender);
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
  private mapEnumToOptions(enumType: any): any[] {
      return Object.keys(enumType)
        .filter(key => !isNaN(Number(key)))
        .map(key => ({
          value: Number(key),
          name: enumType[key]   
        }));
  }

  getEnumName(enumObj: any, value: number): string {
    return enumObj[value];
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
    this.detailModal.open(id);
  }

  createSupplier(): void {
    this.selectedSupplier = {} as SupplierDetailDto;
    this.buildForm();
    this.cities = []; 
    this.areas = []; 
    this.isDrawerOpen = true;
  }

  editSupplier(id: string): void {
    this.supplierService.get(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe((res) => {
            this.selectedSupplier = res;
            this.buildForm();
            
            if (res.countryId) {
                this.locationService.getCitiesByCountry(res.countryId).subscribe(cityRes => {
                    this.cities = cityRes.items;
                });
            } else {
                this.cities = [];
            }

            if (res.cityId) {
                this.locationService.getAreasByCity(res.cityId).subscribe(areaRes => {
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
      code: [this.selectedSupplier.code || '', [Validators.required, Validators.maxLength(50)]],
      name: [this.selectedSupplier.name || '', [Validators.required, Validators.maxLength(255)]],
      taxCode: [this.selectedSupplier.taxCode || '', Validators.maxLength(50)],
      phoneNumber: [this.selectedSupplier.phoneNumber || '', Validators.maxLength(20)],
      email: [this.selectedSupplier.email || '', [Validators.email, Validators.maxLength(128)]],
      representativeName: [this.selectedSupplier.representativeName || '', Validators.maxLength(255)],
      gender: [this.selectedSupplier.gender ?? null],
      debtLimit: [this.selectedSupplier.debtLimit || 0, [Validators.required, Validators.min(0)]],
      paymentTermDays: [this.selectedSupplier.paymentTermDays || 0, [Validators.required, Validators.min(0)]],
      
      countryId: [this.selectedSupplier.countryId || null],
      cityId: [this.selectedSupplier.cityId || null],
      areaId: [this.selectedSupplier.areaId || null],
      address: [this.selectedSupplier.address || '', Validators.maxLength(500)],
      note: [this.selectedSupplier.note || '', Validators.maxLength(1000)],
      isActive: [this.selectedSupplier.isActive !== false] // Default true
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
      ? `SUP_${initials}_${cryptoHex}`   // SUP_CTCPDPI_3F2A1B4C
      : `SUP_${cryptoHex}`;

    this.form.get('code')?.setValue(code);
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
    this.form.reset();
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