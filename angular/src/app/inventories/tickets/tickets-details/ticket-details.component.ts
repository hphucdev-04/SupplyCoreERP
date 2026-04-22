import { Component, OnDestroy, Output, EventEmitter, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
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
import { InventoryTicketDto } from 'src/app/proxy/tickets/dtos';
import { BinDto } from 'src/app/proxy/warehouses/dtos';
import { ProductBatchDto } from 'src/app/proxy/batches/dtos';
import { InventoryTicketService } from 'src/app/proxy/tickets';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { ProductBatchService } from 'src/app/proxy/batches';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { enumName } from 'src/app/shared/untils/enum.util';
import { PurchaseOrderDetailsComponent } from 'src/app/orders/purchaseorders/purchaseorder-details/purchaseorder-details.component';
import { SalesOrderDetailsComponent } from 'src/app/orders/saleorders/saleorder-details/saleorder-details.component';

interface ProductUnitLookup {
  unitId: string;
  unitName: string;
  conversionFactor: number;
  isBaseUnit: boolean;
}

@Component({
  selector: 'app-ticket-details',
  standalone: true,
  imports: [SharedModule, DrawerComponent, PurchaseOrderDetailsComponent, SalesOrderDetailsComponent],
  templateUrl: './ticket-details.component.html',
  styleUrls: ['./ticket-details.component.scss']
})
export class TicketDetailsComponent implements OnDestroy {
  @ViewChild('rejectReasonModal', { static: false }) rejectReasonModal: any;
  @ViewChild('poDetailModal') poDetailModal: PurchaseOrderDetailsComponent;
  @ViewChild('soDetailModal') soDetailModal: SalesOrderDetailsComponent;

  @Output() onSaved = new EventEmitter<void>();

  private destroy$ = new Subject<void>();

  isVisible = false;
  ticketId: string;
  ticket: InventoryTicketDto;

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
    private modalService: NgbModal
  ) { }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Public API ────────────────────────────────────────────
  open(ticketId: string) {
    this.ticketId = ticketId;
    this.ticket = null;
    this.buildForms();
    this.loadTicketData();
    this.loadMasterData();
    this.isVisible = true;
  }

  close() {
    this.isVisible = false;
  }

  // ── Data loading ──────────────────────────────────────────
  loadTicketData() {
    this.ticketService.get(this.ticketId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.ticket = res;
          this.loadBins(res.warehouseId);
        },
        error: () => this.close()
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

  // ✅ NEW: Open linked PO or SO from ticket's reference
  openLinkedOrder() {
    if (!this.ticket?.referenceDocumentId) return;

    if (this.ticket.type === TicketType.GoodsReceipt || this.ticket.type === TicketType.ReturnOutward) {
      // Linked to Purchase Order
      this.poDetailModal?.open(this.ticket.referenceDocumentId);
    } else if (this.ticket.type === TicketType.GoodsIssue || this.ticket.type === TicketType.ReturnInward) {
      // Linked to Sales Order
      this.soDetailModal?.open(this.ticket.referenceDocumentId);
    }
  }

  // ── UX MỚI: Inline Edit ───────────────────────────────────
  onInlineQuantityChange(row: any, newValue: string) {
    const newQty = parseFloat(newValue);
    if (isNaN(newQty) || newQty <= 0) {
      this.toaster.error('::InvalidQuantity', '::Error');
      this.loadTicketData();
      return;
    }
    if (newQty === row.quantity) return;

    this.ticketService.updateDetailQuantity(row.id, newQty)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toaster.success('::UpdateSuccess', '::Success');
          this.loadTicketData();
          this.onSaved.emit();
        },
        error: () => this.loadTicketData()
      });
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
    console.log("Số Bin sau khi lọc:", this.filteredBins.length);
    
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

  // ── Medicine change ───────────────────────────────────────
  onMedicineChange(medicineId: string) {
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
      const base = units.find(u => u.isBaseUnit);
      if (base) {
        this.detailForm.patchValue({ unitId: base.unitId });
        this.selectedConversionFactor = 1;
        this.selectedUnitName = base.unitName;
      }
    });
  }

  // ── Inline batch creation ─────────────────────────────────
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

  // ── Unit change ───────────────────────────────────────────
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
    this.quantityPreview = qty * this.selectedConversionFactor;
  }

  // ── FEFO ──────────────────────────────────────────────────
  onFefoMedicineChange(medicineId: string) {
    this.fefoForm.patchValue({ unitId: null, requiredQuantity: 1 });
    this.fefoUnits = [];
    this.fefoConversionFactor = 1;
    this.fefoBaseQtyPreview = 0;
    this.fefoBaseUnitName = '';

    if (!medicineId) return;

    this.loadUnitsForProduct(medicineId, (units, baseUnitName) => {
      this.fefoUnits = units;
      this.fefoBaseUnitName = baseUnitName;
      const base = units.find(u => u.isBaseUnit);
      if (base) {
        this.fefoForm.patchValue({ unitId: base.unitId });
        this.fefoConversionFactor = 1;
      }
    });
  }

  onFefoUnitChange(unitId: string) {
    const unit = this.fefoUnits.find(u => u.unitId === unitId);
    if (unit) {
      this.fefoConversionFactor = unit.conversionFactor;
      this.fefoForm.patchValue({ conversionFactor: unit.conversionFactor });
    }
    this.updateFefoPreview();
  }

  updateFefoPreview() {
    const qty = this.fefoForm.get('requiredQuantity')?.value || 0;
    this.fefoBaseQtyPreview = qty * this.fefoConversionFactor;
  }

  // ── Load units helper ─────────────────────────────────────
  private loadUnitsForProduct(
    medicineId: string,
    callback: (units: ProductUnitLookup[], baseUnitName: string) => void
  ) {
    this.medicineService.get(medicineId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(detail => {
        const baseUnit: ProductUnitLookup = {
          unitId: detail.baseUnitId,
          unitName: detail.baseUnitName,
          conversionFactor: 1,
          isBaseUnit: true
        };

        const sorted = [...(detail.units || [])].sort((a, b) => (a.level ?? 0) - (b.level ?? 0));
        let cumulative = 1;
        const others: ProductUnitLookup[] = sorted.map(u => {
          cumulative *= u.conversionFactor ?? 1;
          return {
            unitId: u.unitId,
            unitName: u.unitName,
            conversionFactor: cumulative,
            isBaseUnit: false
          };
        });

        callback([baseUnit, ...others], detail.baseUnitName ?? '');
      });
  }

  // ── Forms ─────────────────────────────────────────────────
  buildForms() {
    this.detailForm = this.fb.group({
      productId: [null, [Validators.required]],
      productBatchId: [null, [Validators.required]],
      binId: [null, [Validators.required]],
      unitId: [null, [Validators.required]],
      conversionFactor: [1, [Validators.required, Validators.min(1)]],
      quantity: [1, [Validators.required, Validators.min(0.01)]]
    });

    this.fefoForm = this.fb.group({
      productId: [null, [Validators.required]],
      unitId: [null, [Validators.required]],
      conversionFactor: [1, [Validators.required, Validators.min(1)]],
      requiredQuantity: [1, [Validators.required, Validators.min(0.01)]]
    });
  }

  // ── Add-detail drawer ─────────────────────────────────────
  openAddDetailDrawer() {
    this.allBatches = [];
    this.batches = [];
    this.hiddenBatchCount = 0;
    this.units = [];
    this.selectedConversionFactor = 1;
    this.quantityPreview = 0;
    this.isCreatingBatch = false;
    this.selectedMedicineCondition = null;
    this.filteredBins = this.bins;
    this.hiddenBinCount = 0;
    this.detailForm.reset({ quantity: 1, conversionFactor: 1 });
    this.isAddDetailDrawerOpen = true;
  }

  closeAddDetailDrawer() {
    this.isAddDetailDrawerOpen = false;
    this.isCreatingBatch = false;
  }

  saveDetail() {
    if (this.detailForm.invalid) return;
    this.isSavingDetail = true;

    this.ticketService.createTicketDetail(this.ticketId, this.detailForm.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingDetail = false;
          this.closeAddDetailDrawer();
          this.loadTicketData();
          this.onSaved.emit();
        },
        error: () => { this.isSavingDetail = false; }
      });
  }

  removeDetail(detailId: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.ticketService.removeDetail(this.ticketId, detailId)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
              this.loadTicketData();
              this.onSaved.emit();
            });
        }
      });
  }

  // ── FEFO drawer ───────────────────────────────────────────
  openFefoDrawer() {
    this.fefoUnits = [];
    this.fefoConversionFactor = 1;
    this.fefoBaseQtyPreview = 0;
    this.fefoForm.reset({ requiredQuantity: 1, conversionFactor: 1 });
    this.isFefoDrawerOpen = true;
  }

  closeFefoDrawer() { this.isFefoDrawerOpen = false; }

  runFefo() {
    if (this.fefoForm.invalid) return;
    this.isRunningFefo = true;

    const { productId, requiredQuantity, conversionFactor } = this.fefoForm.value;
    const requiredBaseQuantity = requiredQuantity * (conversionFactor || 1);

    this.ticketService.allocateFEFO(this.ticketId, productId, requiredBaseQuantity)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isRunningFefo = false;
          this.closeFefoDrawer();
          this.loadTicketData();
          this.onSaved.emit();
        },
        error: () => { this.isRunningFefo = false; }
      });
  }

  // ── Ticket workflow ───────────────────────────────────────
  sendToApprove() {
    if (!this.ticket?.details?.length) {
      this.confirmation.error('::NoDataError', '::Error');
      return;
    }
    this.confirmation
      .info('::SendToApproveConfirmation', '::Confirm')
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) return;
        this.ticketService.sendToApprove(this.ticketId)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.loadTicketData();
            this.onSaved.emit();
          });
      });
  }

  reject() {
    this.rejectReason = '';
    this.showRejectError = false;
    this.isRejecting = false;
    this.modalService.open(this.rejectReasonModal, { size: 'md', centered: true, backdrop: 'static' });
  }

  confirmReject(modal: any) {
    if (!this.rejectReason || !this.rejectReason.trim()) {
      this.showRejectError = true;
      return;
    }

    this.isRejecting = true;
    this.ticketService.reject(this.ticketId, this.rejectReason.trim())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isRejecting = false;
          modal.close();
          this.toaster.success('::RejectSuccess', '::Success');
          this.loadTicketData();
          this.onSaved.emit();
        },
        error: () => {
          this.isRejecting = false;
        }
      });
  }

  execute() {
    this.confirmation
      .success('::ExecuteConfirmation', '::Confirm')
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) return;
        this.ticketService.execute(this.ticketId)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.loadTicketData();
            this.onSaved.emit();
          });
      });
  }

  // ── Helpers ───────────────────────────────────────────────
  isIssueTicket(): boolean {
    return this.ticket?.type === TicketType.GoodsIssue
      || this.ticket?.type === TicketType.DisposalIssue
      || this.ticket?.type === TicketType.ReturnOutward;
  }
}