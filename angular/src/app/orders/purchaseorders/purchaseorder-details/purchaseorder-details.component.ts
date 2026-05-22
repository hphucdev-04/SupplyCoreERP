import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import {
  PurchaseOrderDto,
  PurchaseOrderLineDto,
  RelatedTicketDto,
} from 'src/app/proxy/purchase-orders/dtos';
import { PurchaseOrderService } from 'src/app/proxy/purchase-orders';
import { SupplierService } from 'src/app/proxy/suppliers';
import { SupplierProductConditionDto } from 'src/app/proxy/suppliers/dtos';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { MedicineService } from 'src/app/proxy/medicines';
import { MedicineDto } from 'src/app/proxy/medicines/dtos';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { PurchaseOrderStatus } from 'src/app/proxy/enums/orders/purchase-order-status.enum';
import { enumName } from 'src/app/shared/untils/enum.util';
import { UnitConversionHelper } from 'src/app/shared/untils/unit-conversion.helper';

interface ProductUnitLookup {
  unitId: string;
  unitName: string;
  conversionFactor: number;
  isBaseUnit: boolean;
}

@Component({
  selector: 'app-purchase-order-details',
  standalone: true,
  imports: [SharedModule, DrawerComponent],
  templateUrl: './purchaseorder-details.component.html',
})
export class PurchaseOrderDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private readonly ROUTE_NAME = '::Menu:PurchaseOrderDetails';

  orderId: string;
  order: PurchaseOrderDto;
  relatedTickets: RelatedTicketDto[] = [];
  warehouses: WarehouseDto[] = [];
  medicines: MedicineDto[] = [];
  loading = true;

  // Edit master drawer
  isEditDrawerOpen = false;
  editForm: FormGroup;
  isSavingEdit = false;

  // Add line drawer
  isAddLineOpen = false;
  detailForm: FormGroup;
  isSavingDetail = false;
  units: ProductUnitLookup[] = [];
  selectedConversionFactor = 1;
  baseUnitName = '';
  quantityPreview = 0;
  isAutoFilled = false;
  activeConditions: SupplierProductConditionDto[] = [];

  PurchaseOrderStatus = PurchaseOrderStatus;
  readonly enumName = enumName;

  constructor(
    private poService: PurchaseOrderService,
    private supplierService: SupplierService,
    private warehouseService: WarehouseService,
    private medicineService: MedicineService,
    private routesService: RoutesService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.orderId = this.route.snapshot.params['id'];
    if (this.orderId) {
      this.buildForms();
      this.loadData();
      this.loadMasterData();
    } else {
      this.goBack();
    }
  }

  ngOnInit(): void {
    this.orderId = this.route.snapshot.params['id'];
    if (this.orderId) {
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
    this.router.navigate(['/order/purchaseorders']);
  }

  // ── Data ─────────────────────────────────────────────────
  loadData() {
    this.loading = true;
    this.poService
      .get(this.orderId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.order = res;
          this.relatedTickets = res.relatedTickets || [];
          this.loading = false;
          this.loadMasterData();

          this.routesService.add([
            {
              path: `/order/purchaseorders/details/${this.order.id}`,
              name: this.ROUTE_NAME,
              parentName: '::Menu:PurchaseOrders',
              iconClass: 'fas fa-file-invoice',
              layout: eLayoutType.application,
              requiredPolicy: 'Order.PurchaseOrder',
            },
          ]);
        },
        error: () => this.goBack(),
      });


    this.ticketService
      .getRelatedTicketsByPurchaseOrder(this.orderId)
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => (this.relatedTickets = res));
  }

  loadMasterData() {
    this.warehouseService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.warehouses = res.items));

    if (this.order?.supplierId) {
      this.supplierService
        .getProductList(this.order.supplierId, {
          maxResultCount: 1000,
          skipCount: 0,
          isActive: true,
        } as any)
        .pipe(takeUntil(this.destroy$))
        .subscribe(res => {
          this.medicines = res.items.map(
            sp =>
              ({
                id: sp.productId,
                code: sp.productCode,
                name: sp.productName,
              }) as MedicineDto,
          );
        });
    }
  }

  // ── Forms ─────────────────────────────────────────────────
  buildForms() {
    this.editForm = this.fb.group({
      warehouseId: [null, [Validators.required]],
      expectedDeliveryDate: [null],
      dueDate: [{ value: null, disabled: false }],
      note: ['', [Validators.maxLength(1000)]],
    });

    this.detailForm = this.fb.group({
      productId: [null, [Validators.required]],
      unitId: [null, [Validators.required]],
      conversionFactor: [1, [Validators.required, Validators.min(1)]],
      quantity: [1, [Validators.required, Validators.min(0.01)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      taxRate: [0, [Validators.min(0)]],
    });
  }

  calculateDueDate() {
    if (!this.order?.supplierId || !this.order?.orderDate) return;

    this.supplierService.get(this.order.supplierId).subscribe(supplier => {
      const days = supplier.paymentTermDays || 0;
      const orderDate = new Date(this.order.orderDate);
      if (days > 0) {
        orderDate.setDate(orderDate.getDate() + days);
        this.editForm.get('dueDate').setValue(orderDate.toISOString().split('T')[0]);
      } else {
        this.editForm.get('dueDate').setValue(this.order.orderDate.split('T')[0]);
      }
    });
  }

  // ── Edit master ───────────────────────────────────────────
  openEditDrawer() {
    this.editForm.patchValue({
      warehouseId: this.order.warehouseId,
      expectedDeliveryDate: this.order.expectedDeliveryDate?.split('T')[0] ?? null,
      dueDate: this.order.dueDate?.split('T')[0] ?? null,
      note: this.order.note ?? '',
    });
    this.isEditDrawerOpen = true;

    // Tự động tính lại khi mở (để đảm bảo đồng bộ nếu cấu hình NCC thay đổi)
    this.calculateDueDate();
  }

  closeEditDrawer() {
    this.isEditDrawerOpen = false;
  }

  saveEdit() {
    if (this.editForm.invalid) return;
    this.isSavingEdit = true;
    this.poService
      .update(this.orderId, this.editForm.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingEdit = false;
          this.closeEditDrawer();
          this.loadData();
        },
        error: () => (this.isSavingEdit = false),
      });
  }

  // ── Add Line ────────────────────────────────────────────
  openAddLineDrawer() {
    this.units = [];
    this.selectedConversionFactor = 1;
    this.quantityPreview = 0;
    this.detailForm.reset({ quantity: 1, conversionFactor: 1, unitPrice: 0, taxRate: 0 });
    this.isAddLineOpen = true;
  }

  closeAddLineDrawer() {
    this.isAddLineOpen = false;
  }

  onMedicineChange(medicineId: string) {
    this.detailForm.patchValue({ unitId: null, conversionFactor: 1, unitPrice: 0 });
    this.units = [];
    this.activeConditions = [];
    this.selectedConversionFactor = 1;
    this.baseUnitName = '';
    this.quantityPreview = 0;
    this.isAutoFilled = false;

    if (!medicineId) return;

    // 1. Load Medicine Units
    this.medicineService
      .get(medicineId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(detail => {
        this.baseUnitName = detail.baseUnitName ?? '';

        const baseUnit: ProductUnitLookup = {
          unitId: detail.baseUnitId,
          unitName: detail.baseUnitName,
          conversionFactor: 1,
          isBaseUnit: true,
        };
        let cumulative = 1;
        const others: ProductUnitLookup[] = (detail.units ?? [])
          .sort((a, b) => (a.level ?? 0) - (b.level ?? 0))
          .map(u => {
            cumulative *= u.conversionFactor ?? 1;
            return {
              unitId: u.unitId,
              unitName: u.unitName,
              conversionFactor: cumulative,
              isBaseUnit: false,
            };
          });
        const medicineUnits = [baseUnit, ...others];

        // 2. Tra cứu Supplier Product để điền tự động
        this.supplierService
          .getProductList(this.order.supplierId, {
            productId: medicineId,
            maxResultCount: 1,
          } as any)
          .subscribe(res => {
            const sp = res.items[0];
            if (sp && sp.isActive && sp.conditions && sp.conditions.length > 0) {
              this.isAutoFilled = true;
              this.activeConditions = sp.conditions;

              // Chỉ nạp các đơn vị đã thỏa thuận giá
              this.units = sp.conditions.map(c => ({
                unitId: c.unitId,
                unitName: c.unitName,
                conversionFactor: c.conversionFactor || 1,
                isBaseUnit: c.unitId === sp.defaultUnitId,
              }));

              // Chọn đơn vị mặc định
              const defaultUnitId = sp.defaultUnitId;
              const matchedCondition =
                sp.conditions.find(c => c.unitId === defaultUnitId) || sp.conditions[0];

              this.detailForm.patchValue({
                unitId: matchedCondition.unitId,
                conversionFactor: matchedCondition.conversionFactor,
                unitPrice:
                  matchedCondition.standardPrice || matchedCondition.lastPurchasePrice || 0,
              });
              this.selectedConversionFactor = matchedCondition.conversionFactor || 1;
            } else {
              // Fallback về toàn bộ đơn vị của thuốc nếu không có config nhà cung cấp
              this.units = medicineUnits;
              this.activeConditions = [];
              this.detailForm.patchValue({ unitId: baseUnit.unitId, conversionFactor: 1 });
              this.selectedConversionFactor = 1;
            }
            this.updateQuantityPreview();
          });
      });
  }

  onUnitChange(unitId: string) {
    const unit = this.units.find(u => u.unitId === unitId);
    if (unit) {
      this.selectedConversionFactor = unit.conversionFactor;

      let unitPrice = this.detailForm.get('unitPrice')?.value || 0;
      if (this.isAutoFilled && this.activeConditions.length > 0) {
        const cond = this.activeConditions.find(c => c.unitId === unitId);
        if (cond) {
          unitPrice = cond.standardPrice || cond.lastPurchasePrice || 0;
        }
      }

      this.detailForm.patchValue({
        conversionFactor: unit.conversionFactor,
        unitPrice: unitPrice,
      });
    }
    this.updateQuantityPreview();
  }

  updateQuantityPreview() {
    const qty = this.detailForm.get('quantity')?.value || 0;
    const unitId = this.detailForm.get('unitId')?.value;
    this.quantityPreview = UnitConversionHelper.convertToBaseQuantity(
      {
        baseUnitId: '',
        units: [{ unitId: unitId, conversionFactor: this.selectedConversionFactor }],
      },
      unitId,
      qty,
    );
  }

  saveLine() {
    if (this.detailForm.invalid) return;
    this.isSavingDetail = true;

    // ✅ Sử dụng getRawValue để lấy cả các trường bị disabled (productId, unitId)
    const payload = this.detailForm.getRawValue();

    this.poService
      .addLine(this.orderId, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingDetail = false;
          this.closeAddLineDrawer();
          this.loadData();
        },
        error: () => (this.isSavingDetail = false),
      });
  }

  // ── Inline edit line quantity ───────────────────────────
  onInlineLineChange(
    line: PurchaseOrderLineDto,
    field: 'quantity' | 'unitPrice' | 'taxRate',
    rawValue: string,
  ) {
    const value = parseFloat(rawValue);
    if (isNaN(value) || (field === 'quantity' && value <= 0)) {
      this.toaster.error('::InvalidValue', '::Error');
      this.loadData();
      return;
    }
    const payload: any = {
      quantity: line.quantity,
      unitPrice: line.unitPrice,
      taxRate: line.taxRate ?? 0,
    };
    payload[field] = value;
    if (payload[field] === line[field]) return;

    this.poService
      .updateLine(this.orderId, line.id, payload)
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
        this.poService
          .removeLine(this.orderId, lineId)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.loadData();
          });
      }
    });
  }

  // ── Workflow actions ──────────────────────────────────────
  sendToApprove() {
    if (!this.order?.lines?.length) {
      this.toaster.error('::NoLinesError', '::Error');
      return;
    }
    this.confirmation.info('::SendToApproveConfirmation', '::Confirm').subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.poService
        .sendToApprove(this.orderId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.loadData();
        });
    });
  }

  approve() {
    this.confirmation.success('::ApproveConfirmation', '::Confirm').subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.poService
        .approve(this.orderId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.toaster.success('::ApproveSuccess', '::Success');
          this.loadData();
        });
    });
  }

  complete() {
    this.confirmation.success('::CompleteConfirmation', '::Confirm').subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.poService
        .complete(this.orderId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.toaster.success('::CompleteSuccess', '::Success');
          this.loadData();
        });
    });
  }

  // ── Helpers ───────────────────────────────────────────────
  isEditable(): boolean {
    return (
      this.order?.status === PurchaseOrderStatus.Draft ||
      this.order?.status === PurchaseOrderStatus.PendingApproval
    );
  }

  receiveProgress(line: PurchaseOrderLineDto): number {
    if (!line.quantity) return 0;
    return Math.min(100, ((line.receivedQuantity ?? 0) / line.quantity) * 100);
  }

  statusClass(status: PurchaseOrderStatus): string {
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

  statusIcon(status: PurchaseOrderStatus): string {
    const map: Record<number, string> = {
      [PurchaseOrderStatus.Draft]: 'fa-pencil',
      [PurchaseOrderStatus.PendingApproval]: 'fa-clock-o',
      [PurchaseOrderStatus.Approved]: 'fa-check',
      [PurchaseOrderStatus.Receiving]: 'fa-truck',
      [PurchaseOrderStatus.Completed]: 'fa-check-circle',
      [PurchaseOrderStatus.Canceled]: 'fa-times-circle',
    };
    return map[status] ?? 'fa-circle';
  }
}
