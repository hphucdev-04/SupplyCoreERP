import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { DropdownSearchComponent } from 'src/app/shared/components/dropdownsearch-component/dropdown-search.component';
import { SalesOrderDto, SalesOrderLineDto } from 'src/app/proxy/sales-orders/dtos';
import { SalesOrderService } from 'src/app/proxy/sales-orders';
import { InventoryTicketService } from 'src/app/proxy/tickets';
import { InventoryTicketDto } from 'src/app/proxy/tickets/dtos';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { MedicineService } from 'src/app/proxy/medicines';
import { PriceService } from 'src/app/proxy/prices';
import { MedicineDto } from 'src/app/proxy/medicines/dtos';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { ProductPriceDto } from 'src/app/proxy/prices/dtos';
import { SalesOrderStatus } from 'src/app/proxy/enums/orders/sales-order-status.enum';
import { enumName } from 'src/app/shared/untils/enum.util';

interface ProductUnitLookup {
  unitId: string;
  unitName: string;
  conversionFactor: number;
  isBaseUnit: boolean;
}

@Component({
  selector: 'app-sales-order-details',
  standalone: true,
  imports: [SharedModule, DrawerComponent, DropdownSearchComponent],
  templateUrl: './saleorder-details.component.html',
})
export class SaleOrderDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private readonly ROUTE_NAME = '::Menu:SaleOrderDetails';

  orderId: string;
  order: SalesOrderDto;
  relatedTickets: InventoryTicketDto[] = [];
  warehouses: WarehouseDto[] = [];
  medicines: MedicineDto[] = [];
  loading = true;

  // Cancel state (Drawer)
  isCancelDrawerOpen = false;
  cancelReason = '';
  showCancelError = false;
  isCanceling = false;

  // Edit master drawer
  isEditDrawerOpen = false;
  editForm: FormGroup;
  isSavingEdit = false;

  // Add line drawer
  isAddLineOpen = false;
  lineForm: FormGroup;
  isSavingLine = false;
  units: ProductUnitLookup[] = [];
  availablePrices: ProductPriceDto[] = [];
  selectedConversionFactor = 1;
  baseUnitName = '';
  quantityPreview = 0;

  SalesOrderStatus = SalesOrderStatus;
  readonly enumName = enumName;

  constructor(
    private soService: SalesOrderService,
    private ticketService: InventoryTicketService,
    private warehouseService: WarehouseService,
    private medicineService: MedicineService,
    private priceService: PriceService,
    private routesService: RoutesService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router
  ) { }

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
    this.router.navigate(['/order/saleorders']);
  }

  // ── Data ─────────────────────────────────────────────────
  loadData() {
    this.loading = true;
    this.soService
      .get(this.orderId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.order = res;
          this.loading = false;

          this.routesService.add([{
            path: `/order/saleorders/details/${this.order.id}`,
            name: this.ROUTE_NAME,
            parentName: '::Menu:SalesOrders',
            iconClass: 'fas fa-file-invoice',
            layout: eLayoutType.application,
          }]);
        },
        error: () => this.goBack(),
      });

    this.ticketService
      .getRelatedTicketsBySaleOrder(this.orderId)
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => (this.relatedTickets = res));
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

    this.lineForm = this.fb.group({
      productId: [null, [Validators.required]],
      unitId: [null, [Validators.required]],
      conversionFactor: [1, [Validators.required, Validators.min(1)]],
      quantity: [1, [Validators.required, Validators.min(0.01)]],
      unitPrice: [null],
      discountRate: [0, [Validators.min(0), Validators.max(100)]],
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
    this.soService
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
    this.availablePrices = [];
    this.selectedConversionFactor = 1;
    this.quantityPreview = 0;
    this.lineForm.reset({ quantity: 1, conversionFactor: 1, discountRate: 0, taxRate: 0 });
    this.isAddLineOpen = true;
  }

  closeAddLineDrawer() {
    this.isAddLineOpen = false;
  }

  onMedicineChange(medicineId: string) {
    this.lineForm.patchValue({ unitId: null, conversionFactor: 1, unitPrice: null });
    this.units = [];
    this.availablePrices = [];
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
        this.lineForm.patchValue({ unitId: baseUnit.unitId, conversionFactor: 1 });
        this.selectedConversionFactor = 1;

        this.loadPrices(medicineId);
      });
  }

  loadPrices(productId: string) {
    this.priceService.getByProduct(productId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(prices => {
        this.availablePrices = prices;
        this.filterAvailablePrices();
      });
  }

  onUnitChange(unitId: string) {
    const unit = this.units.find((u) => u.unitId === unitId);
    if (unit) {
      this.selectedConversionFactor = unit.conversionFactor;
      this.lineForm.patchValue({ conversionFactor: unit.conversionFactor });
    }
    this.filterAvailablePrices();
    this.updateQuantityPreview();
  }

  filterAvailablePrices() {
    const unitId = this.lineForm.get('unitId')?.value;
    const qty = this.lineForm.get('quantity')?.value || 0;
    const filtered = this.availablePrices.filter(p => p.unitId === unitId && qty >= (p.minQuantity || 0));
    if (filtered.length > 0) {
      const best = filtered.sort((a, b) => (b.minQuantity || 0) - (a.minQuantity || 0))[0];
      this.lineForm.patchValue({ unitPrice: best.price });
    } else {
      this.lineForm.patchValue({ unitPrice: null });
    }
  }

  updateQuantityPreview() {
    const qty = this.lineForm.get('quantity')?.value || 0;
    this.quantityPreview = qty * this.selectedConversionFactor;
    this.filterAvailablePrices();
  }

  saveLine() {
    if (this.lineForm.invalid) return;
    this.isSavingLine = true;
    this.soService
      .addLine(this.orderId, this.lineForm.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingLine = false;
          this.closeAddLineDrawer();
          this.loadData();
        },
        error: () => (this.isSavingLine = false),
      });
  }

  // ── Inline edit ──
  onInlineLineChange(line: SalesOrderLineDto, field: 'quantity' | 'unitPrice' | 'discountRate' | 'taxRate', rawValue: string) {
    const value = parseFloat(rawValue);
    if (isNaN(value) || value < 0) {
      this.toaster.error('::InvalidValue', '::Error');
      this.loadData();
      return;
    }
    if (value === line[field]) return;

    const payload: any = {
      quantity: line.quantity,
      unitPrice: line.unitPrice,
      discountRate: line.discountRate ?? 0,
      taxRate: line.taxRate ?? 0,
    };
    payload[field] = value;

    this.soService
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
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.soService
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
    this.confirmation.info('::SendToApproveConfirmation', '::Confirm').subscribe((status) => {
      if (status !== Confirmation.Status.confirm) return;
      this.soService
        .sendToApprove(this.orderId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.loadData();
        });
    });
  }

  approve() {
    this.confirmation.success('::ApproveSOConfirmation', '::Confirm').subscribe((status) => {
      if (status !== Confirmation.Status.confirm) return;
      this.soService
        .approve(this.orderId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.toaster.success('::ApproveSuccess', '::Success');
          this.loadData();
        });
    });
  }

  complete() {
    this.confirmation.success('::CompleteConfirmation', '::Confirm').subscribe((status) => {
      if (status !== Confirmation.Status.confirm) return;
      this.soService
        .complete(this.orderId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.toaster.success('::CompleteSuccess', '::Success');
          this.loadData();
        });
    });
  }

  openCancelModal() {
    this.cancelReason = '';
    this.showCancelError = false;
    this.isCanceling = false;
    this.isCancelDrawerOpen = true;
  }

  closeCancelDrawer() {
    this.isCancelDrawerOpen = false;
  }

  confirmCancel() {
    if (!this.cancelReason?.trim()) {
      this.showCancelError = true;
      return;
    }
    this.isCanceling = true;
    this.soService
      .cancel(this.orderId, this.cancelReason.trim())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isCanceling = false;
          this.closeCancelDrawer();
          this.toaster.success('::CancelSuccess', '::Success');
          this.loadData();
        },
        error: () => (this.isCanceling = false),
      });
  }

  // ── Helpers ───────────────────────────────────────────────
  isEditable(): boolean {
    return (
      this.order?.status === SalesOrderStatus.Draft ||
      this.order?.status === SalesOrderStatus.PendingApproval
    );
  }

  isCancelable(): boolean {
    return (
      this.order?.status !== SalesOrderStatus.Completed &&
      this.order?.status !== SalesOrderStatus.Canceled &&
      this.order?.status !== SalesOrderStatus.Delivering
    );
  }

  deliverProgress(line: SalesOrderLineDto): number {
    if (!line.quantity) return 0;
    return Math.min(100, ((line.deliveredQuantity ?? 0) / line.quantity) * 100);
  }

  statusClass(status: SalesOrderStatus): string {
    const map: Record<number, string> = {
      [SalesOrderStatus.Draft]: 'badge-secondary',
      [SalesOrderStatus.PendingApproval]: 'badge-warning',
      [SalesOrderStatus.Approved]: 'badge-info',
      [SalesOrderStatus.Delivering]: 'badge-primary',
      [SalesOrderStatus.Completed]: 'badge-success',
      [SalesOrderStatus.Canceled]: 'badge-danger',
    };
    return map[status] ?? 'badge-secondary';
  }

  statusIcon(status: SalesOrderStatus): string {
    const map: Record<number, string> = {
      [SalesOrderStatus.Draft]: 'fa-pencil',
      [SalesOrderStatus.PendingApproval]: 'fa-clock-o',
      [SalesOrderStatus.Approved]: 'fa-check',
      [SalesOrderStatus.Delivering]: 'fa-truck',
      [SalesOrderStatus.Completed]: 'fa-check-circle',
      [SalesOrderStatus.Canceled]: 'fa-times-circle',
    };
    return map[status] ?? 'fa-circle';
  }
}