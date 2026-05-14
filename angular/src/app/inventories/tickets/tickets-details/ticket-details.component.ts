import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { TicketType } from 'src/app/proxy/enums/warehouses/ticket-type.enum';
import { ApprovalStatus } from 'src/app/proxy/enums/warehouses/approval-status.enum';
import { BatchQAStatus } from 'src/app/proxy/enums/warehouses';
import { StorageCondition } from 'src/app/proxy/enums/medicines';
import { MedicineService } from 'src/app/proxy/medicines';
import { MedicineDto } from 'src/app/proxy/medicines/dtos';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { InventoryTicketDto, InventoryTicketLineDto } from 'src/app/proxy/tickets/dtos';
import { BinDto } from 'src/app/proxy/warehouses/dtos';
import { ProductBatchDto } from 'src/app/proxy/batches/dtos';
import { InventoryTicketService } from 'src/app/proxy/tickets';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { ProductBatchService } from 'src/app/proxy/batches';
import { enumName } from 'src/app/shared/untils/enum.util';
import { PurchaseOrderLineDto } from 'src/app/proxy/purchase-orders/dtos';

interface SelectablePOLineDto extends PurchaseOrderLineDto {
  importQuantity: number;
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
  imports: [SharedModule, DrawerComponent, NgbDropdownModule],
  templateUrl: './ticket-details.component.html',
  styleUrls: ['./ticket-details.component.scss']
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

