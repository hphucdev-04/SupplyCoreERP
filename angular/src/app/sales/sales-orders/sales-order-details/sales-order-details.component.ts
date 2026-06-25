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
import { CustomerService } from 'src/app/proxy/customers';
import { InventoryTicketService } from 'src/app/proxy/tickets';
import { InventoryTicketDto } from 'src/app/proxy/tickets/dtos';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { MedicineService } from 'src/app/proxy/medicines';
import { PriceService } from 'src/app/proxy/prices';
import { MedicineDto } from 'src/app/proxy/medicines/dtos';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { ProductCostReferenceDto, ProductPriceDto } from 'src/app/proxy/prices/dtos';
import { SalesOrderStatus } from 'src/app/proxy/enums/orders/sales-order-status.enum';
import { ApprovalStatus } from 'src/app/proxy/enums/warehouses/approval-status.enum';
import { enumName } from 'src/app/shared/untils/enum.util';
import { UnitConversionHelper } from 'src/app/shared/untils/unit-conversion.helper';
import { CurrencyFormatDirective } from 'src/app/shared/directives/currency-format.directive';
import { PrintDocumentService } from 'src/app/shared/services/print-document.service';
import { DocumentPrintModel } from 'src/app/shared/models/document-print.model';

interface ProductUnitLookup {
  unitId: string;
  unitName: string;
  conversionFactor: number;
  isBaseUnit: boolean;
}

