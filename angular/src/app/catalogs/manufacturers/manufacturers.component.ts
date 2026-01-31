import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { LocationService } from 'src/app/proxy/locations';
import { ContinentDto, CountryDto } from 'src/app/proxy/locations/dtos';
import { ManufacturerService } from 'src/app/proxy/manufacturers';
import { ManufacturerDto } from 'src/app/proxy/manufacturers/dtos';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search/search.component';
import { SharedModule } from 'src/app/shared/shared.module';

@Component({
  selector: 'app-manufacturers',
  imports: [SharedModule, DrawerComponent, SearchComponent],
  templateUrl: './manufacturers.component.html',
  styleUrl: './manufacturers.component.scss',
  providers: [ListService]
})
export class ManufacturersComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>(); 
  
  data = { items: [], totalCount: 0 } as PagedResultDto<ManufacturerDto>;
  isDrawerOpen = false;
  form: FormGroup;
  selectedManufacturer = {} as ManufacturerDto;
  filterText = '';

  // Dữ liệu cho Dropdown
  continents: ContinentDto[] = [];
  countries: CountryDto[] = [];       
  filteredCountries: CountryDto[] = [];

  constructor(
    public readonly list: ListService<any>,
    private manufacturerService: ManufacturerService,
    private locationService: LocationService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService
  ) {
    this.buildForm();
  }

  ngOnInit(): void {
    const streamCreator = (query) => this.manufacturerService.getList({ ...query, filter: this.filterText });
    
    this.list.maxResultCount = 10;
    this.list.hookToQuery(streamCreator).subscribe((response) => {
      this.data = response;
    });

    this.loadLookups();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLookups() {
    this.locationService.getContinents()
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => this.continents = res.items);

    this.locationService.getAllCountries()
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => this.countries = res.items);
  }

  onSearch(searchValue: string): void {
    this.filterText = searchValue;
    this.list.get();
  }

  createManufacturer(): void {
    this.selectedManufacturer = {} as ManufacturerDto;
    this.buildForm();
    this.isDrawerOpen = true;
  }

  editManufacturer(id: string): void {
    this.manufacturerService.get(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => {
        this.selectedManufacturer = res;
        this.buildForm();
        
        // Trigger lọc quốc gia khi mở form sửa
        this.onContinentChange(res.continentId);
        
        this.isDrawerOpen = true;
      });
  }

  deleteManufacturer(id: string): void {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure')
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.manufacturerService.delete(id)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
              this.list.get();
            });
        }
      });
  }

  buildForm(): void {
    this.form = this.fb.group({
      name: [this.selectedManufacturer.name || '', [Validators.required, Validators.maxLength(255)]],
      continentId: [this.selectedManufacturer.continentId || null, [Validators.required]],
      countryId: [this.selectedManufacturer.countryId || null, [Validators.required]],
    });

    // Lắng nghe thay đổi Châu lục để lọc Quốc gia (Dùng takeUntil để tự hủy)
    this.form.get('continentId')?.valueChanges
      .pipe(takeUntil(this.destroy$)) 
      .subscribe((val) => {
        this.onContinentChange(val);
      });
  }

  onContinentChange(continentId: string) {
    if (!continentId) {
      this.filteredCountries = [];
      return;
    }
    
    // Lọc danh sách quốc gia
    this.filteredCountries = this.countries.filter(x => x.continentId === continentId);
    
    // Nếu quốc gia đang chọn không thuộc châu lục mới -> Reset ô quốc gia
    const currentCountryId = this.form.get('countryId')?.value;
    const isExists = this.filteredCountries.find(x => x.id === currentCountryId);
    
    // Chỉ reset khi user đang thao tác trên form (form dirty) hoặc khi tạo mới
    if (!isExists && this.form.get('continentId')?.dirty) {
      this.form.get('countryId')?.setValue(null);
    }
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
    this.form.reset();
  }

  save(): void {
    if (this.form.invalid) return;

    const request = this.selectedManufacturer.id
      ? this.manufacturerService.update(this.selectedManufacturer.id, this.form.value)
      : this.manufacturerService.create(this.form.value);

    request
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.closeDrawer();
        this.list.get();
      });
  }
}