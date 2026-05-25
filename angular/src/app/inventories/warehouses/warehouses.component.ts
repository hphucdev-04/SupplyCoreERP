import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { AreaDto, CityDto, CountryDto } from 'src/app/proxy/locations/dtos';
import { ApprovalStatus } from 'src/app/proxy/enums/warehouses/approval-status.enum';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { LocationService } from 'src/app/proxy/locations';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search-component/search.component';
import { Router } from '@angular/router';
import { enumName } from 'src/app/shared/untils/enum.util';

@Component({
  standalone: true,
  selector: 'app-warehouses',
  templateUrl: './warehouses.component.html',
  styleUrls: ['./warehouses.component.scss'],
  providers: [ListService],
  imports: [SharedModule, DrawerComponent, SearchComponent],
})
export class WarehousesComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  warehouse = { items: [], totalCount: 0 } as PagedResultDto<WarehouseDto>;

  isDrawerOpen = false;
  form: FormGroup;
  selectedWarehouse: WarehouseDto;

  filterText = '';

  countries: CountryDto[] = [];
  cities: CityDto[] = [];
  areas: AreaDto[] = [];

  approvalStatus = ApprovalStatus;

  // --- TỶ LỆ QUY ĐỔI (THỰC TẾ) ---
  // 1 mét (m) = 20 pixels (px) trên bản vẽ (Đồng bộ với Map Layout)
  readonly PX_PER_M = 20;
  readonly enumName = enumName;

  constructor(
    public readonly list: ListService,
    private warehouseService: WarehouseService,
    private locationService: LocationService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder,
    private router: Router,
    private toaster: ToasterService,
  ) {}

  ngOnInit() {
    this.buildForm();
    this.loadWarehouses();
    this.loadCountries();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ==============================================
  // HELPER CHUYỂN ĐỔI MÉT <--> PIXEL
  // ==============================================
  toM(px: number | undefined | null): number {
    if (px == null) return 0;
    return Number((px / this.PX_PER_M).toFixed(2));
  }

  toPx(m: number | undefined | null): number {
    if (m == null) return 0;
    return Math.round(m * this.PX_PER_M);
  }

  loadWarehouses() {
    const streamCreator = query =>
      this.warehouseService.getList({
        ...query,
        filter: this.filterText,
      });

    this.list
      .hookToQuery(streamCreator)
      .pipe(takeUntil(this.destroy$))
      .subscribe(response => {
        this.warehouse = response;
      });
  }

  onSearch(searchValue: string): void {
    this.filterText = searchValue;
    this.list.get();
  }

  manageLocations(id: string): void {
    this.router.navigate(['/inventory/warehouses', 'layouts', id]);
  }

  loadCountries() {
    this.locationService
      .getAllCountries()
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.countries = res.items;
      });
  }

  onCountryChange(countryId: string) {
    // Reset data các cấp nhỏ hơn
    this.form.get('cityId').setValue(null);
    this.form.get('areaId').setValue(null);
    this.cities = [];
    this.areas = [];

    if (countryId) {
      this.locationService
        .getCitiesByCountry(countryId) // Gọi API lấy city theo country
        .pipe(takeUntil(this.destroy$))
        .subscribe(res => {
          this.cities = res.items;
        });
    }
  }
  onCityChange(cityId: string) {
    this.form.get('areaId').setValue(null);
    this.areas = [];

    if (cityId) {
      this.locationService
        .getAreasByCity(cityId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(res => {
          this.areas = res.items;
        });
    }
  }

  // ==============================================
  // QUẢN LÝ FORM & NGHIỆP VỤ
  // ==============================================
  buildForm() {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      address: ['', [Validators.maxLength(500)]],
      countryId: [null],
      cityId: [null],
      areaId: [null],
      // Validator tính bằng MÉT (Ví dụ: Tối thiểu 5m x 5m, Tối đa 500m x 500m)
      mapWidth: [50, [Validators.required, Validators.min(5), Validators.max(500)]],
      mapLength: [50, [Validators.required, Validators.min(5), Validators.max(500)]],
    });
  }

  createWarehouse() {
    this.selectedWarehouse = null;
    // Reset form với giá trị mặc định là 50m x 50m
    this.form.reset({ mapWidth: 50, mapLength: 50 });
    this.isDrawerOpen = true;
  }

  editWarehouse(id: string) {
    this.warehouseService
      .get(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.selectedWarehouse = res;

        // Cần đổi từ Pixel (DB) sang Mét (Form) để hiển thị
        this.form.patchValue({
          ...res,
          mapWidth: this.toM(res.mapWidth),
          mapLength: this.toM(res.mapLength),
        });

        // Cascading data cho form Edit
        if (res.countryId) {
          this.locationService
            .getCitiesByCountry(res.countryId)
            .pipe(takeUntil(this.destroy$))
            .subscribe(cityRes => {
              this.cities = cityRes.items;
              this.form.get('cityId').setValue(res.cityId); // Set lại giá trị city sau khi list đã load xong
            });
        }

        if (res.cityId) {
          this.locationService
            .getAreasByCity(res.cityId)
            .pipe(takeUntil(this.destroy$))
            .subscribe(areaRes => {
              this.areas = areaRes.items;
              this.form.get('areaId').setValue(res.areaId); // Set lại giá trị area sau khi list đã load xong
            });
        }

        this.isDrawerOpen = true;
      });
  }

  closeDrawer() {
    this.isDrawerOpen = false;
    this.form.reset();
  }

  save() {
    if (this.form.invalid) return;

    // Đổi từ Mét (Form) sang Pixel (DB) trước khi lưu
    const payload = {
      ...this.form.value,
      mapWidth: this.toPx(this.form.value.mapWidth),
      mapLength: this.toPx(this.form.value.mapLength),
    };

    const request = this.selectedWarehouse?.id
      ? this.warehouseService.update(this.selectedWarehouse.id, payload)
      : this.warehouseService.create(payload);

    request.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.closeDrawer();
      this.list.get();
    });
  }

  delete(id: string, name: string) {
    this.confirmation
      .warn('::WarehouseDeletionConfirmationMessage', '::AreYouSure', {
        messageLocalizationParams: [name],
      })
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.warehouseService
            .delete(id)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => this.list.get());
        }
      });
  }

  sendToApprove(id: string) {
    this.confirmation
      .warn('::SendWarehouseToApprovalConfirmationMessage', '::AreYouSure')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.warehouseService.sendToApprove(id).subscribe(() => this.list.get());
        }
      });
  }

  approve(id: string) {
    this.warehouseService
      .approve(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.list.get());
  }

  reject(id: string) {
    this.warehouseService
      .reject(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.list.get());
  }

  onToggleActive(row: WarehouseDto, event: any): void {
    event.stopPropagation();
    this.confirmation
      .warn(row.isActive ? '::AreYouSureToDeactivate' : '::AreYouSureToActivate', '::Confirm')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.warehouseService.toggleActive(row.id).subscribe(() => this.list.get());
          this.toaster.success(
            row.isActive ? '::DeactivateSuccessfully' : '::ActivateSuccessfully',
            '::Success',
          );
        } else {
          event.target.checked = row.isActive;
        }
      });
  }
}
