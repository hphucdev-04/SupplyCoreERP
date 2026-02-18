import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { MedicineService } from 'src/app/proxy/medicines';
import { ActiveIngredientService } from 'src/app/proxy/active-ingredients';
import { BaseUnitService } from 'src/app/proxy/base-units';
import { MedicineDetailDto } from 'src/app/proxy/medicines/dtos';
import { ProductPriceDto, PriceListDto } from 'src/app/proxy/prices/dtos'; 
import { UsageRoute, StorageCondition } from 'src/app/proxy/enums/medicines';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { PriceService } from 'src/app/proxy/prices';
import { CurrencyFormatDirective } from 'src/app/shared/directives/currency-format.directive';
import { CurrencyType } from 'src/app/proxy/enums';

@Component({
  selector: 'app-medicine-detail',
  standalone: true,
  imports: [SharedModule, ReactiveFormsModule, CommonModule, DrawerComponent, CurrencyFormatDirective],
  templateUrl: 'medicice-details.component.html',
  styleUrl: 'medicice-details.component.scss',
})
export class MedicineDetailComponent {
  
  // --- STATE MODAL ---
  isVisible = false;
  id = '';
  medicine: MedicineDetailDto;
  
  // --- STATE DATA KHÁC ---
  // List này gộp BaseUnit + Các Unit quy đổi -> Dùng để chọn khi set giá
  availableUnitsForPrice: any[] = []; 
  
  // Danh sách giá hiện tại của thuốc
  productPrices: ProductPriceDto[] = []; 
  
  // Danh sách bảng giá (Bán lẻ, Bán buôn...) để dropdown
  priceLists: PriceListDto[] = [];

  // --- DRAWER STATES ---
  isIngrDrawerOpen = false;
  isUnitDrawerOpen = false;
  isPriceDrawerOpen = false; // <--- MỚI

  // --- FORMS ---
  ingrForm: FormGroup;
  unitForm: FormGroup;
  priceForm: FormGroup; // <--- MỚI

  // --- DROPDOWN DATA (Cho Ingr/Unit Tab) ---
  allIngredients: any[] = [];
  allUnits: any[] = []; // List đơn vị toàn hệ thống (đã lọc)

  // --- TRẠNG THÁI EDIT UNIT ---
  isEditingUnit = false;
  editingUnitId: string | null = null;
  
  // --- TRẠNG THÁI EDIT PRICE (MỚI) ---
  isEditingPrice = false; 
  editingPriceId: string | null = null;

  // --- ENUMS ---
  UsageRoute = UsageRoute;
  StorageCondition = StorageCondition;
  CurrencyType = CurrencyType

  constructor(
    private medicineService: MedicineService,
    private ingredientService: ActiveIngredientService,
    private unitService: BaseUnitService,
    private priceService: PriceService, 
    private fb: FormBuilder,
    private confirmation: ConfirmationService
  ) {
    this.initForms();
  }

  // --- MAIN OPEN/LOAD ---
  open(id: string) {
    this.id = id;
    this.medicine = null; 
    this.isVisible = true;
    this.loadData(); 
  }

  close() {
    this.isVisible = false;
  }

  loadData() {
    this.medicineService.get(this.id).subscribe(res => {
      this.medicine = res;
      this.prepareUnitsForPrice(); // [MỚI] Tạo list đơn vị để chọn giá
      this.loadLookups(res); 
      this.loadPrices(); // [MỚI] Tải danh sách giá
    });
  }

  // [MỚI] Hàm tạo danh sách đơn vị cho Dropdown Giá (Base + Quy đổi)
  prepareUnitsForPrice() {
    if (!this.medicine) return;
    
    this.availableUnitsForPrice = [
        // 1. Đơn vị cơ bản
        { id: this.medicine.baseUnitId, name: this.medicine.baseUnitName + ' (Base)' },
        // 2. Các đơn vị quy đổi
        ...this.medicine.units.map(u => ({ id: u.unitId, name: u.unitName }))
    ];
  }

  // [MỚI] Load danh sách giá
  loadPrices() {
    this.priceService.getByProduct(this.id).subscribe(res => {
        this.productPrices = res;
    });
  }

  loadLookups(medicineData: MedicineDetailDto) {
    forkJoin({
      ingrs: this.ingredientService.getList({ maxResultCount: 1000 }),
      units: this.unitService.getList({ maxResultCount: 1000 }),
      priceLists: this.priceService.getPriceLists() // <--- Load danh sách bảng giá
    }).subscribe(res => {
      // Logic lọc Ingredient
      this.allIngredients = res.ingrs.items.filter(ingr => 
          !medicineData.ingredients.some(existing => existing.activeIngredientId === ingr.id)
      );

      // Logic lọc Unit
      const fullUnits = res.units.items;
      this.allUnits = fullUnits.filter(u => 
          u.id !== medicineData.baseUnitId && 
          !medicineData.units.some(existing => existing.unitId === u.id)
      );
      // Nếu đang Edit Unit, push lại unit đó vào list để hiện tên
      if (this.isEditingUnit && this.editingUnitId) {
         const currentUnit = fullUnits.find(u => u.id === this.editingUnitId);
         if (currentUnit && !this.allUnits.find(u => u.id === currentUnit.id)) {
             this.allUnits.push(currentUnit);
         }
      }

      // Lưu danh sách bảng giá
      this.priceLists = res.priceLists;
    });
  }

