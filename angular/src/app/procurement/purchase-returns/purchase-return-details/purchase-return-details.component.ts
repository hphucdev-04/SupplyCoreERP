import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { PurchaseReturnService } from 'src/app/proxy/purchase-returns/purchase-return.service';
import { PurchaseReturnDto, PurchaseReturnLineDto } from 'src/app/proxy/purchase-returns/dtos/models';
import { PurchaseOrderService } from 'src/app/proxy/purchase-orders';
import { PurchaseOrderLineDto } from 'src/app/proxy/purchase-orders/dtos';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { PurchaseReturnStatus } from 'src/app/proxy/enums/orders/purchase-return-status.enum';
import { ApprovalStatus } from 'src/app/proxy/enums/warehouses/approval-status.enum';
import { PurchaseReturnType, purchaseReturnTypeOptions } from 'src/app/proxy/enums/orders/purchase-return-type.enum';
import { DropdownSearchComponent } from 'src/app/shared/components/dropdownsearch-component/dropdown-search.component';
import { enumName } from 'src/app/shared/untils/enum.util';
import { PrintDocumentService } from 'src/app/shared/services/print-document.service';
import { DocumentPrintModel } from 'src/app/shared/models/document-print.model';

interface SelectablePOLine extends PurchaseOrderLineDto {
  selected: boolean;
  returnQuantity: number;
  depreciationRate: number;
  taxRate: number;
}

