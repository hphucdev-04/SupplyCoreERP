import { Component, OnDestroy, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { TicketType } from 'src/app/proxy/enums/warehouses/ticket-type.enum';
import { ApprovalStatus } from 'src/app/proxy/enums/warehouses/approval-status.enum';
import { BatchQAStatus, ZoneType } from 'src/app/proxy/enums/warehouses';
import { StorageCondition } from 'src/app/proxy/enums/medicines';
import { MedicineService } from 'src/app/proxy/medicines';
import { MedicineDto, MedicineRegistrationDto } from 'src/app/proxy/medicines/dtos';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { DropdownSearchComponent } from 'src/app/shared/components/dropdownsearch-component/dropdown-search.component';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { InventoryTicketDto, InventoryTicketLineDto } from 'src/app/proxy/tickets/dtos';
import { BinDto } from 'src/app/proxy/warehouses/dtos';
import { ProductBatchDto } from 'src/app/proxy/batches/dtos';
import { InventoryTicketService } from 'src/app/proxy/tickets';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { ProductBatchService } from 'src/app/proxy/batches';
import { InventoryBalanceService } from 'src/app/proxy/balances';
import { enumName } from 'src/app/shared/untils/enum.util';
import { PurchaseOrderService } from 'src/app/proxy/purchase-orders';
import { PurchaseOrderLineDto } from 'src/app/proxy/purchase-orders/dtos';
import { SalesOrderLineDto } from 'src/app/proxy/sales-orders/dtos';
import { PurchaseReturnLineDto } from 'src/app/proxy/purchase-returns/dtos/models';
import { SalesRecallLineDto } from 'src/app/proxy/sales-recalls/dtos/models';
import { UnitConversionHelper } from 'src/app/shared/untils/unit-conversion.helper';

interface SelectablePOLineDto extends PurchaseOrderLineDto {
  importQuantity: number;
}

interface SelectableSOLineDto extends SalesOrderLineDto {
  exportQuantity: number;
}

interface ProductUnitLookup {
  unitId: string;
  unitName: string;
  conversionFactor: number;
  isBaseUnit: boolean;
}

@Component({
  selector: 'app-ticket-details',
  standalone: true,
  imports: [SharedModule, DrawerComponent, DropdownSearchComponent, NgbDropdownModule],
  templateUrl: './ticket-details.component.html',
  styleUrls: ['./ticket-details.component.scss'],
})
export class TicketDetailsComponent implements OnInit, OnDestroy {
  @ViewChild('rejectReasonModal', { static: false }) rejectReasonModal: any;

  private destroy$ = new Subject<void>();
  private readonly ROUTE_NAME = '::Menu:TicketDetails';

  ticketId: string;
  ticket: InventoryTicketDto;
  loading = true;

  // Accordion state
  expandedLineIds = new Set<string>();

  rejectReason = '';
  showRejectError = false;
  isRejecting = false;

  bins: (BinDto & { codeWithStock?: string })[] = [];
  filteredBins: (BinDto & { codeWithStock?: string })[] = [];
  hiddenBinCount = 0;
  selectedMedicineCondition: StorageCondition | null = null;
  medicines: MedicineDto[] = [];

  allBatches: ProductBatchDto[] = [];
  batches: ProductBatchDto[] = [];
  hiddenBatchCount = 0;

  isCreatingBatch = false;
  quickBatchForm: FormGroup;
  isSavingQuickBatch = false;

  isAddDetailDrawerOpen = false;
  detailForm: FormGroup;
  isSavingDetail = false;
  selectedTicketLine: InventoryTicketLineDto | null = null;

  units: ProductUnitLookup[] = [];
  selectedConversionFactor = 1;
  selectedUnitName = '';
  baseUnitName = '';
  quantityPreview = 0;
  remainingQty = 0;

  registrations: MedicineRegistrationDto[] = [];
  poSupplier: { id: string; name: string } | null = null;

  isFefoDrawerOpen = false;
  fefoForm: FormGroup;
  isRunningFefo = false;

  fefoUnits: ProductUnitLookup[] = [];
  fefoBaseQtyPreview = 0;
  fefoBaseUnitName = '';
  private fefoConversionFactor = 1;

  // PO Selection
  poLines: SelectablePOLineDto[] = [];
  isPoLineDrawerOpen = false;

  // SO Selection
  soLines: SelectableSOLineDto[] = [];
  isSoLineDrawerOpen = false;

  // PR Selection
  prLines: (PurchaseReturnLineDto & { exportQuantity?: number })[] = [];
  isPrLineDrawerOpen = false;

  // Recall Selection
  recallLines: (SalesRecallLineDto & { exportQuantity?: number })[] = [];
  isRecallLineDrawerOpen = false;

  lineBatches: { [productId: string]: ProductBatchDto[] } = {};

  TicketType = TicketType;
  ApprovalStatus = ApprovalStatus;
  BatchQAStatus = BatchQAStatus;
  StorageCondition = StorageCondition;

  readonly enumName = enumName;

  constructor(
    private ticketService: InventoryTicketService,
    private warehouseService: WarehouseService,
    private medicineService: MedicineService,
    private batchService: ProductBatchService,
    private poService: PurchaseOrderService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private routesService: RoutesService,
    private balanceService: InventoryBalanceService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.ticketId = this.route.snapshot.params['id'];
    if (this.ticketId) {
      this.buildForms();
      this.loadTicketData();
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

  goBack(): void {
    this.router.navigate(['/inventory/tickets']);
  }

  // Accordion Logic 
  toggleLine(id: string) {
    if (this.expandedLineIds.has(id)) {
      this.expandedLineIds.delete(id);
    } else {
      this.expandedLineIds.add(id);
    }
  }

  // Data loading 
  loadTicketData() {
    this.loading = true;
    this.ticketService
      .get(this.ticketId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.ticket = res;
          this.loading = false;
          this.loadBins(res.warehouseId);
          if (res.referenceDocumentId) {
            if (this.isGoodsReceiptTicket()) {
              this.loadPoLines(res.referenceDocumentId);
              this.poService.get(res.referenceDocumentId)
                .pipe(takeUntil(this.destroy$))
                .subscribe(po => {
                  if (po) {
                    this.poSupplier = { id: po.supplierId, name: po.supplierName };
                  }
                });
            } else if (this.isGoodsIssueTicket()) {
              this.loadSoLines(res.referenceDocumentId);
            } else if (this.isReturnOutwardTicket()) {
              this.loadPrLines(res.referenceDocumentId);
            } else if (this.isRecallReceiptTicket()) {
              this.loadRecallLines(res.referenceDocumentId);
            }
          }

          if (res.lines) {
            const productIds = Array.from(new Set(res.lines.map(l => l.productId)));
            productIds.forEach(pid => {
              this.batchService
                .getList({ productId: pid, maxResultCount: 100 } as any)
                .subscribe(batchesRes => {
                  this.lineBatches[pid] = this.filterBatchesByTicketType(batchesRes.items);
                });
            });
          }

          this.routesService.add([
            {
              path: `/inventory/tickets/details/${this.ticket.id}`,
              name: this.ROUTE_NAME,
              parentName: '::Menu:InventoryTickets',
              iconClass: 'fas fa-file-invoice',
              layout: eLayoutType.application,
            },
          ]);
        },
        error: () => this.goBack(),
      });
  }

  loadPoLines(poId: string) {
    this.ticketService
      .getLinesFromPurchaseOrder(poId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.poLines = res.map(l => ({
          ...l,
          importQuantity: l.quantity,
        }));
      });
  }

  loadMasterData() {
    this.medicineService
      .getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.medicines = res.items));
  }

  loadBins(warehouseId: string) {
    this.warehouseService
      .getStorageBins(warehouseId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.bins = res.filter(b => !b.isBlocked).map(b => ({
          ...b,
          codeWithStock: b.code
        }));
        this.applyBinFilter(this.selectedMedicineCondition);
      });
  }

  openLinkedOrder() {
    if (!this.ticket?.referenceDocumentId) return;
    if (this.isGoodsReceiptTicket() || this.isReturnOutwardTicket()) {
      this.router.navigate(['/orders/purchaseorders/details', this.ticket.referenceDocumentId]);
    } else if (this.isGoodsIssueTicket() || this.isRecallReceiptTicket()) {
      this.router.navigate(['/orders/saleorders/details', this.ticket.referenceDocumentId]);
    }
  }

  // ── Bin filtering ─────────────────────────────────────────
  private applyBinFilter(condition: StorageCondition | null) {
    this.selectedMedicineCondition = condition;
    
    // 1. Lọc theo phân khu được phép chứa (allowedZones) ứng với loại Ticket
    let allowedZones: ZoneType[] = [];
    if (this.ticket) {
      const type = this.ticket.type as any;
      const isReceipt = type === TicketType.GoodsReceipt || String(type) === '0' || type === 'GoodsReceipt';
      const isRecallOrReturnOut = type === TicketType.RecallReceipt || String(type) === '4' || type === 'RecallReceipt'
        || type === TicketType.ReturnOutward || String(type) === '3' || type === 'ReturnOutward';
      const isIssue = type === TicketType.GoodsIssue || String(type) === '1' || type === 'GoodsIssue';

      if (isReceipt) {
        allowedZones = [ZoneType.QA];
      } else if (isRecallOrReturnOut) {
        allowedZones = [ZoneType.Quarantine];
      } else if (isIssue) {
        allowedZones = [ZoneType.Storage, ZoneType.QA];
      }
    }

    let result = this.bins;
    if (allowedZones.length > 0) {
      result = result.filter(b => {
        if (b.zoneType === undefined || b.zoneType === null) return false;
        return allowedZones.some(az => (az as any) === b.zoneType || String(az) === String(b.zoneType) || ZoneType[az] === (b.zoneType as any));
      });
    }

    // 2. Lọc theo điều kiện bảo quản (chỉ bắt buộc đối với phân khu Storage)
    if (condition !== null) {
      result = result.filter(b => {
        const isStorage = (b.zoneType as any) === ZoneType.Storage || String(b.zoneType) === '0' || (b.zoneType as any) === 'Storage';
        if (isStorage) {
          return (b.zoneStorageCondition as any) === condition || String(b.zoneStorageCondition) === String(condition) || StorageCondition[b.zoneStorageCondition as any] === (condition as any);
        }
        return true;
      });
    }

    this.filteredBins = result;
    this.hiddenBinCount = this.bins.length - this.filteredBins.length;

    const currentBinId = this.detailForm?.get('binId')?.value;
    if (currentBinId && !this.filteredBins.find(b => b.id === currentBinId)) {
      this.detailForm?.patchValue({ binId: null });
    }
  }

  // ── Batch filtering ───────────────────────────────────────
  private filterBatchesByTicketType(all: ProductBatchDto[]): ProductBatchDto[] {
    const type = this.ticket?.type as any;
    const isGoodsIssue = type === TicketType.GoodsIssue || String(type) === '1' || type === 'GoodsIssue';
    const isReturnOrDisposal =
      type === TicketType.ReturnOutward || String(type) === '3' || type === 'ReturnOutward' ||
      type === TicketType.DisposalIssue || String(type) === '5' || type === 'DisposalIssue';

    if (isGoodsIssue) {
      return all.filter(b => b.status === BatchQAStatus.Approved);
    } else if (isReturnOrDisposal) {
      return all;
    } else {
      return all.filter(
        b => b.status !== BatchQAStatus.Recalled && b.status !== BatchQAStatus.Expired,
      );
    }
  }

  private applyBatchFilter(all: ProductBatchDto[]) {
    this.allBatches = all;
    this.batches = this.filterBatchesByTicketType(all);
    this.hiddenBatchCount = all.length - this.batches.length;
  }

  onMedicineChange(medicineId: string, targetUnitId?: string, targetFactor?: number) {
    this.detailForm.patchValue({ productBatchId: null, unitId: null, binId: null });
    this.allBatches = [];
    this.batches = [];
    this.hiddenBatchCount = 0;
    this.units = [];
    this.selectedConversionFactor = 1;
    this.selectedUnitName = '';
    this.baseUnitName = '';
    this.quantityPreview = 0;
    this.isCreatingBatch = false;
    this.quickBatchForm?.reset();

    if (!medicineId) {
      this.applyBinFilter(null);
      return;
    }

    const medicine = this.medicines.find(m => m.id === medicineId);
    this.applyBinFilter(medicine?.storageCondition ?? null);

    // ✅ Tải danh sách SĐK bằng API mới tối ưu
    this.medicineService
      .getRegistrations(medicineId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.registrations = res.filter(r => r.isActive);
      });

    this.batchService
      .getList({ productId: medicineId, maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => this.applyBatchFilter(res.items));

    this.loadUnitsForProduct(medicineId, (units, baseUnitName) => {
      this.units = units;
      this.baseUnitName = baseUnitName;

      // ✅ Nếu có targetUnitId (từ TicketLine), ưu tiên chọn nó
      if (targetUnitId) {
        this.detailForm.patchValue({
          unitId: targetUnitId,
          conversionFactor: targetFactor || 1,
        });
        const unit = units.find(u => u.unitId === targetUnitId);
        this.selectedConversionFactor = targetFactor || unit?.conversionFactor || 1;
        this.selectedUnitName = unit?.unitName || '';
      } else {
        // Mặc định chọn BaseUnit
        const base = units.find(u => u.isBaseUnit);
        if (base) {
          this.detailForm.patchValue({ unitId: base.unitId });
          this.selectedConversionFactor = 1;
          this.selectedUnitName = base.unitName;
        }
      }
      this.updateQuantityPreview();
    });
  }

  onBatchChange(batchId: string) {
    this.detailForm.patchValue({ binId: null });
    const productId = this.detailForm.get('productId')?.value;

    if (this.isIssueTicket() && productId && batchId) {
      this.balanceService.getList({
        warehouseId: this.ticket.warehouseId,
        productId: productId,
        productBatchId: batchId,
        maxResultCount: 1,
        skipCount: 0
      } as any).pipe(takeUntil(this.destroy$))
        .subscribe(res => {
          const balance = res.items?.[0];
          if (balance) {
            this.balanceService.get(balance.id)
              .pipe(takeUntil(this.destroy$))
              .subscribe(detail => {
                const balancesList = detail.binBalances || [];
                const availableBins = balancesList.filter(bb => (bb.availableQuantity || 0) > 0);
                const binIds = availableBins.map(ab => ab.binId);

                const medicine = this.medicines.find(m => m.id === productId);
                this.applyBinFilter(medicine?.storageCondition ?? null);

                this.filteredBins = this.filteredBins.filter(b => binIds.includes(b.id)).map(b => {
                  const bb = availableBins.find(ab => ab.binId === b.id);
                  const availableQty = bb?.availableQuantity || 0;
                  return {
                    ...b,
                    codeWithStock: `${b.code} (Khả dụng: ${availableQty} ${this.baseUnitName})`
                  };
                });
                this.cdr.markForCheck();
              });
          } else {
            this.filteredBins = [];
            this.cdr.markForCheck();
          }
        });
    } else {
      const medicine = this.medicines.find(m => m.id === productId);
      this.applyBinFilter(medicine?.storageCondition ?? null);
      this.filteredBins = this.filteredBins.map(b => ({
        ...b,
        codeWithStock: b.code
      }));
      this.cdr.markForCheck();
    }
  }

  openQuickBatchForm() {
    this.quickBatchForm = this.fb.group({
      batchNumber: ['', [Validators.required, Validators.maxLength(50)]],
      manufacturingDate: [null, [Validators.required]],
      expiryDate: [null, [Validators.required]],
      medicineRegistrationId: [null],
      supplierId: [this.poSupplier?.id || null]
    });

    // Mặc định chọn SĐK đầu tiên nếu có
    if (this.registrations.length > 0) {
      this.quickBatchForm.patchValue({
        medicineRegistrationId: this.registrations[0].id,
      });
    }

    this.isCreatingBatch = true;
  }

  cancelQuickBatch() {
    this.isCreatingBatch = false;
    this.quickBatchForm = null;
  }

  saveQuickBatch() {
    if (this.quickBatchForm?.invalid) return;
    const mfg = new Date(this.quickBatchForm.value.manufacturingDate);
    const exp = new Date(this.quickBatchForm.value.expiryDate);
    if (exp <= mfg) {
      this.toaster.error('::ExpiryDateMustBeGreaterThanMfgDate', '::Error');
      return;
    }
    const productId = this.detailForm.get('productId')?.value;
    if (!productId) return;

    this.isSavingQuickBatch = true;
    this.batchService
      .create({
        productId,
        ...this.quickBatchForm.value,
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: newBatch => {
          this.isSavingQuickBatch = false;
          this.isCreatingBatch = false;
          this.allBatches = [...this.allBatches, newBatch];
          this.applyBatchFilter(this.allBatches);
          this.detailForm.patchValue({ productBatchId: newBatch.id });
          this.onBatchChange(newBatch.id);
          this.toaster.success('::CreateSuccess', '::Success');
        },
        error: () => {
          this.isSavingQuickBatch = false;
        },
      });
  }

  onUnitChange(unitId: string) {
    const unit = this.units.find(u => u.unitId === unitId);
    if (unit) {
      this.selectedConversionFactor = unit.conversionFactor;
      this.selectedUnitName = unit.unitName;
      this.detailForm.patchValue({ conversionFactor: unit.conversionFactor });
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

  private loadUnitsForProduct(
    medicineId: string,
    callback: (units: ProductUnitLookup[], baseUnitName: string) => void,
  ) {
    this.medicineService
      .get(medicineId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(detail => {
        const baseUnit: ProductUnitLookup = {
          unitId: detail.baseUnitId,
          unitName: detail.baseUnitName,
          conversionFactor: 1,
          isBaseUnit: true,
        };
        const sorted = [...(detail.units || [])].sort((a, b) => (a.level ?? 0) - (b.level ?? 0));
        let cumulative = 1;
        const others: ProductUnitLookup[] = sorted.map(u => {
          cumulative *= u.conversionFactor ?? 1;
          return {
            unitId: u.unitId,
            unitName: u.unitName,
            conversionFactor: cumulative,
            isBaseUnit: false,
          };
        });
        callback([baseUnit, ...others], detail.baseUnitName ?? '');
      });
  }

  buildForms() {
    this.detailForm = this.fb.group({
      productId: [null, [Validators.required]],
      productBatchId: [null, [Validators.required]],
      binId: [null, [Validators.required]],
      unitId: [null, [Validators.required]],
      conversionFactor: [1, [Validators.required, Validators.min(1)]],
      quantity: [1, [Validators.required, Validators.min(0.01)]],
    });
  }

  openAddDetailDrawer(line: InventoryTicketLineDto, batchId?: string) {
    this.selectedTicketLine = line;

    // Tính toán số lượng còn lại
    const totalAssignedBaseQty = (line.details || []).reduce(
      (sum, d) =>
        sum +
        UnitConversionHelper.convertToBaseQuantity(
          { baseUnitId: '', units: [{ unitId: d.unitId, conversionFactor: d.conversionFactor }] },
          d.unitId,
          d.quantity || 0,
        ),
      0,
    );
    const lineBaseQty = UnitConversionHelper.convertToBaseQuantity(
      { baseUnitId: '', units: [{ unitId: line.unitId, conversionFactor: line.conversionFactor }] },
      line.unitId,
      line.quantity || 0,
    );
    const remainingBaseQty = Math.max(0, lineBaseQty - totalAssignedBaseQty);
    this.remainingQty = UnitConversionHelper.convertFromBaseQuantity(
      { baseUnitId: '', units: [{ unitId: line.unitId, conversionFactor: line.conversionFactor }] },
      line.unitId,
      remainingBaseQty,
    );

    this.detailForm.reset({
      productId: line.productId,
      unitId: line.unitId,
      quantity: this.remainingQty > 0 ? this.remainingQty : 1,
      conversionFactor: line.conversionFactor || 1,
    });

    // ✅ Truyền thêm đơn vị và hệ số để không bị reset
    this.onMedicineChange(line.productId, line.unitId, line.conversionFactor);

    // Đợi load batches xong
    setTimeout(() => {
      this.detailForm.patchValue({
        productBatchId: batchId || null,
      });
      this.onBatchChange(batchId || null);
      this.updateQuantityPreview();
    }, 800);
    this.isAddDetailDrawerOpen = true;
  }

  openQuickCreateBatchDrawer(line: InventoryTicketLineDto) {
    this.openAddDetailDrawer(line);
    // Đợi drawer mở và medicine được set rồi mới mở quick form
    setTimeout(() => {
      this.openQuickBatchForm();
    }, 600);
  }

  closeAddDetailDrawer() {
    this.isAddDetailDrawerOpen = false;
    this.isCreatingBatch = false;
  }

  saveDetail() {
    if (this.detailForm.invalid || !this.selectedTicketLine) return;

    // Kiểm tra số lượng còn lại ở client
    const inputQty = this.detailForm.get('quantity')?.value || 0;
    if (inputQty > this.remainingQty + 0.0001) {
      this.toaster.error('::Error:QuantityExceedsRemaining', '::Error');
      return;
    }

    this.isSavingDetail = true;

    // ✅ Sử dụng getRawValue() để lấy cả các trường bị disabled (productId, unitId)
    const payload = this.detailForm.getRawValue();

    this.ticketService
      .addDetail(this.selectedTicketLine.id, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingDetail = false;
          this.closeAddDetailDrawer();
          this.loadTicketData();
        },
        error: () => {
          this.isSavingDetail = false;
        },
      });
  }

  deleteDetail(detailId: string) {
    this.confirmation.warn('::AreYouSureToDeleteDetail', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        // ✅ Fix: Pass Detail ID directly
        this.ticketService
          .deleteDetail(detailId)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.loadTicketData();
          });
      }
    });
  }

  deleteLine(lineId: string) {
    this.confirmation.warn('::AreYouSureToDeleteLine', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        // ✅ Fix: Pass Line ID directly
        this.ticketService
          .deleteLine(lineId)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.loadTicketData();
          });
      }
    });
  }

  // ── PO Line Selection ─────────────────────────────────────
  openPoLineDrawer() {
    this.isPoLineDrawerOpen = true;
  }

  closePoLineDrawer() {
    this.isPoLineDrawerOpen = false;
  }

  addPoLineToTicket(poLine: SelectablePOLineDto) {
    if (poLine.importQuantity <= 0) {
      this.toaster.error('::QuantityMustBeGreaterThanZero', '::Error');
      return;
    }
    if (poLine.importQuantity > poLine.quantity) {
      this.toaster.error('::ImportQuantityExceedsRemaining', '::Error');
      return;
    }

    // ✅ TicketId for root, payload for batch info
    this.ticketService
      .addLineFromPurchaseOrder(this.ticketId, poLine.id, poLine.importQuantity)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadTicketData();
        this.toaster.success('::ImportSuccess', '::Success');

        // Sau khi import xong, tìm line mới tạo và tự động mở drawer gán lô cho nó luôn (để người dùng làm tiếp bước tiếp theo)
        // Hoặc đơn giản là load lại data và để người dùng tự chọn. Ở đây ta load lại data.
        this.closePoLineDrawer();
      });
  }

  // ── SO Line Selection ─────────────────────────────────────
  loadSoLines(soId: string) {
    this.ticketService
      .getLinesFromSalesOrder(soId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.soLines = res.map(l => ({
          ...l,
          exportQuantity: l.quantity,
        }));
      });
  }

  openSoLineDrawer() {
    this.isSoLineDrawerOpen = true;
  }

  closeSoLineDrawer() {
    this.isSoLineDrawerOpen = false;
  }

  addSoLineToTicket(soLine: SelectableSOLineDto) {
    if (soLine.exportQuantity <= 0) {
      this.toaster.error('::QuantityMustBeGreaterThanZero', '::Error');
      return;
    }
    if (soLine.exportQuantity > soLine.quantity) {
      this.toaster.error('::ExportQuantityExceedsRemaining', '::Error');
      return;
    }

    this.ticketService
      .addLineFromSalesOrder(this.ticketId, soLine.id, soLine.exportQuantity)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadTicketData();
        this.toaster.success('::AllocationSuccess', '::Success');
        this.closeSoLineDrawer();
      });
  }

  // ── PR Line Selection ─────────────────────────────────────
  loadPrLines(prId: string) {
    this.ticketService
      .getLinesFromPurchaseReturn(prId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.prLines = res.map(l => ({
          ...l,
          exportQuantity: l.quantity,
        }));
      });
  }

  openPrLineDrawer() {
    this.isPrLineDrawerOpen = true;
  }

  closePrLineDrawer() {
    this.isPrLineDrawerOpen = false;
  }

  addPrLineToTicket(prLine: any) {
    if (prLine.exportQuantity <= 0) {
      this.toaster.error('::QuantityMustBeGreaterThanZero', '::Error');
      return;
    }
    if (prLine.exportQuantity > prLine.quantity) {
      this.toaster.error('::ExportQuantityExceedsRemaining', '::Error');
      return;
    }

    this.ticketService
      .addLineFromPurchaseReturn(this.ticketId, prLine.id, prLine.exportQuantity)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadTicketData();
        this.toaster.success('::AllocationSuccess', '::Success');
        this.closePrLineDrawer();
      });
  }

  // ── Recall Line Selection ─────────────────────────────────
  loadRecallLines(recallId: string) {
    this.ticketService
      .getLinesFromSalesRecall(recallId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.recallLines = res.map(l => ({
          ...l,
          exportQuantity: l.quantity,
        }));
      });
  }

  openRecallLineDrawer() {
    this.isRecallLineDrawerOpen = true;
  }

  closeRecallLineDrawer() {
    this.isRecallLineDrawerOpen = false;
  }

  addRecallLineToTicket(recallLine: any) {
    if (recallLine.exportQuantity <= 0) {
      this.toaster.error('::QuantityMustBeGreaterThanZero', '::Error');
      return;
    }
    if (recallLine.exportQuantity > recallLine.quantity) {
      this.toaster.error('::ExportQuantityExceedsRemaining', '::Error');
      return;
    }

    this.ticketService
      .addLineFromSalesRecall(this.ticketId, recallLine.id, recallLine.exportQuantity)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadTicketData();
        this.toaster.success('::AllocationSuccess', '::Success');
        this.closeRecallLineDrawer();
      });
  }

  // ── Ticket workflow ───────────────────────────────────────
  sendToApprove() {
    if (!this.ticket?.lines?.length) {
      this.confirmation.error('::NoDataError', '::Error');
      return;
    }

    // Kiểm tra tính đầy đủ của các dòng hàng trước khi gửi duyệt
    for (const line of this.ticket.lines) {
      const totalAssignedBaseQty = (line.details || []).reduce(
        (sum, d) =>
          sum +
          UnitConversionHelper.convertToBaseQuantity(
            { baseUnitId: '', units: [{ unitId: d.unitId, conversionFactor: d.conversionFactor }] },
            d.unitId,
            d.quantity || 0,
          ),
        0,
      );
      const lineBaseQty = UnitConversionHelper.convertToBaseQuantity(
        {
          baseUnitId: '',
          units: [{ unitId: line.unitId, conversionFactor: line.conversionFactor }],
        },
        line.unitId,
        line.quantity || 0,
      );

      if (Math.abs(lineBaseQty - totalAssignedBaseQty) > 0.0001) {
        this.toaster.error('::Error:LineNotFullyAllocated', '::Error', {
          messageLocalizationParams: [line.productName],
        });
        return;
      }
    }

    this.confirmation.info('::SendToApproveConfirmation', '::Confirm').subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.ticketService
        .sendToApprove(this.ticketId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.loadTicketData();
        });
    });
  }

  execute() {
    this.confirmation.success('::ExecuteConfirmation', '::Confirm').subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.ticketService
        .execute(this.ticketId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.loadTicketData();
        });
    });
  }

  applyFEFO() {
    this.confirmation.warn('::ApplyFEFOConfirmation', '::AreYouSure').subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;
      this.ticketService
        .applyFEFO(this.ticketId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.toaster.success('::ApplyFEFOSuccess', '::Success');
          this.loadTicketData();
        });
    });
  }

  isIssueTicket(): boolean {
    if (!this.ticket) return false;
    const type = this.ticket.type as any;
    return (
      type === TicketType.GoodsIssue || String(type) === '1' || type === 'GoodsIssue' ||
      type === TicketType.DisposalIssue || String(type) === '5' || type === 'DisposalIssue' ||
      type === TicketType.ReturnOutward || String(type) === '3' || type === 'ReturnOutward'
    );
  }

  isGoodsIssueTicket(): boolean {
    if (!this.ticket) return false;
    const type = this.ticket.type as any;
    return type === TicketType.GoodsIssue || String(type) === '1' || type === 'GoodsIssue';
  }

  isGoodsReceiptTicket(): boolean {
    if (!this.ticket) return false;
    const type = this.ticket.type as any;
    return type === TicketType.GoodsReceipt || String(type) === '0' || type === 'GoodsReceipt';
  }

  isReturnOutwardTicket(): boolean {
    if (!this.ticket) return false;
    const type = this.ticket.type as any;
    return type === TicketType.ReturnOutward || String(type) === '3' || type === 'ReturnOutward';
  }

  isRecallReceiptTicket(): boolean {
    if (!this.ticket) return false;
    const type = this.ticket.type as any;
    return type === TicketType.RecallReceipt || String(type) === '4' || type === 'RecallReceipt';
  }

  getLineAssignedBaseQty(line: InventoryTicketLineDto): number {
    return (line.details || []).reduce(
      (sum, d) =>
        sum +
        UnitConversionHelper.convertToBaseQuantity(
          { baseUnitId: '', units: [{ unitId: d.unitId, conversionFactor: d.conversionFactor }] },
          d.unitId,
          d.quantity || 0,
        ),
      0,
    );
  }
}