@Component({
  selector: 'app-sales-order-details',
  standalone: true,
  imports: [SharedModule, DrawerComponent, DropdownSearchComponent, CurrencyFormatDirective],
  templateUrl: './sales-order-details.component.html',
})
export class SalesOrderDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private readonly ROUTE_NAME = '::Menu:SaleOrderDetails';

  orderId: string;
  order: SalesOrderDto;
  relatedTickets: InventoryTicketDto[] = [];
  warehouses: WarehouseDto[] = [];
  medicines: MedicineDto[] = [];
  loading = true;

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
  referencePrices: ProductPriceDto[] = [];
  selectedConversionFactor = 1;
  baseUnitName = '';
  quantityPreview = 0;
  costReference: ProductCostReferenceDto | null = null;
  belowCostWarning: string | null = null;

  SalesOrderStatus = SalesOrderStatus;
  ApprovalStatus = ApprovalStatus;
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
    public router: Router,
    private customerService: CustomerService,
    private printDocumentService: PrintDocumentService,
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

  ngOnDestroy(): void {
    this.routesService.remove([this.ROUTE_NAME]);
    this.destroy$.next();
    this.destroy$.complete();
  }

  goBack() {
    this.router.navigate(['/sales/sales-orders']);
  }

  printDocument() {
    if (!this.order) return;
    this.printDocumentService.print(this.buildPrintModel());
  }

  // ── Data ─────────────────────────────────────────────────
  loadData() {
    this.loading = true;
    this.soService
      .get(this.orderId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.order = res;
          this.loading = false;

          this.routesService.add([
            {
              path: `/sales/sales-orders/details/${this.order.id}`,
              name: this.ROUTE_NAME,
              parentName: '::Menu:Sales-Orders',
              iconClass: 'fas fa-file-invoice',
              layout: eLayoutType.application,
              requiredPolicy: 'Order.SaleOrder',
            },
          ]);
        },
        error: () => this.goBack(),
      });

    this.ticketService
      .getRelatedTicketsBySaleOrder(this.orderId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.relatedTickets = res));
  }

  loadMasterData() {
    this.warehouseService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.warehouses = res.items));

    this.medicineService
      .getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.medicines = res.items));
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

    this.lineForm
      .get('unitPrice')
      ?.valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(() => this.updateBelowCostWarning());
  }

  calculateDueDate() {
    if (!this.order?.customerId || !this.order?.orderDate) return;

    this.customerService.get(this.order.customerId).subscribe(customer => {
      const days = customer.paymentTermDays || 0;
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

    // Tự động tính lại khi mở (để đảm bảo đồng bộ nếu cấu hình Khách hàng thay đổi)
    this.calculateDueDate();
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
    this.referencePrices = [];
    this.selectedConversionFactor = 1;
    this.quantityPreview = 0;
    this.costReference = null;
    this.belowCostWarning = null;
    this.lineForm.reset({
      quantity: 1,
      conversionFactor: 1,
      unitPrice: null,
      discountRate: 0,
      taxRate: 0,
    });
    this.isAddLineOpen = true;
  }

  closeAddLineDrawer() {
    this.isAddLineOpen = false;
  }

  onMedicineChange(medicineId: string) {
    this.lineForm.patchValue({ unitId: null, conversionFactor: 1, unitPrice: null });
    this.units = [];
    this.availablePrices = [];
    this.referencePrices = [];
    this.selectedConversionFactor = 1;
    this.baseUnitName = '';
    this.quantityPreview = 0;
    this.costReference = null;
    this.belowCostWarning = null;
    if (!medicineId) return;

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
        this.units = [baseUnit, ...others];
        this.baseUnitName = detail.baseUnitName ?? '';
        this.lineForm.patchValue({ unitId: baseUnit.unitId, conversionFactor: 1 });
        this.selectedConversionFactor = 1;

        this.loadPrices(medicineId);
        this.loadCostReference(medicineId, baseUnit.unitId);
      });
  }

  loadPrices(productId: string) {
    this.priceService
      .getByProduct(productId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(prices => {
        this.availablePrices = prices;
        this.filterAvailablePrices();
      });
  }

  onUnitChange(unitId: string) {
    const unit = this.units.find(u => u.unitId === unitId);
    if (unit) {
      this.selectedConversionFactor = unit.conversionFactor;
      this.lineForm.patchValue({ conversionFactor: unit.conversionFactor });
    }
    this.filterAvailablePrices();
    this.updateQuantityPreview();

    const productId = this.lineForm.get('productId')?.value;
    if (productId && unitId) {
      this.loadCostReference(productId, unitId);
    } else {
      this.costReference = null;
      this.belowCostWarning = null;
    }
  }

  filterAvailablePrices() {
    const unitId = this.lineForm.get('unitId')?.value;
    const qty = this.lineForm.get('quantity')?.value || 0;
    this.referencePrices = this.availablePrices.filter(
      p => p.unitId === unitId && qty >= (p.minQuantity || 0),
    );
  }

  updateQuantityPreview() {
    const qty = this.lineForm.get('quantity')?.value || 0;
    const unitId = this.lineForm.get('unitId')?.value;
    this.quantityPreview = UnitConversionHelper.convertToBaseQuantity(
      {
        baseUnitId: '',
        units: [{ unitId: unitId, conversionFactor: this.selectedConversionFactor }],
      },
      unitId,
      qty,
    );
    this.filterAvailablePrices();
  }

  loadCostReference(productId: string, unitId: string) {
    this.priceService
      .getCostReference(productId, unitId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.costReference = res;
          this.updateBelowCostWarning();
        },
        error: () => {
          this.costReference = null;
          this.belowCostWarning = null;
        },
      });
  }

  updateBelowCostWarning() {
    const rawUnitPrice = this.lineForm?.get('unitPrice')?.value;
    const lowestPurchasePrice = this.costReference?.lowestPurchasePrice;

    if (
      rawUnitPrice === '' ||
      rawUnitPrice === null ||
      rawUnitPrice === undefined ||
      lowestPurchasePrice === null ||
      lowestPurchasePrice === undefined
    ) {
      this.belowCostWarning = null;
      return;
    }

    const unitPrice = Number(rawUnitPrice);
    if (isNaN(unitPrice) || unitPrice >= lowestPurchasePrice) {
      this.belowCostWarning = null;
      return;
    }

    this.belowCostWarning =
      `Giá bán (${this.formatCurrency(unitPrice)}) thấp hơn giá nhập chuẩn thấp nhất ` +
      `(${this.formatCurrency(lowestPurchasePrice)}).`;
  }

  private formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN').format(value);
  }

  saveLine() {
    if (this.lineForm.invalid) return;
    this.isSavingLine = true;
    const rawUnitPrice = this.lineForm.get('unitPrice')?.value;
    const payload = {
      ...this.lineForm.getRawValue(),
      unitPrice:
        rawUnitPrice === '' || rawUnitPrice === null || rawUnitPrice === undefined
          ? null
          : Number(rawUnitPrice),
    };

    this.soService
      .addLine(this.orderId, payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingLine = false;
          this.closeAddLineDrawer();
          this.loadData();
        },
        error: err => {
          this.isSavingLine = false;
          const message =
            err.status === 403 && payload.unitPrice !== null
              ? 'Bạn không có quyền nhập giá bán thủ công.'
              : err.error?.error?.message || '::Error';
          const title = err.status === 403 ? 'Lỗi phân quyền' : '::Error';
          this.toaster.error(message, title);
        },
      });
  }

  // ── Inline edit ──
  onInlineLineChange(
    line: SalesOrderLineDto,
    field: 'quantity' | 'unitPrice' | 'discountRate' | 'taxRate',
    rawValue: string,
  ) {
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
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
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
    this.confirmation.info('::SendToApproveConfirmation', '::Confirm').subscribe(status => {
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
    this.confirmation.success('::ApproveSOConfirmation', '::Confirm').subscribe(status => {
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
    this.confirmation.success('::CompleteConfirmation', '::Confirm').subscribe(status => {
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

  // ── Helpers ───────────────────────────────────────────────
  isEditable(): boolean {
    return (
      this.order?.status === SalesOrderStatus.Draft ||
      this.order?.status === SalesOrderStatus.PendingApproval
    );
  }

  deliverProgress(line: SalesOrderLineDto): number {
    if (!line.quantity) return 0;
    return Math.min(100, ((line.deliveredQuantity ?? 0) / line.quantity) * 100);
  }

  statusClass(status: SalesOrderStatus): string {
    const map: Record<number, string> = {
      [SalesOrderStatus.Draft]: 'ph-badge--neutral',
      [SalesOrderStatus.PendingApproval]: 'ph-badge--pending',
      [SalesOrderStatus.Approved]: 'ph-badge--info',
      [SalesOrderStatus.Delivering]: 'ph-badge--pending',
      [SalesOrderStatus.Completed]: 'ph-badge--approved',
      [SalesOrderStatus.Canceled]: 'ph-badge--rejected',
    };
    return map[status] ?? 'ph-badge--neutral';
  }

  statusIcon(status: SalesOrderStatus): string {
    const map: Record<number, string> = {
      [SalesOrderStatus.Draft]: 'fa-pencil',
      [SalesOrderStatus.PendingApproval]: 'fa-clock-o',
      [SalesOrderStatus.Approved]: 'fa-check',
      [SalesOrderStatus.Delivering]: 'fa-road',
      [SalesOrderStatus.Completed]: 'fa-check-circle',
      [SalesOrderStatus.Canceled]: 'fa-times-circle',
    };
    return map[status] ?? 'fa-circle';
  }

  private buildPrintModel(): DocumentPrintModel {
    return {
      title: 'Hóa đơn bán hàng',
      documentNumber: this.order.code ?? '',
      printedAt: this.formatPrintDateTime(new Date().toISOString()),
      sections: [
        {
          title: 'Thông tin chung',
          columns: 2,
          fields: [
            { label: 'Khách hàng', value: this.order.customerName ?? '' },
            { label: 'Mã khách hàng', value: this.order.customerCode ?? '' },
            { label: 'Kho', value: this.order.warehouseName ?? '' },
            { label: 'Mã kho', value: this.order.warehouseCode ?? '' },
            { label: 'Ngày đơn', value: this.formatPrintDate(this.order.orderDate) },
            {
              label: 'Ngày giao dự kiến',
              value: this.formatPrintDate(this.order.expectedDeliveryDate),
            },
            { label: 'Hạn thanh toán', value: this.formatPrintDate(this.order.dueDate) },
            { label: 'Trạng thái', value: this.getStatusLabel(this.order.status) },
          ],
        },
      ],
      columns: [
        { key: 'index', header: 'STT', align: 'center', width: '44px' },
        { key: 'productCode', header: 'Mã hàng', width: '100px' },
        { key: 'productName', header: 'Tên hàng' },
        { key: 'unitName', header: 'ĐVT', align: 'center', width: '72px' },
        { key: 'quantity', header: 'SL', align: 'right', width: '72px' },
        { key: 'unitPrice', header: 'Đơn giá', align: 'right', width: '96px' },
        { key: 'discountRate', header: 'CK %', align: 'right', width: '68px' },
        { key: 'taxRate', header: 'Thuế %', align: 'right', width: '72px' },
        { key: 'finalPrice', header: 'Thành tiền', align: 'right', width: '110px' },
      ],
      rows: (this.order.lines ?? []).map((line, index) => ({
        index: index + 1,
        productCode: line.productCode ?? '',
        productName: line.productName ?? '',
        unitName: line.unitName ?? '',
        quantity: this.formatPrintNumber(line.quantity),
        unitPrice: this.formatPrintCurrency(line.unitPrice),
        discountRate: this.formatPrintPercent(line.discountRate),
        taxRate: this.formatPrintPercent(line.taxRate),
        finalPrice: this.formatPrintCurrency(line.finalPrice),
      })),
      summary: [
        { label: 'Tạm tính', value: this.formatPrintCurrency(this.order.subTotal) },
        { label: 'Chiết khấu', value: this.formatPrintCurrency(this.order.discountAmount) },
        { label: 'Thuế', value: this.formatPrintCurrency(this.order.taxAmount) },
        { label: 'Tổng cộng', value: this.formatPrintCurrency(this.order.totalAmount) },
      ],
      note: this.order.note ?? '',
      signatures: [{ label: 'Người lập' }, { label: 'Kế toán' }, { label: 'Khách hàng' }],
    };
  }

  private getStatusLabel(status?: SalesOrderStatus): string {
    switch (status) {
      case SalesOrderStatus.Draft:
        return 'Nháp';
      case SalesOrderStatus.PendingApproval:
        return 'Chờ duyệt';
      case SalesOrderStatus.Approved:
        return 'Đã duyệt';
      case SalesOrderStatus.Delivering:
        return 'Đang giao';
      case SalesOrderStatus.Completed:
        return 'Hoàn thành';
      case SalesOrderStatus.Canceled:
        return 'Đã hủy';
      default:
        return '';
    }
  }

  private formatPrintDate(value?: string | null): string {
    if (!value) return '';
    return new Intl.DateTimeFormat('vi-VN').format(new Date(value));
  }

  private formatPrintDateTime(value?: string | null): string {
    if (!value) return '';
    return new Intl.DateTimeFormat('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(value));
  }

  private formatPrintCurrency(value?: number | null): string {
    return new Intl.NumberFormat('vi-VN').format(value ?? 0) + ' đ';
  }

  private formatPrintNumber(value?: number | null): string {
    return new Intl.NumberFormat('vi-VN').format(value ?? 0);
  }

  private formatPrintPercent(value?: number | null): string {
    return `${value ?? 0}%`;
  }
}
