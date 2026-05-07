import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, forkJoin, takeUntil } from 'rxjs';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { SupplierService } from 'src/app/proxy/suppliers';
import { MedicineService } from 'src/app/proxy/medicines';
import { BaseUnitService } from 'src/app/proxy/base-units';
import {
  SupplierDetailDto,
  SupplierProductDto,
  CreateUpdateSupplierProductDto
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
  imports: [
    SharedModule,
    DrawerComponent,
    CurrencyFormatDirective,
    DropdownSearchComponent
  ],
  templateUrl: './supplier-details.component.html',
  styleUrls: ['./supplier-details.component.scss']
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

  // Dropdown data
  allMedicines: MedicineDto[] = [];
  allUnits: any[] = [];

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
    private toaster: ToasterService
  ) {
    this.initProductForm();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadData(id);
    } else {
      this.router.navigate(['/partner/suppliers']);
    }
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
      products: this.supplierService.getProductList(supplierId, {maxResultCount: 10}),
      medicines: this.medicineService.getList({ maxResultCount: 1000 }),
      units: this.unitService.getList({ maxResultCount: 1000 })
    }).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.supplier = res.supplier;
          this.products = res.products.items;
          this.allMedicines = res.medicines.items;
          this.allUnits = res.units.items;
          this.loading = false;

          this.routesService.add([{
            path: `/partner/suppliers/details/${this.supplier.id}`,
            name: this.ROUTE_NAME,
            parentName: '::Menu:Suppliers',
            iconClass: 'fas fa-truck',
            layout: eLayoutType.application,
          }]);
        },
        error: () => {
          this.toaster.error('::FailedToLoadData');
          this.router.navigate(['/partner/suppliers']);
        }
      });
  }

  refreshProducts() {
    if (this.supplier?.id) {
      this.supplierService.getProductList(this.supplier.id, {maxResultCount: 10})
        .pipe(takeUntil(this.destroy$))
        .subscribe(products => this.products = products.items);
    }
  }

  initProductForm() {
    this.productForm = this.fb.group({
      productId: [null, Validators.required],
      defaultUnitId: [null, Validators.required],
      standardPrice: [0, [Validators.required, Validators.min(0)]],
      leadTimeDays: [0, [Validators.min(0)]],
      minOrderQuantity: [0, [Validators.min(0)]],
      isPreferred: [false],
      note: ['']
    });
  }

  openAddProductDrawer() {
    this.isEditingProduct = false;
    this.editingProductId = null;
    this.productForm.reset({
      standardPrice: 0,
      leadTimeDays: 0,
      minOrderQuantity: 0,
      isPreferred: false,
      note: ''
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
      standardPrice: product.standardPrice,
      leadTimeDays: product.leadTimeDays,
      minOrderQuantity: product.minOrderQuantity,
      isPreferred: product.isPreferred,
      note: product.note
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
      standardPrice: rawValue.standardPrice,
      leadTimeDays: rawValue.leadTimeDays,
      minOrderQuantity: rawValue.minOrderQuantity,
      isPreferred: rawValue.isPreferred,
      note: rawValue.note,
      defaultConversionFactor: 1,
      overDeliveryTolerancePct: 0,
      underDeliveryTolerancePct: 0
    };

    const request = this.isEditingProduct
      ? this.supplierService.updateProduct(this.supplier.id, this.editingProductId!, payload)
      : this.supplierService.addProduct(this.supplier.id, payload);

    request.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.isProductDrawerOpen = false;
        this.refreshProducts();
        this.toaster.success(this.isEditingProduct ? '::UpdateSuccess' : '::CreateSuccess', '::Success');
      },
      error: (err) => this.toaster.error(err.error?.error?.message || '::Error')
    });
  }

  deleteProduct(productId: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.supplierService.removeProduct(this.supplier.id, productId)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: () => {
              this.refreshProducts();
              this.toaster.success('::DeleteSuccess', '::Success');
            },
            error: (err) => this.toaster.error(err.error?.error?.message || '::Error')
          });
      }
    });
  }

  toggleProductActive(product: SupplierProductDto, event: any) {
    event.stopPropagation();
    const action = product.isActive ? 'deactivate' : 'activate';
    const confirmKey = action === 'activate' ? '::AreYouSureToActivate' : '::AreYouSureToDeactivate';
    this.confirmation.warn(confirmKey, '::Confirm').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        // Dùng product.productId
        this.supplierService.toggleProductActive(this.supplier.id, product.productId)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: () => {
              this.refreshProducts();
              const successKey = action === 'activate' ? '::ActivateSuccessfully' : '::DeactivateSuccessfully';
              this.toaster.success(successKey, '::Success');
            },
            error: (err) => this.toaster.error(err.error?.error?.message || '::Error')
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