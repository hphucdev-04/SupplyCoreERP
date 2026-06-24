import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, forkJoin, takeUntil } from 'rxjs';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { SupplierService } from 'src/app/proxy/suppliers';
import { MedicineService } from 'src/app/proxy/medicines';
import { BaseUnitService } from 'src/app/proxy/base-units';
import {
  SupplierDetailDto,
  SupplierProductDto,
  CreateUpdateSupplierProductDto,
  SupplierProductConditionDto,
  CreateUpdateSupplierProductConditionDto,
} from 'src/app/proxy/suppliers/dtos';
import { MedicineDto } from 'src/app/proxy/medicines/dtos';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { CurrencyFormatDirective } from 'src/app/shared/directives/currency-format.directive';
import { DropdownSearchComponent } from 'src/app/shared/components/dropdownsearch-component/dropdown-search.component';
import { enumName } from 'src/app/shared/untils/enum.util';
import { Gender } from 'src/app/proxy/enums/partner/gender.enum';

@Component({
  selector: 'app-supplier-details',
  standalone: true,
  imports: [SharedModule, DrawerComponent, CurrencyFormatDirective, DropdownSearchComponent],
  templateUrl: './supplier-details.component.html',
  styleUrls: ['./supplier-details.component.scss'],
})
export class SupplierDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private readonly ROUTE_NAME = '::Menu:SupplierDetails';

  supplier: SupplierDetailDto;
  products: SupplierProductDto[] = [];

  // Drawer for product form
  isProductDrawerOpen = false;
  productForm: FormGroup;
  isEditingProduct = false;
  editingProductId: string | null = null; // lưu productId (ID của sản phẩm)

  // Drawer for product conditions (Bottom Drawer)
  isConditionsDrawerOpen = false;
  selectedProduct: SupplierProductDto | null = null;
  conditionsForm: FormGroup;

  // Dropdown data
  allMedicines: MedicineDto[] = [];
  allUnits: any[] = [];
  allowedUnits: { id: string; name: string; conversionFactor: number }[] = [];

  // Loading state
  loading = true;

  Gender = Gender;
  readonly enumName = enumName;

  activeTab: string = 'overview';

  // Getters for template
  get activeProductsCount(): number {
    return this.products?.filter(p => p.isActive).length || 0;
  }

  get totalProductsCount(): number {
    return this.products?.length || 0;
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private routesService: RoutesService,
    private supplierService: SupplierService,
    private medicineService: MedicineService,
    private unitService: BaseUnitService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ) {
    this.initProductForm();
    this.initConditionsForm();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadData(id);
    } else {
      this.router.navigate(['/partner/suppliers']);
    }

    // Lắng nghe thay đổi trên form điều kiện để tự động validate động tại client
    this.conditionsForm.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.validateTieredPricing();
    });
  }

  ngOnDestroy(): void {
    this.routesService.remove([this.ROUTE_NAME]);
    this.destroy$.next();
    this.destroy$.complete();
  }

  goBack(): void {
    this.router.navigate(['/partner/suppliers']);
  }

  private loadData(supplierId: string) {
    this.loading = true;
    forkJoin({
      supplier: this.supplierService.get(supplierId),
      products: this.supplierService.getProductList(supplierId, { maxResultCount: 10 }),
      medicines: this.medicineService.getList({ maxResultCount: 1000 }),
      units: this.unitService.getList({ maxResultCount: 1000 }),
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.supplier = res.supplier;
          this.products = res.products.items;
          this.allMedicines = res.medicines.items;
          this.allUnits = res.units.items;
          this.loading = false;

          this.routesService.add([
            {
              path: `/partner/suppliers/details/${this.supplier.id}`,
              name: this.ROUTE_NAME,
              parentName: '::Menu:Suppliers',
              iconClass: 'fas fa-truck',
              layout: eLayoutType.application,
            },
          ]);
        },
        error: () => {
          this.toaster.error('::FailedToLoadData');
          this.router.navigate(['/partner/suppliers']);
        },
      });
  }

  refreshProducts() {
    if (this.supplier?.id) {
      this.supplierService
        .getProductList(this.supplier.id, { maxResultCount: 10 })
        .pipe(takeUntil(this.destroy$))
        .subscribe(products => (this.products = products.items));
    }
  }

  initProductForm() {
    this.productForm = this.fb.group({
      productId: [null, Validators.required],
      defaultUnitId: [null, Validators.required],
      leadTimeDays: [0, [Validators.min(0)]],
      isPreferred: [false],
      note: [''],
    });
  }

  initConditionsForm() {
    this.conditionsForm = this.fb.group({
      conditions: this.fb.array([]),
    });
  }

  get conditionsFormArray(): FormArray {
    return this.conditionsForm.get('conditions') as FormArray;
  }

  createConditionFormGroup(condition?: SupplierProductConditionDto): FormGroup {
    return this.fb.group({
      id: [condition?.id || null],
      unitId: [condition?.unitId || null, Validators.required],
      conversionFactor: [
        condition?.conversionFactor || 1,
        [Validators.required, Validators.min(1)],
      ],
      standardPrice: [condition?.standardPrice || 0, [Validators.required, Validators.min(0)]],
      minOrderQuantity: [
        condition?.minOrderQuantity || 1,
        [Validators.required, Validators.min(0.0001)],
      ],
    });
  }

  addCondition(condition?: SupplierProductConditionDto) {
    const group = this.createConditionFormGroup(condition);
    group.get('conversionFactor')?.disable();
    this.conditionsFormArray.push(group);
  }

  removeCondition(index: number) {
    this.conditionsFormArray.removeAt(index);
    this.validateTieredPricing();
  }

  validateTieredPricing() {
    const groups = this.conditionsFormArray.controls as FormGroup[];

    // 1. Reset lỗi tiered pricing custom trước khi validate lại
    groups.forEach(group => {
      const moqCtrl = group.get('minOrderQuantity');
      const priceCtrl = group.get('standardPrice');

      if (moqCtrl?.errors) {
        const { duplicateMoq, ...otherErrors } = moqCtrl.errors;
        moqCtrl.setErrors(Object.keys(otherErrors).length ? otherErrors : null);
      }
      if (priceCtrl?.errors) {
        const { priceNotDescending, ...otherErrors } = priceCtrl.errors;
        priceCtrl.setErrors(Object.keys(otherErrors).length ? otherErrors : null);
      }
    });

    // 2. Nhóm các FormGroup theo unitId
    const groupsByUnit: { [unitId: string]: { control: FormGroup; index: number }[] } = {};
    groups.forEach((group, index) => {
      const unitId = group.get('unitId')?.value;
      if (!unitId) return;
      if (!groupsByUnit[unitId]) {
        groupsByUnit[unitId] = [];
      }
      groupsByUnit[unitId].push({ control: group, index });
    });

    // 3. Duyệt qua từng nhóm đơn vị tính để kiểm tra các ràng buộc
    Object.keys(groupsByUnit).forEach(unitId => {
      const items = groupsByUnit[unitId];

      // Ràng buộc 1: Trúng MOQ cho cùng đơn vị tính
      const moqCounts: { [moq: number]: number } = {};
      items.forEach(item => {
        const moq = item.control.get('minOrderQuantity')?.value;
        if (moq !== null && moq !== undefined && moq > 0) {
          moqCounts[moq] = (moqCounts[moq] || 0) + 1;
        }
      });

      items.forEach(item => {
        const moqCtrl = item.control.get('minOrderQuantity');
        const moq = moqCtrl?.value;
        if (moq !== null && moq !== undefined && moq > 0 && moqCounts[moq] > 1) {
          moqCtrl.setErrors({ ...moqCtrl.errors, duplicateMoq: true });
        }
      });

      // Ràng buộc 2: Quy tắc giá lũy tiến giảm (Quy tắc B)
      // Lọc ra các dòng hợp lệ có MOQ và đơn giá lớn hơn 0 để so sánh
      const validItems = items.filter(item => {
        const moq = item.control.get('minOrderQuantity')?.value;
        const price = item.control.get('standardPrice')?.value;
        return (
          moq !== null &&
          moq !== undefined &&
          moq > 0 &&
          price !== null &&
          price !== undefined &&
          price > 0
        );
      });

      // Sắp xếp các dòng theo MOQ tăng dần
      const sortedItems = [...validItems].sort((a, b) => {
        const moqA = a.control.get('minOrderQuantity')?.value;
        const moqB = b.control.get('minOrderQuantity')?.value;
        return moqA - moqB;
      });

      // Kiểm tra: Đơn giá của mốc MOQ lớn hơn phải nhỏ hơn hoặc bằng mốc MOQ nhỏ hơn
      let maxAllowedPrice = Infinity;
      sortedItems.forEach(item => {
        const priceCtrl = item.control.get('standardPrice');
        const currPrice = priceCtrl?.value || 0;

        if (currPrice > maxAllowedPrice) {
          priceCtrl?.setErrors({ ...priceCtrl.errors, priceNotDescending: true });
        }

        // Cập nhật giá trị trần cho các mốc MOQ lớn hơn tiếp theo
        if (currPrice < maxAllowedPrice) {
          maxAllowedPrice = currPrice;
        }
      });
    });
  }

  isDefaultUnit(unitId: string): boolean {
    return unitId === this.selectedProduct?.defaultUnitId;
  }

  onUnitChange(index: number) {
    const group = this.conditionsFormArray.at(index) as FormGroup;
    const unitId = group.get('unitId')?.value;
    const unitConfig = this.allowedUnits.find(u => u.id === unitId);
    if (unitConfig) {
      group.get('conversionFactor')?.setValue(unitConfig.conversionFactor);
    } else {
      group.get('conversionFactor')?.setValue(1);
    }
    group.get('conversionFactor')?.disable();
  }

  getDefaultUnitCondition(product: SupplierProductDto): SupplierProductConditionDto | undefined {
    return product.conditions?.find(c => c.unitId === product.defaultUnitId);
  }

  getOtherUnitsTooltip(product: SupplierProductDto): string {
    if (!product.conditions || product.conditions.length <= 1) return '';
    return product.conditions
      .filter(c => c.unitId !== product.defaultUnitId)
      .map(
        c =>
          `${c.unitName}: ${c.standardPrice?.toLocaleString('vi-VN')} ₫ (Hệ số: ${c.conversionFactor})`,
      )
      .join('\n');
  }

  openAddProductDrawer() {
    this.isEditingProduct = false;
    this.editingProductId = null;

    this.productForm.reset({
      leadTimeDays: 0,
      isPreferred: false,
      note: '',
    });
    this.productForm.get('productId')?.enable();
    this.productForm.get('defaultUnitId')?.enable();
    this.isProductDrawerOpen = true;
  }

  openEditProductDrawer(product: SupplierProductDto) {
    this.isEditingProduct = true;
    this.editingProductId = product.productId; // LƯU Ý: dùng product.productId

    this.productForm.patchValue({
      productId: product.productId,
      defaultUnitId: product.defaultUnitId,
      leadTimeDays: product.leadTimeDays,
      isPreferred: product.isPreferred,
      note: product.note,
    });

    this.productForm.get('productId')?.disable();
    this.productForm.get('defaultUnitId')?.disable();
    this.isProductDrawerOpen = true;
  }

  saveProduct() {
    if (this.productForm.invalid) return;
    const rawValue = this.productForm.getRawValue();
    const payload: CreateUpdateSupplierProductDto = {
      productId: rawValue.productId,
      defaultUnitId: rawValue.defaultUnitId,
      leadTimeDays: rawValue.leadTimeDays,
      isPreferred: rawValue.isPreferred,
      note: rawValue.note,
      conditions: [],
    };

    const request = this.isEditingProduct
      ? this.supplierService.updateProduct(this.supplier.id, this.editingProductId!, payload)
      : this.supplierService.addProduct(this.supplier.id, payload);

    request.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.isProductDrawerOpen = false;
        this.refreshProducts();
        this.toaster.success(
          this.isEditingProduct ? '::UpdateSuccess' : '::CreateSuccess',
          '::Success',
        );
      },
      error: err => this.toaster.error(err.error?.error?.message || '::Error'),
    });
  }

  openConditionsDrawer(product: SupplierProductDto) {
    this.selectedProduct = product;
    this.conditionsFormArray.clear();
    this.allowedUnits = [];

    this.medicineService.get(product.productId!).subscribe({
      next: medDetail => {
        const baseUnit = {
          id: medDetail.baseUnitId!,
          name: medDetail.baseUnitName!,
          conversionFactor: 1,
        };

        const conversionUnits = (medDetail.units || []).map(u => ({
          id: u.unitId!,
          name: u.unitName!,
          conversionFactor: u.conversionFactor || 1,
        }));

        this.allowedUnits = [baseUnit, ...conversionUnits];

        if (product.conditions && product.conditions.length > 0) {
          product.conditions.forEach(cond => {
            const group = this.createConditionFormGroup(cond);
            group.get('conversionFactor')?.disable();
            this.conditionsFormArray.push(group);
          });
        } else {
          const defUnitConfig = this.allowedUnits.find(u => u.id === product.defaultUnitId);
          const defFactor = defUnitConfig ? defUnitConfig.conversionFactor : 1;

          const defaultGroup = this.createConditionFormGroup({
            unitId: product.defaultUnitId,
            conversionFactor: defFactor,
            standardPrice: 0,
            minOrderQuantity: 1,
            overDeliveryTolerancePct: 0,
            underDeliveryTolerancePct: 0,
          } as SupplierProductConditionDto);
          defaultGroup.get('conversionFactor')?.disable();
          this.conditionsFormArray.push(defaultGroup);
        }

        this.isConditionsDrawerOpen = true;
        this.validateTieredPricing();
      },
      error: () => {
        this.toaster.error('::FailedToLoadData');
      },
    });
  }

  saveConditions() {
    if (this.conditionsForm.invalid || !this.selectedProduct) return;

    const rawConditions = this.conditionsFormArray.getRawValue();

    const payload: CreateUpdateSupplierProductDto = {
      productId: this.selectedProduct.productId,
      defaultUnitId: this.selectedProduct.defaultUnitId,
      leadTimeDays: this.selectedProduct.leadTimeDays,
      isPreferred: this.selectedProduct.isPreferred,
      note: this.selectedProduct.note,
      conditions: rawConditions,
    };

    this.supplierService
      .updateProduct(this.supplier.id, this.selectedProduct.productId, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isConditionsDrawerOpen = false;
          this.refreshProducts();
          this.toaster.success('::UpdateSuccess', '::Success');
        },
        error: err => this.toaster.error(err.error?.error?.message || '::Error'),
      });
  }

  deleteProduct(productId: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.supplierService
          .removeProduct(this.supplier.id, productId)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: () => {
              this.refreshProducts();
              this.toaster.success('::DeleteSuccess', '::Success');
            },
            error: err => this.toaster.error(err.error?.error?.message || '::Error'),
          });
      }
    });
  }

  toggleProductActive(product: SupplierProductDto, event: any) {
    event.stopPropagation();
    const action = product.isActive ? 'deactivate' : 'activate';
    const confirmKey =
      action === 'activate' ? '::AreYouSureToActivate' : '::AreYouSureToDeactivate';
    this.confirmation.warn(confirmKey, '::Confirm').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        // Dùng product.productId
        this.supplierService
          .toggleProductActive(this.supplier.id, product.productId)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: () => {
              this.refreshProducts();
              const successKey =
                action === 'activate' ? '::ActivateSuccessfully' : '::DeactivateSuccessfully';
              this.toaster.success(successKey, '::Success');
            },
            error: err => this.toaster.error(err.error?.error?.message || '::Error'),
          });
      } else {
        event.target.checked = product.isActive;
      }
    });
  }

  getAvailableMedicines() {
    const addedProductIds = this.products.map(p => p.productId);
    return this.allMedicines.filter(m => !addedProductIds.includes(m.id));
  }
}