  bins: BinDto[] = [];
  filteredBins: BinDto[] = [];
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
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private routesService: RoutesService
  ) { }

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

  // ── Accordion Logic ──────────────────────────────────────
  toggleLine(id: string) {
    if (this.expandedLineIds.has(id)) {
      this.expandedLineIds.delete(id);
    } else {
      this.expandedLineIds.add(id);
    }
  }

  // ── Data loading ──────────────────────────────────────────
  loadTicketData() {
    this.loading = true;
    this.ticketService.get(this.ticketId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.ticket = res;
          this.loading = false;
          this.loadBins(res.warehouseId);
          if (res.referenceDocumentId && res.type === TicketType.GoodsReceipt) {
            this.loadPoLines(res.referenceDocumentId);
          }

          if (res.lines) {
            const productIds = Array.from(new Set(res.lines.map(l => l.productId)));
            productIds.forEach(pid => {
                this.batchService.getList({ productId: pid, maxResultCount: 100 } as any).subscribe(batchesRes => {
                    this.lineBatches[pid] = this.filterBatchesByTicketType(batchesRes.items);
                });
            });
          }

          this.routesService.add([{
            path: `/inventory/tickets/details/${this.ticket.id}`,
            name: this.ROUTE_NAME,
            parentName: '::Menu:InventoryTickets',
            iconClass: 'fas fa-file-invoice',
            layout: eLayoutType.application,
          }]);
        },
        error: () => this.goBack()
      });
  }

  loadPoLines(poId: string) {
    this.ticketService.getLinesFromPurchaseOrder(poId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
          this.poLines = res.map(l => ({
              ...l,
              importQuantity: l.quantity
          }));
      });
  }

  loadMasterData() {
    this.medicineService.getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => this.medicines = res.items);
  }

  loadBins(warehouseId: string) {
    this.warehouseService.getStorageBins(warehouseId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.bins = res.filter(b => !b.isBlocked);
        this.applyBinFilter(this.selectedMedicineCondition);
      });
  }

  openLinkedOrder() {
    if (!this.ticket?.referenceDocumentId) return;
    if (this.ticket.type === TicketType.GoodsReceipt || this.ticket.type === TicketType.ReturnOutward) {
        this.router.navigate(['/orders/purchaseorders/details', this.ticket.referenceDocumentId]);
    } else if (this.ticket.type === TicketType.GoodsIssue || this.ticket.type === TicketType.ReturnInward) {
        this.router.navigate(['/orders/saleorders/details', this.ticket.referenceDocumentId]);
    }
  }

  // ── Bin filtering ─────────────────────────────────────────
  private applyBinFilter(condition: StorageCondition | null) {
    this.selectedMedicineCondition = condition;
    if (condition == null) {
      this.filteredBins = this.bins;
      this.hiddenBinCount = 0;
    } else {
      this.filteredBins = this.bins.filter(b => b.zoneStorageCondition === condition);
      this.hiddenBinCount = this.bins.length - this.filteredBins.length;
    }
    
    const currentBinId = this.detailForm?.get('binId')?.value;
    if (currentBinId && !this.filteredBins.find(b => b.id === currentBinId)) {
      this.detailForm?.patchValue({ binId: null });
    }
  }

  // ── Batch filtering ───────────────────────────────────────
  private filterBatchesByTicketType(all: ProductBatchDto[]): ProductBatchDto[] {
    if (!this.isIssueTicket()) {
      return all.filter(b => b.status !== BatchQAStatus.Recalled && b.status !== BatchQAStatus.Expired);
    } else {
      return all.filter(b => b.status === BatchQAStatus.Approved);
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

    this.batchService.getList({ productId: medicineId, maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => this.applyBatchFilter(res.items));

    this.loadUnitsForProduct(medicineId, (units, baseUnitName) => {
      this.units = units;
      this.baseUnitName = baseUnitName;
      
      // ✅ Nếu có targetUnitId (từ TicketLine), ưu tiên chọn nó
      if (targetUnitId) {
        this.detailForm.patchValue({ 
          unitId: targetUnitId,
          conversionFactor: targetFactor || 1
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

  openQuickBatchForm() {
    this.quickBatchForm = this.fb.group({
      batchNumber: ['', [Validators.required, Validators.maxLength(50)]],
      manufacturingDate: [null, [Validators.required]],
      expiryDate: [null, [Validators.required]]
    });
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
    this.batchService.create({ productId, ...this.quickBatchForm.value })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (newBatch) => {
          this.isSavingQuickBatch = false;
          this.isCreatingBatch = false;
          this.allBatches = [...this.allBatches, newBatch];
          this.applyBatchFilter(this.allBatches);
          this.detailForm.patchValue({ productBatchId: newBatch.id });
          this.toaster.success('::CreateSuccess', '::Success');
        },
        error: () => { this.isSavingQuickBatch = false; }
      });
  }

  onUnitChange(unitId: string) {
    const unit = this.units.find((u) => u.unitId === unitId);
    if (unit) {
      this.selectedConversionFactor = unit.conversionFactor;
      this.selectedUnitName = unit.unitName;
      this.detailForm.patchValue({ conversionFactor: unit.conversionFactor });
    }
    this.updateQuantityPreview();
  }

  updateQuantityPreview() {
    const qty = this.detailForm.get('quantity')?.value || 0;
    this.quantityPreview = qty * this.selectedConversionFactor;
  }

  private loadUnitsForProduct(
    medicineId: string,
    callback: (units: ProductUnitLookup[], baseUnitName: string) => void
  ) {
    this.medicineService.get(medicineId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(detail => {
        const baseUnit: ProductUnitLookup = { unitId: detail.baseUnitId, unitName: detail.baseUnitName, conversionFactor: 1, isBaseUnit: true };
        const sorted = [...(detail.units || [])].sort((a, b) => (a.level ?? 0) - (b.level ?? 0));
        let cumulative = 1;
        const others: ProductUnitLookup[] = sorted.map(u => {
          cumulative *= u.conversionFactor ?? 1;
          return { unitId: u.unitId, unitName: u.unitName, conversionFactor: cumulative, isBaseUnit: false };
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
      quantity: [1, [Validators.required, Validators.min(0.01)]]
    });
  }

  openAddDetailDrawer(line: InventoryTicketLineDto, batchId?: string) {
    this.selectedTicketLine = line;
    this.detailForm.reset({ quantity: 1, conversionFactor: 1 });
    
    this.detailForm.patchValue({ 
      productId: line.productId,
      unitId: line.unitId,
      quantity: 1, // Mặc định để 1 để người dùng nhập số lượng cho lô này
      conversionFactor: line.conversionFactor || 1
    });
    
    // ✅ Truyền thêm đơn vị và hệ số để không bị reset
    this.onMedicineChange(line.productId, line.unitId, line.conversionFactor);
    
    // Đợi load batches xong
    setTimeout(() => {
      this.detailForm.patchValue({ 
          productBatchId: batchId || null
      });
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
    this.isSavingDetail = true;

    // ✅ Fix: Pass Line ID to addDetail
    this.ticketService.addDetail(this.selectedTicketLine.id, this.detailForm.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingDetail = false;
          this.closeAddDetailDrawer();
          this.loadTicketData();
        },
        error: () => { this.isSavingDetail = false; }
      });
  }

  deleteDetail(detailId: string) {
      this.confirmation.warn('::AreYouSureToDeleteDetail', '::AreYouSure').subscribe(status => {
          if (status === Confirmation.Status.confirm) {
              // ✅ Fix: Pass Detail ID directly
              this.ticketService.deleteDetail(detailId)
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
              this.ticketService.deleteLine(lineId)
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
    this.ticketService.addLineFromPurchaseOrder(this.ticketId, poLine.id, poLine.importQuantity)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
          this.loadTicketData();
          this.toaster.success('::ImportSuccess', '::Success');
          
          // Sau khi import xong, tìm line mới tạo và tự động mở drawer gán lô cho nó luôn (để người dùng làm tiếp bước tiếp theo)
          // Hoặc đơn giản là load lại data và để người dùng tự chọn. Ở đây ta load lại data.
          this.closePoLineDrawer();
      });
  }

  // ── Ticket workflow ───────────────────────────────────────
  sendToApprove() {
    if (!this.ticket?.lines?.length) {
      this.confirmation.error('::NoDataError', '::Error');
      return;
    }
    this.confirmation.info('::SendToApproveConfirmation', '::Confirm')
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) return;
        this.ticketService.sendToApprove(this.ticketId)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.loadTicketData();
          });
      });
  }

  execute() {
    this.confirmation.success('::ExecuteConfirmation', '::Confirm')
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) return;
        this.ticketService.execute(this.ticketId)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.loadTicketData();
          });
      });
  }

  isIssueTicket(): boolean {
    return this.ticket?.type === TicketType.GoodsIssue
      || this.ticket?.type === TicketType.DisposalIssue
      || this.ticket?.type === TicketType.ReturnOutward;
  }
}
