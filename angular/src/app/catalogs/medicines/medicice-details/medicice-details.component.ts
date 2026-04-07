import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { MedicineService } from 'src/app/proxy/medicines';
import { ActiveIngredientService } from 'src/app/proxy/active-ingredients';
import { BaseUnitService } from 'src/app/proxy/base-units';
import { CreateUpdateMedicineIngredientDto, CreateUpdateMedicineUnitDto, MedicineDetailDto, MedicineUnitDto } from 'src/app/proxy/medicines/dtos';
import { ProductPriceDto, PriceListDto, CreateUpdateProductPriceDto } from 'src/app/proxy/prices/dtos'; 
import { UsageRoute, StorageCondition } from 'src/app/proxy/enums/medicines';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { PriceService } from 'src/app/proxy/prices';
import { CurrencyFormatDirective } from 'src/app/shared/directives/currency-format.directive';
import { CurrencyType } from 'src/app/proxy/enums';
import { enumName } from 'src/app/shared/utils/enum.util';

@Component({
  selector: 'app-medicine-detail',
  standalone: true,
  imports: [SharedModule, DrawerComponent, CurrencyFormatDirective],
  templateUrl: 'medicice-details.component.html',
  styleUrl: 'medicice-details.component.scss',
})
export class MedicineDetailComponent {
  
  //Modal State
  isVisible = false;
  id = '';
  medicine: MedicineDetailDto;
  
  //List này gộp BaseUnit + Các Unit quy đổi -> Dùng để chọn khi set giá
  availableUnitsForPrice: any[] = []; 
  
  //List price of medicine
  productPrices: ProductPriceDto[] = []; 
  
  //List price
  priceLists: PriceListDto[] = [];

  //Drawer state
  isIngrDrawerOpen = false;
  isUnitDrawerOpen = false;
  isPriceDrawerOpen = false; 

  //Form
  ingrForm: FormGroup;
  unitForm: FormGroup;
  priceForm: FormGroup; 

  //Dropdown data (Ingredient/Unit)
  allIngredients: any[] = [];
  allUnits: any[] = []; 

  //Edit unit State
  isEditingUnit = false;
  editingUnitId: string | null = null;
  
  //Edit price state
  isEditingPrice = false; 
  editingPriceId: string | null = null;

  //Enum
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

  readonly enumName = enumName;

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
      this.prepareUnitsForPrice(); 
      this.loadLookups(res); 
      this.loadPrices(); 
    });
  }

  prepareUnitsForPrice() {
    if (!this.medicine) return;
    
    this.availableUnitsForPrice = [
        //Đơn vị cơ bản
        { id: this.medicine.baseUnitId, name: this.medicine.baseUnitName + ' (Base)' },
        //Các đơn vị quy đổi
        ...this.medicine.units.map(u => ({ id: u.unitId, name: u.unitName }))
    ];
  }

  loadPrices() {
    this.priceService.getByProduct(this.id).subscribe(res => {
        this.productPrices = res;
    });
  }

  loadLookups(medicineData: MedicineDetailDto) {
    forkJoin({
      ingrs: this.ingredientService.getList({ maxResultCount: 1000 }),
      units: this.unitService.getList({ maxResultCount: 1000 }),
      priceLists: this.priceService.getPriceLists() 
    }).subscribe(res => {
      //Logic lọc Ingredient
      this.allIngredients = res.ingrs.items.filter(ingr => 
          !medicineData.ingredients.some(existing => existing.activeIngredientId === ingr.id)
      );

      //Logic lọc Unit
      const fullUnits = res.units.items;
      this.allUnits = fullUnits.filter(u => 
          u.id !== medicineData.baseUnitId && 
          !medicineData.units.some(existing => existing.unitId === u.id)
      );
      //Nếu đang Edit Unit, push lại unit đó vào list để hiện tên
      if (this.isEditingUnit && this.editingUnitId) {
         const currentUnit = fullUnits.find(u => u.id === this.editingUnitId);
         if (currentUnit && !this.allUnits.find(u => u.id === currentUnit.id)) {
             this.allUnits.push(currentUnit);
         }
      }

      //Lưu danh sách bảng giá
      this.priceLists = res.priceLists;
    });
  }

  initForms() {
    //Ingredients Form
    this.ingrForm = this.fb.group({
      activeIngredientId: [null, Validators.required]
    });

    //Units Form
    this.unitForm = this.fb.group({
      unitId: [null, Validators.required],
      conversionFactor: [null, [Validators.required, Validators.min(2)]],
      level: [null, [Validators.required, Validators.min(1)]]
    });

    //Price Form
    this.priceForm = this.fb.group({
        priceListId: [null, Validators.required],
        unitId: [null, Validators.required],
        price: [0, [Validators.required, Validators.min(0)]],
        minQuantity: [1, [Validators.required, Validators.min(1)]]
    });
  }

  //Ingredient
  openIngrDrawer() { this.ingrForm.reset(); this.isIngrDrawerOpen = true; }
  
  saveIngr() {
    if (this.ingrForm.invalid) return;
    const payload = this.ingrForm.getRawValue() as CreateUpdateMedicineIngredientDto
    this.medicineService.addIngredient(this.id, payload).subscribe(() => {
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

  // Unit
  openUnitDrawer() {
    this.isEditingUnit = false;
    this.editingUnitId = null;
    const nextLevel = this.medicine?.units?.length ? Math.max(...this.medicine.units.map(u => u.level)) + 1 : 1;
    this.unitForm.reset({ conversionFactor: 10, level: nextLevel });
    this.unitForm.get('unitId')?.enable(); 
    this.isUnitDrawerOpen = true;
  }

  editUnit(unit: MedicineUnitDto) { 
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
    const payload = this.unitForm.getRawValue() as CreateUpdateMedicineUnitDto

    if (this.isEditingUnit) {
        this.medicineService.updateUnit(this.id, this.editingUnitId!, payload).subscribe(() => {
            this.isUnitDrawerOpen = false;
            this.loadData();
        });
    } else {
        this.medicineService.addUnit(this.id, payload).subscribe(() => {
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

  //Price
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
      const payload = this.priceForm.getRawValue() as CreateUpdateProductPriceDto

      if (this.isEditingPrice) {
          // Update
          this.priceService.update(this.editingPriceId!, payload).subscribe(() => {
              this.isPriceDrawerOpen = false;
              this.loadPrices();
          });
      } else {
          // Create (Cần thêm ProductId vào DTO)
          const createDto = { ...payload, productId: this.id };
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
}