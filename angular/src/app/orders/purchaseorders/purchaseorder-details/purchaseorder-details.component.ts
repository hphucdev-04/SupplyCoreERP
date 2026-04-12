import { Component, OnDestroy, Output, EventEmitter, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { PurchaseOrderDto, PurchaseOrderDetailDto } from 'src/app/proxy/purchase-orders/dtos';
import { PurchaseOrderService } from 'src/app/proxy/purchase-orders';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { MedicineService } from 'src/app/proxy/medicines';
import { MedicineDto } from 'src/app/proxy/medicines/dtos';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { PurchaseOrderStatus } from 'src/app/proxy/enums/orders/purchase-order-status.enum';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { enumName } from 'src/app/shared/utils/enum.util';

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
export class PurchaseOrderDetailsComponent implements OnDestroy {
  @ViewChild('cancelModal', { static: false }) cancelModal: any;
  @Output() onSaved = new EventEmitter<void>();

  private destroy$ = new Subject<void>();

  isVisible = false;
  orderId: string;
  order: PurchaseOrderDto;
  warehouses: WarehouseDto[] = [];
  medicines: MedicineDto[] = [];

  // Cancel state
  cancelReason = '';
  showCancelError = false;
  isCanceling = false;

  // Edit master drawer
  isEditDrawerOpen = false;
  editForm: FormGroup;
  isSavingEdit = false;

  // Add detail drawer
  isAddDetailOpen = false;
  detailForm: FormGroup;
  isSavingDetail = false;
  units: ProductUnitLookup[] = [];
  selectedConversionFactor = 1;
  baseUnitName = '';
  quantityPreview = 0;

  PurchaseOrderStatus = PurchaseOrderStatus;
  readonly enumName = enumName;

  constructor(
    private poService: PurchaseOrderService,
    private warehouseService: WarehouseService,
    private medicineService: MedicineService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private modalService: NgbModal
  ) {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Public API ────────────────────────────────────────────
  open(orderId: string) {
    this.orderId = orderId;
    this.order = null;
    this.buildForms();
    this.loadData();
    this.loadMasterData();
    this.isVisible = true;
  }

  close() {
    this.isVisible = false;
  }

  // ── Data ─────────────────────────────────────────────────
  loadData() {
    this.poService
      .get(this.orderId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => (this.order = res),
        error: () => this.close(),
      });
  }

  loadMasterData() {
    this.warehouseService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => (this.warehouses = res.items));

    this.medicineService
      .getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => (this.medicines = res.items));
  }

  // ── Forms ─────────────────────────────────────────────────
  buildForms() {
    this.editForm = this.fb.group({
      warehouseId: [null, [Validators.required]],
      expectedDeliveryDate: [null],
      dueDate: [null],
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

  // ── Edit master ───────────────────────────────────────────
  openEditDrawer() {
    this.editForm.patchValue({
      warehouseId: this.order.warehouseId,
      expectedDeliveryDate: this.order.expectedDeliveryDate?.split('T')[0] ?? null,
      dueDate: this.order.dueDate?.split('T')[0] ?? null,
      note: this.order.note ?? '',
    });
    this.isEditDrawerOpen = true;
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
          this.onSaved.emit();
        },
        error: () => (this.isSavingEdit = false),
      });
  }

  // ── Add Detail ────────────────────────────────────────────
  openAddDetailDrawer() {
    this.units = [];
    this.selectedConversionFactor = 1;
    this.quantityPreview = 0;
    this.detailForm.reset({ quantity: 1, conversionFactor: 1, unitPrice: 0, taxRate: 0 });
    this.isAddDetailOpen = true;
  }

  closeAddDetailDrawer() {
    this.isAddDetailOpen = false;
  }

  onMedicineChange(medicineId: string) {
    this.detailForm.patchValue({ unitId: null, conversionFactor: 1 });
    this.units = [];
    this.selectedConversionFactor = 1;
    this.baseUnitName = '';
    this.quantityPreview = 0;
    if (!medicineId) return;

    this.medicineService
      .get(medicineId)
      .pipe(takeUntil(this.destroy$))
      .subscribe((detail) => {
        const baseUnit: ProductUnitLookup = {
          unitId: detail.baseUnitId,
          unitName: detail.baseUnitName,
          conversionFactor: 1,
          isBaseUnit: true,
        };
        let cumulative = 1;
        const others: ProductUnitLookup[] = (detail.units ?? [])
          .sort((a, b) => (a.level ?? 0) - (b.level ?? 0))
          .map((u) => {
            cumulative *= u.conversionFactor ?? 1;
            return { unitId: u.unitId, unitName: u.unitName, conversionFactor: cumulative, isBaseUnit: false };
          });
        this.units = [baseUnit, ...others];
        this.baseUnitName = detail.baseUnitName ?? '';
        this.detailForm.patchValue({ unitId: baseUnit.unitId, conversionFactor: 1 });
        this.selectedConversionFactor = 1;
      });
  }

  onUnitChange(unitId: string) {
    const unit = this.units.find((u) => u.unitId === unitId);
    if (unit) {
      this.selectedConversionFactor = unit.conversionFactor;
      this.detailForm.patchValue({ conversionFactor: unit.conversionFactor });
    }
    this.updateQuantityPreview();
  }

  updateQuantityPreview() {
    const qty = this.detailForm.get('quantity')?.value || 0;
    this.quantityPreview = qty * this.selectedConversionFactor;
  }

  saveDetail() {
    if (this.detailForm.invalid) return;
    this.isSavingDetail = true;
    this.poService
      .addDetail(this.orderId, this.detailForm.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingDetail = false;
          this.closeAddDetailDrawer();
          this.loadData();
          this.onSaved.emit();
        },
        error: () => (this.isSavingDetail = false),
      });
  }

  // ── Inline edit detail quantity ───────────────────────────
  onInlineDetailChange(detail: PurchaseOrderDetailDto, field: 'quantity' | 'unitPrice' | 'taxRate', rawValue: string) {
    const value = parseFloat(rawValue);
    if (isNaN(value) || (field === 'quantity' && value <= 0)) {
      this.toaster.error('::InvalidValue', '::Error');
      this.loadData();
      return;
    }
    const payload: any = {
      quantity: detail.quantity,
      unitPrice: detail.unitPrice,
      taxRate: detail.taxRate ?? 0,
    };
    payload[field] = value;
    if (payload[field] === detail[field]) return;

    this.poService
      .updateDetail(this.orderId, detail.id, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toaster.success('::UpdateSuccess', '::Success');
          this.loadData();
          this.onSaved.emit();
        },
        error: () => this.loadData(),
      });
  }

  removeDetail(detailId: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.poService
          .removeDetail(this.orderId, detailId)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.loadData();
            this.onSaved.emit();
          });
      }
    });
  }

  // ── Workflow actions ──────────────────────────────────────
  sendToApprove() {
    if (!this.order?.details?.length) {
      this.toaster.error('::NoDetailsError', '::Error');
      return;
    }
    this.confirmation.info('::SendToApproveConfirmation', '::Confirm').subscribe((status) => {
      if (status !== Confirmation.Status.confirm) return;
      this.poService
        .sendToApprove(this.orderId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.loadData();
          this.onSaved.emit();
        });
    });
  }

  approve() {
    this.confirmation.success('::ApproveConfirmation', '::Confirm').subscribe((status) => {
      if (status !== Confirmation.Status.confirm) return;
      this.poService
        .approve(this.orderId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.toaster.success('::ApproveSuccess', '::Success');
          this.loadData();
          this.onSaved.emit();
        });
    });
  }

  complete() {
    this.confirmation.success('::CompleteConfirmation', '::Confirm').subscribe((status) => {
      if (status !== Confirmation.Status.confirm) return;
      this.poService
        .complete(this.orderId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.toaster.success('::CompleteSuccess', '::Success');
          this.loadData();
          this.onSaved.emit();
        });
    });
  }

  openCancelModal() {
    this.cancelReason = '';
    this.showCancelError = false;
    this.isCanceling = false;
    this.modalService.open(this.cancelModal, { size: 'md', centered: true, backdrop: 'static' });
  }

  confirmCancel(modal: any) {
    if (!this.cancelReason?.trim()) {
      this.showCancelError = true;
      return;
    }
    this.isCanceling = true;
    this.poService
      .cancel(this.orderId, this.cancelReason.trim())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isCanceling = false;
          modal.close();
          this.toaster.success('::CancelSuccess', '::Success');
          this.loadData();
          this.onSaved.emit();
        },
        error: () => (this.isCanceling = false),
      });
  }

  // ── Helpers ───────────────────────────────────────────────
  isEditable(): boolean {
    return (
      this.order?.status === PurchaseOrderStatus.Draft ||
      this.order?.status === PurchaseOrderStatus.PendingApproval
    );
  }

  isCancelable(): boolean {
    return (
      this.order?.status !== PurchaseOrderStatus.Completed &&
      this.order?.status !== PurchaseOrderStatus.Canceled &&
      this.order?.status !== PurchaseOrderStatus.Receiving
    );
  }

  receiveProgress(detail: PurchaseOrderDetailDto): number {
    if (!detail.quantity) return 0;
    return Math.min(100, ((detail.receivedQuantity ?? 0) / detail.quantity) * 100);
  }

  statusClass(status: PurchaseOrderStatus): string {
    const map: Record<number, string> = {
      [PurchaseOrderStatus.Draft]: 'badge-secondary',
      [PurchaseOrderStatus.PendingApproval]: 'badge-warning',
      [PurchaseOrderStatus.Approved]: 'badge-info',
      [PurchaseOrderStatus.Receiving]: 'badge-primary',
      [PurchaseOrderStatus.Completed]: 'badge-success',
      [PurchaseOrderStatus.Canceled]: 'badge-danger',
    };
    return map[status] ?? 'badge-secondary';
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