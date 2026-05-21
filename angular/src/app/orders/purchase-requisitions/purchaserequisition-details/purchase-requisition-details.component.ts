import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { Subject, forkJoin, of } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import {
  PurchaseRequisitionDto,
  PurchaseRequisitionLineDto,
} from 'src/app/proxy/purchase-requisitions/dtos';
import { PurchaseRequisitionService } from 'src/app/proxy/purchase-requisitions';
import { MedicineService } from 'src/app/proxy/medicines';
import { MedicineDto } from 'src/app/proxy/medicines/dtos';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { SupplierService } from 'src/app/proxy/suppliers';
import { SupplierDto } from 'src/app/proxy/suppliers/dtos';
import { PurchaseRequisitionStatus } from 'src/app/proxy/enums/orders/purchase-requisition-status.enum';
import { PurchaseOrderStatus } from 'src/app/proxy/enums/orders/purchase-order-status.enum';
import { enumName } from 'src/app/shared/untils/enum.util';
import { DropdownSearchComponent } from 'src/app/shared/components/dropdownsearch-component/dropdown-search.component';

interface ProductUnitLookup {
  unitId: string;
  unitName: string;
  conversionFactor: number;
}

@Component({
  selector: 'app-purchase-requisition-details',
  standalone: true,
  imports: [SharedModule, DrawerComponent, DropdownSearchComponent],
  templateUrl: './purchase-requisition-details.component.html',
})
export class PurchaseRequisitionDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private readonly ROUTE_NAME = '::Menu:PurchaseRequisitionDetails';

  id: string;
  requisition: PurchaseRequisitionDto;
  medicines: MedicineDto[] = [];
  warehouses: WarehouseDto[] = [];
  suppliers: SupplierDto[] = [];
  loading = true;

  // Edit header drawer
  isEditDrawerOpen = false;
  editForm: FormGroup;
  isSavingEdit = false;

  // Add line drawer
  isAddLineOpen = false;
  lineForm: FormGroup;
  isSavingLine = false;
  units: ProductUnitLookup[] = [];

  // Convert to PO Drawer (Bottom)
  isConvertDrawerOpen = false;
  convertForm: FormGroup;
  isSavingConvert = false;
  suppliersPerLine: Record<string, { id: string; name: string }[]> = {};

  PurchaseRequisitionStatus = PurchaseRequisitionStatus;
  PurchaseOrderStatus = PurchaseOrderStatus;
  readonly enumName = enumName;

  constructor(
    private prService: PurchaseRequisitionService,
    private medicineService: MedicineService,
    private warehouseService: WarehouseService,
    private supplierService: SupplierService,
    private routesService: RoutesService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.id = this.route.snapshot.params['id'];
    if (this.id) {
      this.buildForms();
      this.loadData();
      this.loadMasterData();
    } else {
      this.goBack();
    }
  }

  ngOnDestroy(): void {
    this.routesService.remove([this.ROUTE_NAME]);
    this.destroy$.next();
    this.destroy$.complete();
  }

  goBack() {
    this.router.navigate(['/order/purchaserequisitions']);
  }

  goToOrder(id: string) {
    this.router.navigate(['/order/purchaseorders/details', id]);
  }

  loadData() {
    this.loading = true;
    this.prService
      .get(this.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.requisition = res;
          this.loading = false;

          this.routesService.add([
            {
              path: `/order/purchaserequisitions/details/${this.requisition.id}`,
              name: this.ROUTE_NAME,
              parentName: '::Menu:PurchaseRequisitions',
              iconClass: 'fas fa-file-invoice',
              layout: eLayoutType.application,
              requiredPolicy: 'Order.PurchaseRequisition',
            },
          ]);
        },
        error: () => this.goBack(),
      });
  }

  loadMasterData() {
    this.medicineService
      .getList({ maxResultCount: 1000, skipCount: 0, isActive: true })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.medicines = res.items));

    this.warehouseService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.warehouses = res.items));

    this.supplierService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.suppliers = res.items));
  }

  buildForms() {
    // Header Edit Form
    this.editForm = this.fb.group({
      warehouseId: [null, [Validators.required]],
      requiredDate: [null],
      note: ['', [Validators.maxLength(1000)]],
    });

    // Add Line Form
    this.lineForm = this.fb.group({
      productId: [null, [Validators.required]],
      unitId: [null, [Validators.required]],
      quantity: [1, [Validators.required, Validators.min(0.001)]],
      note: ['', [Validators.maxLength(500)]],
    });

    // Convert to PO Form
    this.convertForm = this.fb.group({
      orderDate: [new Date().toISOString().split('T')[0], [Validators.required]],
      note: [''],
      allocations: this.fb.array([]),
    });
  }

  // ── Header Edit ──────────────────────────────────────────
  openEditDrawer() {
    this.editForm.patchValue({
      warehouseId: this.requisition.warehouseId,
      requiredDate: this.requisition.requiredDate?.split('T')[0] ?? null,
      note: this.requisition.note ?? '',
    });
    this.isEditDrawerOpen = true;
  }

  closeEditDrawer() {
    this.isEditDrawerOpen = false;
  }

  saveEdit() {
    if (this.editForm.invalid) return;
    this.isSavingEdit = true;
    this.prService
      .update(this.id, this.editForm.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingEdit = false;
          this.closeEditDrawer();
          this.loadData();
          this.toaster.success('::UpdateSuccess', '::Success');
        },
        error: () => (this.isSavingEdit = false),
      });
  }

  // ── Line Management ─────────────────────────────────────
  openAddLineDrawer() {
    this.units = [];
    this.lineForm.reset({ quantity: 1, note: '' });
    this.isAddLineOpen = true;
  }

  closeAddLineDrawer() {
    this.isAddLineOpen = false;
  }

  onMedicineChange(medicineId: string) {
    this.lineForm.patchValue({ unitId: null });
    this.units = [];
    if (!medicineId) return;

    this.medicineService.get(medicineId).subscribe(detail => {
      const baseUnit = {
        unitId: detail.baseUnitId,
        unitName: detail.baseUnitName,
        conversionFactor: 1,
      };
      let cumulative = 1;
      const others = (detail.units ?? [])
        .sort((a, b) => (a.level ?? 0) - (b.level ?? 0))
        .map(u => {
          cumulative *= u.conversionFactor ?? 1;
          return { unitId: u.unitId, unitName: u.unitName, conversionFactor: cumulative };
        });
      this.units = [baseUnit, ...others];

      // Auto select base unit
      this.lineForm.patchValue({ unitId: detail.baseUnitId });
    });
  }

  saveLine() {
    if (this.lineForm.invalid) return;
    this.isSavingLine = true;
    this.prService
      .addLine(this.id, this.lineForm.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingLine = false;
          this.closeAddLineDrawer();
          this.loadData();
          this.toaster.success('::AddSuccess', '::Success');
        },
        error: () => (this.isSavingLine = false),
      });
  }

  onInlineLineChange(
    line: PurchaseRequisitionLineDto,
    field: 'quantity' | 'note',
    rawValue: string,
  ) {
    const value = field === 'quantity' ? parseFloat(rawValue) : rawValue;
    if (field === 'quantity' && (isNaN(value as number) || (value as number) <= 0)) {
      this.toaster.error('::InvalidValue', '::Error');
      this.loadData();
      return;
    }

    const payload: any = {
      quantity: line.quantity,
      note: line.note,
    };
    payload[field] = value;

    if (payload[field] === line[field]) return;

    this.prService
      .updateLine(this.id, line.id, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toaster.success('::UpdateSuccess', '::Success');
          this.loadData();
        },
        error: () => this.loadData(),
      });
  }

  removeLine(lineId: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.prService
          .removeLine(this.id, lineId)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.loadData();
            this.toaster.success('::DeleteSuccess', '::Success');
          });
      }
    });
  }

  // ── Workflow ───────────────────────────────────────────
  sendToApprove() {
    if (!this.requisition?.lines?.length) {
      this.toaster.error('::NoLinesError', '::Error');
      return;
    }
    this.confirmation.info('::SendToApproveConfirmation', '::Confirm').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.prService.sendToApprove(this.id).subscribe(() => this.loadData());
      }
    });
  }

  approve() {
    this.confirmation.success('::ApproveConfirmation', '::Confirm').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.prService.approve(this.id).subscribe(() => this.loadData());
      }
    });
  }

  reject() {
    this.confirmation.warn('::RejectConfirmation', '::Confirm').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.prService.reject(this.id).subscribe(() => this.loadData());
      }
    });
  }

  // ── Convert to PO (Bottom Drawer) ──────────────────────
  get allocations(): FormArray {
    return this.convertForm.get('allocations') as FormArray;
  }

  openConvertDrawer() {
    const arr = this.allocations;
    arr.clear();
    this.suppliersPerLine = {};

    this.convertForm.patchValue({ orderDate: new Date().toISOString().split('T')[0], note: '' });

    // 1. Batch Load: Lấy danh sách ProductIds duy nhất cần đặt hàng
    const productIds = [
      ...new Set(
        this.requisition.lines
          .filter(line => line.orderedQuantity < line.quantity)
          .map(line => line.productId),
      ),
    ];

    if (productIds.length === 0) {
      this.toaster.info('::AllProductsAlreadyOrdered', '::Info');
      return;
    }

    // 2. Tối ưu nạp dữ liệu: Gọi nạp Suppliers & Scores cho tất cả sản phẩm trong 1 lần mở
    const supplierRequests = productIds.map(pid =>
      this.supplierService
        .getSupplierList(pid, { maxResultCount: 100, skipCount: 0 })
        .pipe(catchError(() => of({ items: [] }))),
    );

    const sourcingRequests = this.supplierService
      .getSourcingSuggestions(productIds)
      .pipe(catchError(() => of([])));

    forkJoin([forkJoin(supplierRequests), sourcingRequests]).subscribe(
      ([supplierResults, suggestions]) => {
        // Lưu vào map để tái sử dụng
        productIds.forEach((pid, index) => {
          const productSuggestions = suggestions.filter(s => s.productId === pid);

          this.suppliersPerLine[pid] = supplierResults[index].items.map(s => {
            const sug = productSuggestions.find(ps => ps.supplierId === s.supplierId);
            const scoreText = sug ? ` - [Score: ${sug.score}]` : '';
            return {
              id: s.supplierId,
              name: `${s.supplierName}${scoreText}${s.isPreferred ? ' (Ưu tiên)' : ''}`,
            };
          });
        });

        // 3. Khởi tạo FormArray
        this.requisition.lines.forEach(line => {
          const remaining = line.quantity - line.orderedQuantity;
          if (remaining > 0) {
            const group = this.fb.group({
              selected: [true],
              requisitionLineId: [line.id],
              productName: [line.productName],
              productId: [line.productId],
              unitName: [line.unitName],
              remainingQty: [remaining],
              quantity: [
                remaining,
                [Validators.required, Validators.min(0.001), Validators.max(remaining)],
              ],
              supplierId: [null, [Validators.required]],
              warehouseId: [this.requisition.warehouseId],
            });

            group.get('selected').valueChanges.subscribe(selected => {
              const sCtrl = group.get('supplierId');
              const qCtrl = group.get('quantity');
              if (selected) {
                sCtrl.enable();
                qCtrl.enable();
              } else {
                sCtrl.disable();
                qCtrl.disable();
              }
            });

            arr.push(group);
          }
        });

        this.isConvertDrawerOpen = true;
      },
    );
  }

  autoFillBestSuppliers() {
    const selectedControls = this.allocations.controls.filter(c => c.get('selected').value);
    const productIds = selectedControls.map(c => c.get('productId').value);

    if (productIds.length === 0) return;

    this.supplierService.getSourcingSuggestions(productIds).subscribe(suggestions => {
      selectedControls.forEach(control => {
        const productId = control.get('productId').value;
        // The first one in sorted suggestions for this product is the best
        const productSuggestions = suggestions.filter(s => s.productId === productId);
        const bestSuggestion = productSuggestions[0];
        if (bestSuggestion) {
          control.get('supplierId').setValue(bestSuggestion.supplierId);
          (control as any)._sourcingScore = bestSuggestion.score;
        }
      });
      this.toaster.success('::BestSuppliersApplied', '::Success');
    });
  }

  saveConvert() {
    if (this.convertForm.invalid) {
      this.convertForm.markAllAsTouched();
      this.toaster.error('::PleaseSelectSupplierAndValidQuantityForAllLines', '::Error');
      return;
    }

    const selectedAllocations = this.allocations.getRawValue().filter(x => x.selected);
    if (selectedAllocations.length === 0) {
      this.toaster.error('::SelectAtLeastOneProduct', '::Error');
      return;
    }

    this.isSavingConvert = true;
    const payload = {
      orderDate: this.convertForm.value.orderDate,
      note: this.convertForm.value.note,
      allocations: selectedAllocations.map(x => ({
        requisitionLineId: x.requisitionLineId,
        supplierId: x.supplierId,
        warehouseId: x.warehouseId,
        quantity: x.quantity,
      })),
    };

    this.prService
      .convertToPurchaseOrder(this.id, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingConvert = false;
          this.isConvertDrawerOpen = false;
          this.toaster.success('::ConvertToPOSuccess', '::Success');
          this.loadData();
        },
        error: () => (this.isSavingConvert = false),
      });
  }

  // ── Helpers ────────────────────────────────────────────
  isEditable(): boolean {
    return (
      this.requisition?.status === PurchaseRequisitionStatus.Draft ||
      this.requisition?.status === PurchaseRequisitionStatus.Rejected
    );
  }

  statusClass(status: PurchaseRequisitionStatus): string {
    const map: Record<number, string> = {
      [PurchaseRequisitionStatus.Draft]: 'ph-badge--neutral',
      [PurchaseRequisitionStatus.PendingApproval]: 'ph-badge--pending',
      [PurchaseRequisitionStatus.Approved]: 'ph-badge--info',
      [PurchaseRequisitionStatus.Rejected]: 'ph-badge--rejected',
      [PurchaseRequisitionStatus.PartialOrdered]: 'ph-badge--primary',
      [PurchaseRequisitionStatus.Ordered]: 'ph-badge--approved',
      [PurchaseRequisitionStatus.Canceled]: 'ph-badge--rejected',
    };
    return map[status] ?? 'ph-badge--neutral';
  }

  statusIcon(status: PurchaseRequisitionStatus): string {
    const map: Record<number, string> = {
      [PurchaseRequisitionStatus.Draft]: 'fa-pencil',
      [PurchaseRequisitionStatus.PendingApproval]: 'fa-clock-o',
      [PurchaseRequisitionStatus.Approved]: 'fa-check',
      [PurchaseRequisitionStatus.Rejected]: 'fa-times-circle',
      [PurchaseRequisitionStatus.PartialOrdered]: 'fa-hourglass-half',
      [PurchaseRequisitionStatus.Ordered]: 'fa-check-circle',
      [PurchaseRequisitionStatus.Canceled]: 'fa-ban',
    };
    return map[status] ?? 'fa-circle';
  }

  poStatusClass(status: PurchaseOrderStatus): string {
    const map: Record<number, string> = {
      [PurchaseOrderStatus.Draft]: 'ph-badge--neutral',
      [PurchaseOrderStatus.PendingApproval]: 'ph-badge--pending',
      [PurchaseOrderStatus.Approved]: 'ph-badge--info',
      [PurchaseOrderStatus.Receiving]: 'ph-badge--primary',
      [PurchaseOrderStatus.Completed]: 'ph-badge--approved',
      [PurchaseOrderStatus.Canceled]: 'ph-badge--rejected',
    };
    return map[status] ?? 'ph-badge--neutral';
  }
}