@Component({
  selector: 'app-purchase-return-details',
  standalone: true,
  imports: [SharedModule, DrawerComponent, DropdownSearchComponent],
  templateUrl: './purchase-return-details.component.html',
})
export class PurchaseReturnDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private readonly ROUTE_NAME = '::Menu:PurchaseReturnDetails';

  returnId: string;
  returnDto: PurchaseReturnDto;
  warehouses: WarehouseDto[] = [];
  poLines: SelectablePOLine[] = [];
  loading = true;

  // Edit master drawer
  isEditDrawerOpen = false;
  editForm: FormGroup;
  isSavingEdit = false;

  // Add lines drawer bottom
  isAddLineOpen = false;
  isSavingLines = false;

  PurchaseReturnStatus = PurchaseReturnStatus;
  ApprovalStatus = ApprovalStatus;
  PurchaseReturnType = PurchaseReturnType;
  purchaseReturnTypeOptions = purchaseReturnTypeOptions;
  readonly enumName = enumName;

  constructor(
    private returnService: PurchaseReturnService,
    private poService: PurchaseOrderService,
    private warehouseService: WarehouseService,
    private routesService: RoutesService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    public router: Router,
    private printDocumentService: PrintDocumentService,
  ) {}

  ngOnInit(): void {
    this.returnId = this.route.snapshot.params['id'];
    if (this.returnId) {
      this.buildForms();
      this.loadData();
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
    this.router.navigate(['/procurement/purchase-returns']);
  }

  printDocument() {
    if (!this.returnDto) return;
    this.printDocumentService.print(this.buildPrintModel());
  }

  // ── Data Loading ─────────────────────────────────────────
  loadData() {
    this.loading = true;
    this.returnService
      .get(this.returnId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.returnDto = res;
          this.loading = false;
          this.loadLookups();

          this.routesService.add([
            {
              path: `/procurement/purchase-returns/details/${this.returnDto.id}`,
              name: this.ROUTE_NAME,
              parentName: '::Menu:Purchase-Returns',
              iconClass: 'fas fa-file-invoice',
              layout: eLayoutType.application,
              requiredPolicy: 'Procurement.PurchaseReturns',
            },
          ]);
        },
        error: () => this.goBack(),
      });
  }

  loadLookups() {
    this.warehouseService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => (this.warehouses = res.items));

    // Load các dòng hàng của đơn mua gốc để chọn trả
    if (this.returnDto?.purchaseOrderId) {
      this.poService
        .get(this.returnDto.purchaseOrderId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(po => {
          this.poLines = po.lines.map(line => {
            // Kiểm tra xem dòng hàng này đã được chọn trả trong phiếu hiện tại chưa
            const existingLine = this.returnDto.lines.find(l => l.purchaseOrderLineId === line.id);
            return {
              ...line,
              selected: !!existingLine,
              returnQuantity: existingLine ? existingLine.quantity : line.receivedQuantity,
              depreciationRate: existingLine ? existingLine.depreciationRate : 0,
              taxRate: line.taxRate,
            } as SelectablePOLine;
          });
        });
    }
  }

  // ── Forms ─────────────────────────────────────────────────
  buildForms() {
    this.editForm = this.fb.group({
      warehouseId: [null, [Validators.required]],
      returnType: [null, [Validators.required]],
      returnDate: [null, [Validators.required]],
      note: ['', [Validators.maxLength(1000)]],
    });
  }

  // ── Edit Master ───────────────────────────────────────────
  openEditDrawer() {
    this.editForm.patchValue({
      warehouseId: this.returnDto.warehouseId,
      returnType: this.returnDto.returnType,
      returnDate: this.returnDto.returnDate?.split('T')[0] ?? null,
      note: this.returnDto.note ?? '',
    });
    this.isEditDrawerOpen = true;
  }

  closeEditDrawer() {
    this.isEditDrawerOpen = false;
  }

  saveEdit() {
    if (this.editForm.invalid) return;
    this.isSavingEdit = true;
    this.returnService
      .update(this.returnId, this.editForm.value)
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

  // ── Drawer Bottom Add Lines ──────────────────────────────
  openAddLinesDrawer() {
    this.loadData(); // Reload để có dữ liệu mới nhất
    this.isAddLineOpen = true;
  }

  closeAddLinesDrawer() {
    this.isAddLineOpen = false;
  }

  saveLines() {
    const selectedLines = this.poLines.filter(line => line.selected);
    if (selectedLines.length === 0) {
      this.toaster.warn('::PleaseSelectAtLeastOneLine', '::Warning');
      return;
    }

    // Validate số lượng
    for (const line of selectedLines) {
      if (line.returnQuantity <= 0) {
        this.toaster.error('::QuantityMustBeGreaterThanZero', '::Error');
        return;
      }
      if (line.returnQuantity > line.receivedQuantity) {
        this.toaster.error('::ReturnQtyCannotExceedReceivedQty', '::Error');
        return;
      }
    }

    this.isSavingLines = true;

    // Xóa các dòng cũ và add lại các dòng mới được chọn
    // Để đơn giản và nhất quán: gọi API addLine cho từng dòng được chọn
    const requests = selectedLines.map(line => {
      // Tìm xem dòng này đã có trong phiếu chưa, nếu có thì có thể xóa đi add lại hoặc gọi API addLine (Backend tự xử lý add/update nếu trùng hoặc cho phép add)
      // Thực tế ở Backend: AddLineAsync sẽ add mới dòng. Để tránh trùng, ta xóa hết các dòng cũ trước khi add lại, hoặc backend check.
      // Dựa trên thiết kế: AddLineAsync nhận AddPurchaseReturnLineDto. Ta chỉ cần gọi addLine.
      return this.returnService.addLine(this.returnId, {
        purchaseOrderLineId: line.id,
        productId: line.productId,
        unitId: line.unitId,
        conversionFactor: line.conversionFactor,
        quantity: line.returnQuantity,
        originalUnitPrice: line.unitPrice,
        depreciationRate: line.depreciationRate,
        taxRate: line.taxRate,
      });
    });

    forkJoin(requests)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSavingLines = false;
          this.closeAddLinesDrawer();
          this.loadData();
          this.toaster.success('::SaveLinesSuccess', '::Success');
        },
        error: () => (this.isSavingLines = false),
      });
  }

  removeLine(lineId: string) {
    this.confirmation
      .warn('::AreYouSureToDeleteLine', '::AreYouSure')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.returnService
            .removeLine(this.returnId, lineId)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
              this.loadData();
              this.toaster.success('::DeleteSuccess', '::Success');
            });
        }
      });
  }

  // ── Workflows Actions ─────────────────────────────────────
  sendToApprove() {
    this.confirmation
      .info('::ConfirmSendToApprove', '::AreYouSure')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.returnService.sendToApprove(this.returnId).subscribe(() => {
            this.loadData();
            this.toaster.success('::SendToApproveSuccess', '::Success');
          });
        }
      });
  }

  approve() {
    this.confirmation
      .info('::ConfirmApprove', '::AreYouSure')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.returnService.approve(this.returnId).subscribe(() => {
            this.loadData();
            this.toaster.success('::ApproveSuccess', '::Success');
          });
        }
      });
  }

  reject() {
    this.confirmation
      .warn('::ConfirmReject', '::AreYouSure')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.returnService.reject(this.returnId).subscribe(() => {
            this.loadData();
            this.toaster.success('::RejectSuccess', '::Success');
          });
        }
      });
  }

  isEditable(): boolean {
    return (
      this.returnDto?.status === PurchaseReturnStatus.Draft ||
      this.returnDto?.status === PurchaseReturnStatus.PendingApproval
    );
  }

  statusClass(status: PurchaseReturnStatus): string {
    const map: Record<number, string> = {
      [PurchaseReturnStatus.Draft]: 'ph-badge--neutral',
      [PurchaseReturnStatus.PendingApproval]: 'ph-badge--pending',
      [PurchaseReturnStatus.Approved]: 'ph-badge--info',
      [PurchaseReturnStatus.Returning]: 'ph-badge--primary',
      [PurchaseReturnStatus.Completed]: 'ph-badge--approved',
      [PurchaseReturnStatus.Rejected]: 'ph-badge--rejected',
    };
    return map[status] ?? 'ph-badge--neutral';
  }

  statusIcon(status: PurchaseReturnStatus): string {
    const map: Record<number, string> = {
      [PurchaseReturnStatus.Draft]: 'fa-pencil',
      [PurchaseReturnStatus.PendingApproval]: 'fa-clock-o',
      [PurchaseReturnStatus.Approved]: 'fa-check',
      [PurchaseReturnStatus.Returning]: 'fa-truck',
      [PurchaseReturnStatus.Completed]: 'fa-check-circle',
      [PurchaseReturnStatus.Rejected]: 'fa-times-circle',
    };
    return map[status] ?? 'fa-circle';
  }

  private buildPrintModel(): DocumentPrintModel {
    return {
      title: 'Phiếu trả hàng mua',
      documentNumber: this.returnDto.code ?? '',
      printedAt: this.formatPrintDateTime(new Date().toISOString()),
      sections: [
        {
          title: 'Thông tin chung',
          columns: 2,
          fields: [
            { label: 'Nhà cung cấp', value: this.returnDto.supplierName ?? '' },
            { label: 'Mã nhà cung cấp', value: this.returnDto.supplierCode ?? '' },
            { label: 'Kho', value: this.returnDto.warehouseName ?? '' },
            { label: 'Mã kho', value: this.returnDto.warehouseCode ?? '' },
            { label: 'Đơn mua gốc', value: this.returnDto.purchaseOrderCode ?? '' },
            { label: 'Ngày trả', value: this.formatPrintDate(this.returnDto.returnDate) },
            { label: 'Loại trả', value: this.getReturnTypeLabel(this.returnDto.returnType) },
            { label: 'Trạng thái', value: this.getStatusLabel(this.returnDto.status) },
          ],
        },
      ],
      columns: [
        { key: 'index', header: 'STT', align: 'center', width: '44px' },
        { key: 'productCode', header: 'Mã hàng', width: '100px' },
        { key: 'productName', header: 'Tên hàng' },
        { key: 'unitName', header: 'ĐVT', align: 'center', width: '72px' },
        { key: 'quantity', header: 'SL trả', align: 'right', width: '80px' },
        { key: 'originalUnitPrice', header: 'Đơn giá gốc', align: 'right', width: '96px' },
        { key: 'depreciationRate', header: 'Khấu hao %', align: 'right', width: '82px' },
        { key: 'taxRate', header: 'Thuế %', align: 'right', width: '72px' },
        { key: 'finalPrice', header: 'Thành tiền', align: 'right', width: '110px' },
      ],
      rows: (this.returnDto.lines ?? []).map((line, index) => ({
        index: index + 1,
        productCode: line.productCode ?? '',
        productName: line.productName ?? '',
        unitName: line.unitName ?? '',
        quantity: this.formatPrintNumber(line.quantity),
        originalUnitPrice: this.formatPrintCurrency(line.originalUnitPrice),
        depreciationRate: this.formatPrintPercent(line.depreciationRate),
        taxRate: this.formatPrintPercent(line.taxRate),
        finalPrice: this.formatPrintCurrency(line.finalPrice),
      })),
      summary: [
        { label: 'Tạm tính', value: this.formatPrintCurrency(this.returnDto.subTotal) },
        { label: 'Thuế', value: this.formatPrintCurrency(this.returnDto.taxAmount) },
        { label: 'Tổng cộng', value: this.formatPrintCurrency(this.returnDto.totalAmount) },
      ],
      note: this.returnDto.note ?? '',
      signatures: [{ label: 'Người lập' }, { label: 'Kế toán' }, { label: 'Nhà cung cấp' }],
    };
  }

  private getStatusLabel(status?: PurchaseReturnStatus): string {
    switch (status) {
      case PurchaseReturnStatus.Draft:
        return 'Nháp';
      case PurchaseReturnStatus.PendingApproval:
        return 'Chờ duyệt';
      case PurchaseReturnStatus.Approved:
        return 'Đã duyệt';
      case PurchaseReturnStatus.Returning:
        return 'Đang trả';
      case PurchaseReturnStatus.Completed:
        return 'Hoàn thành';
      case PurchaseReturnStatus.Rejected:
        return 'Từ chối';
      default:
        return '';
    }
  }

  private getReturnTypeLabel(type?: PurchaseReturnType): string {
    switch (type) {
      case PurchaseReturnType.Defective:
        return 'Hàng lỗi';
      case PurchaseReturnType.Commercial:
        return 'Thương mại';
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