  initForms() {
    // Ingredients Form
    this.ingrForm = this.fb.group({
      activeIngredientId: [null, Validators.required]
    });

    // Units Form
    this.unitForm = this.fb.group({
      unitId: [null, Validators.required],
      conversionFactor: [null, [Validators.required, Validators.min(2)]],
      level: [null, [Validators.required, Validators.min(1)]]
    });

    // Price Form
    this.priceForm = this.fb.group({
        priceListId: [null, Validators.required],
        unitId: [null, Validators.required],
        price: [0, [Validators.required, Validators.min(0)]],
        minQuantity: [1, [Validators.required, Validators.min(1)]]
    });
  }

  // =========================================================
  // LOGIC INGREDIENT
  // =========================================================
  openIngrDrawer() { this.ingrForm.reset(); this.isIngrDrawerOpen = true; }
  
  saveIngr() {
    if (this.ingrForm.invalid) return;
    this.medicineService.addIngredient(this.id, this.ingrForm.value).subscribe(() => {
      this.isIngrDrawerOpen = false;
      this.loadData(); 
    });
  }

  removeIngr(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
        if (status === Confirmation.Status.confirm) {
            this.medicineService.removeIngredient(this.id, id).subscribe(() => this.loadData());
        }
    });
  }

  // =========================================================
  // LOGIC UNIT
  // =========================================================
  openUnitDrawer() {
    this.isEditingUnit = false;
    this.editingUnitId = null;
    const nextLevel = this.medicine?.units?.length ? Math.max(...this.medicine.units.map(u => u.level)) + 1 : 1;
    this.unitForm.reset({ conversionFactor: 10, level: nextLevel });
    this.unitForm.get('unitId')?.enable(); 
    this.isUnitDrawerOpen = true;
  }

  //Hàm sửa unit 
  editUnit(unit: any) { // unit là MedicineUnitDto
      this.isEditingUnit = true;
      this.editingUnitId = unit.unitId;
      this.unitForm.patchValue({
          unitId: unit.unitId,
          conversionFactor: unit.conversionFactor,
          level: unit.level
      });
      this.unitForm.get('unitId')?.disable(); 
      this.isUnitDrawerOpen = true;
  }

  saveUnit() {
    if (this.unitForm.invalid) return;
    const formValue = this.unitForm.getRawValue();

    if (this.isEditingUnit) {
        this.medicineService.updateUnit(this.id, this.editingUnitId!, formValue).subscribe(() => {
            this.isUnitDrawerOpen = false;
            this.loadData();
        });
    } else {
        this.medicineService.addUnit(this.id, formValue).subscribe(() => {
            this.isUnitDrawerOpen = false;
            this.loadData();
        });
    }
  }

  removeUnit(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
        if (status === Confirmation.Status.confirm) {
            this.medicineService.removeUnit(this.id, id).subscribe(() => this.loadData());
        }
    });
  }

  // =========================================================
  // [MỚI] LOGIC PRICE (QUẢN LÝ GIÁ)
  // =========================================================

  openPriceDrawer() {
      this.isEditingPrice = false;
      this.editingPriceId = null;
      this.priceForm.reset({ price: 0, minQuantity: 1 });
      
      this.priceForm.get('priceListId')?.enable();
      this.priceForm.get('unitId')?.enable();
      
      this.isPriceDrawerOpen = true;
  }

  editPrice(price: ProductPriceDto) {
      this.isEditingPrice = true;
      this.editingPriceId = price.id;
      
      this.priceForm.patchValue({
          priceListId: price.priceListId,
          unitId: price.unitId,
          price: price.price,
          minQuantity: price.minQuantity
      });

      this.priceForm.get('priceListId')?.disable();
      this.priceForm.get('unitId')?.disable();

      this.isPriceDrawerOpen = true;
  }

  savePrice() {
      if (this.priceForm.invalid) return;
      const formValue = this.priceForm.getRawValue();

      if (this.isEditingPrice) {
          // Update
          this.priceService.update(this.editingPriceId!, formValue).subscribe(() => {
              this.isPriceDrawerOpen = false;
              this.loadPrices();
          });
      } else {
          // Create (Cần thêm ProductId vào DTO)
          const createDto = { ...formValue, productId: this.id };
          this.priceService.create(createDto).subscribe(() => {
              this.isPriceDrawerOpen = false;
              this.loadPrices();
          });
      }
  }

  removePrice(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
        if (status === Confirmation.Status.confirm) {
            this.priceService.delete(id).subscribe(() => this.loadPrices());
        }
    });
  }

  getEnumName(enumObj: any, value: number): string {
    return enumObj[value];
  }

}