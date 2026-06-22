import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { Subject, forkJoin, lastValueFrom } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { SalesRecallService } from 'src/app/proxy/sales-recalls/sales-recall.service';
import {
  SalesRecallDto,
  CustomerRecallTraceDto,
  SalesRecallLineDto,
} from 'src/app/proxy/sales-recalls/dtos/models';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { SalesRecallStatus } from 'src/app/proxy/enums/orders/sales-recall-status.enum';
import { RecallLevel } from 'src/app/proxy/enums/orders/recall-level.enum';
import { enumName } from 'src/app/shared/untils/enum.util';

interface SelectableTrace extends CustomerRecallTraceDto {
  selected: boolean;
  recallQuantity: number;
  recallPrice: number;
  taxRate: number;
  unitId: string; // Thực tế cần đơn vị tính để insert
}

@Component({
  selector: 'app-sales-recall-details',
  standalone: true,
  imports: [SharedModule, DrawerComponent],
  templateUrl: './sales-recall-details.component.html',
  styleUrl: './sales-recall-details.component.scss',
})
export class SalesRecallDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private readonly ROUTE_NAME = '::Menu:SalesRecallDetails';

  recallId: string;
  recallDto: SalesRecallDto;
  warehouses: WarehouseDto[] = [];
  traceLines: SelectableTrace[] = [];
  loading = true;

  // Cần lưu tổng số lượng đã giao ra thị trường để tính % tiến độ
  totalDeliveredQty = 0;
  totalRequiredQty = 0;
  totalRecalledQty = 0;
  progressPercent = 0;

  // Edit master drawer
  isEditDrawerOpen = false;
  editForm: FormGroup;
  isSavingEdit = false;

  // Drawer Bottom truy vết
  isTraceOpen = false;
  isSavingLines = false;

  SalesRecallStatus = SalesRecallStatus;
  RecallLevel = RecallLevel;
  readonly enumName = enumName;

  constructor(
    private recallService: SalesRecallService,
    private warehouseService: WarehouseService,
    private routesService: RoutesService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.recallId = this.route.snapshot.params['id'];
    if (this.recallId) {
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
    this.router.navigate(['/sales/sales-recalls']);
  }

  // ── Data Loading ─────────────────────────────────────────
  loadData() {
    this.loading = true;
    this.recallService
      .get(this.recallId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.recallDto = res;
          this.loading = false;
          this.loadLookups();
          this.calculateProgress();

          this.routesService.add([
            {
              path: `/sales/sales-recalls/details/${this.recallDto.id}`,
              name: this.ROUTE_NAME,
              parentName: '::Menu:Sales-Recalls',
              iconClass: 'fas fa-file-invoice',
              layout: eLayoutType.application,
              requiredPolicy: 'Sales.SalesRecall',
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

    // Thực hiện gọi API truy vết khách hàng để làm dữ liệu nguồn cho Drawer Bottom
    if (this.recallDto?.productBatchId) {
      this.recallService
        .traceCustomersByBatch(this.recallDto.productBatchId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(traces => {
          // Tính tổng số lượng thuốc đã phát hành ra thị trường
          this.totalDeliveredQty = traces.reduce((acc, curr) => acc + curr.quantity, 0);

          this.traceLines = traces.map(trace => {
            // Check xem khách hàng + salesorder này đã có trong lines hiện tại của phiếu thu hồi chưa
            const existingLine = this.recallDto.lines.find(
              l => l.customerId === trace.customerId && l.salesOrderId === trace.salesOrderId,
            );
            return {
              ...trace,
              selected: !!existingLine,
              recallQuantity: existingLine ? existingLine.quantity : trace.quantity,
              recallPrice: existingLine ? existingLine.originalUnitPrice : (trace.unitPrice || 0),
              taxRate: existingLine ? existingLine.taxRate : (trace.taxRate || 0),
              unitId: trace.unitId,
            } as SelectableTrace;
          });
          this.calculateProgress();
        });
    }
  }

  calculateProgress() {
    if (!this.recallDto) return;
    
    // Tính tổng số lượng yêu cầu và thực tế thu hồi (theo đơn vị cơ bản)
    this.totalRequiredQty = this.recallDto.lines.reduce((acc, curr) => acc + (curr.baseQuantity || 0), 0);
    this.totalRecalledQty = this.recallDto.lines.reduce((acc, curr) => acc + (curr.recalledBaseQuantity || 0), 0);
    
    // Tính % tiến độ thực tế
    if (this.totalRequiredQty > 0) {
      this.progressPercent = Math.round((this.totalRecalledQty / this.totalRequiredQty) * 100);
    } else {
      this.progressPercent = 0;
    }
  }

  // ── Forms ─────────────────────────────────────────────────
  buildForms() {
    this.editForm = this.fb.group({
      recallDecisionNumber: ['', [Validators.required, Validators.maxLength(256)]],
      warehouseId: [null, [Validators.required]],
      recallDate: [null, [Validators.required]],
      level: [RecallLevel.Level3, [Validators.required]],
      note: ['', [Validators.maxLength(1000)]],
    });
  }

  // ── Edit Master ───────────────────────────────────────────
  openEditDrawer() {
    this.editForm.patchValue({
      recallDecisionNumber: this.recallDto.recallDecisionNumber,
      warehouseId: this.recallDto.warehouseId,
      recallDate: this.recallDto.recallDate?.split('T')[0] ?? null,
      level: this.recallDto.level,
      note: this.recallDto.note ?? '',
    });
    this.isEditDrawerOpen = true;
  }

  closeEditDrawer() {
    this.isEditDrawerOpen = false;
  }

  saveEdit() {
    if (this.editForm.invalid) return;
    this.isSavingEdit = true;
    this.recallService
      .update(this.recallId, this.editForm.value)
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

  // ── Drawer Bottom: Trace & Suggest ────────────────────────
  openTraceDrawer() {
    this.loadData();
    this.isTraceOpen = true;
  }

  closeTraceDrawer() {
    this.isTraceOpen = false;
  }

  async saveTraceLines() {
    const selectedLines = this.traceLines.filter(line => line.selected);
    if (selectedLines.length === 0) {
      this.toaster.warn('::PleaseSelectAtLeastOneLine', '::Warning');
      return;
    }

    // Validate số lượng
    for (const line of selectedLines) {
      if (line.recallQuantity <= 0) {
        this.toaster.error('::QuantityMustBeGreaterThanZero', '::Error');
        return;
      }
      if (line.recallQuantity > line.quantity) {
        this.toaster.error('::RecallQtyCannotExceedDeliveredQty', '::Error');
        return;
      }
    }

    this.isSavingLines = true;

    try {
      // Thực hiện gửi API addLine tuần tự để tránh lỗi ConcurrencyStamp của EF Core
      for (const line of selectedLines) {
        const request$ = this.recallService.addLine(this.recallId, {
          customerId: line.customerId,
          salesOrderId: line.salesOrderId,
          unitId: line.unitId,
          conversionFactor: line.conversionFactor || 1,
          quantity: line.recallQuantity,
          originalUnitPrice: line.recallPrice || 0,
          taxRate: line.taxRate || 0,
        });
        await lastValueFrom(request$);
      }

      this.isSavingLines = false;
      this.closeTraceDrawer();
      this.loadData();
      this.toaster.success('::TraceSaveSuccess', '::Success');
    } catch (error) {
      this.isSavingLines = false;
      console.error(error);
    }
  }

  removeLine(lineId: string) {
    this.confirmation.warn('::AreYouSureToDeleteLine', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.recallService
          .removeLine(this.recallId, lineId)
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
    this.confirmation.info('::ConfirmSendToApprove', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.recallService.sendToApprove(this.recallId).subscribe(() => {
          this.loadData();
          this.toaster.success('::SendToApproveSuccess', '::Success');
        });
      }
    });
  }

  approve() {
    this.confirmation.info('::ConfirmApproveRecall', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.recallService.approve(this.recallId).subscribe(() => {
          this.loadData();
          this.toaster.success('::ApproveSuccess', '::Success');
        });
      }
    });
  }

  reject() {
    this.confirmation.warn('::ConfirmRejectRecall', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.recallService.reject(this.recallId).subscribe(() => {
          this.loadData();
          this.toaster.success('::RejectSuccess', '::Success');
        });
      }
    });
  }

  statusClass(status: SalesRecallStatus): string {
    const map: Record<number, string> = {
      [SalesRecallStatus.Draft]: 'ph-badge--neutral',
      [SalesRecallStatus.PendingApproval]: 'ph-badge--pending',
      [SalesRecallStatus.Approved]: 'ph-badge--info',
      [SalesRecallStatus.Recalling]: 'ph-badge--pending',
      [SalesRecallStatus.Completed]: 'ph-badge--approved',
      [SalesRecallStatus.Rejected]: 'ph-badge--rejected',
    };
    return map[status] ?? 'ph-badge--neutral';
  }

  statusIcon(status: SalesRecallStatus): string {
    const map: Record<number, string> = {
      [SalesRecallStatus.Draft]: 'fa-pencil',
      [SalesRecallStatus.PendingApproval]: 'fa-clock-o',
      [SalesRecallStatus.Approved]: 'fa-check',
      [SalesRecallStatus.Recalling]: 'fa-refresh',
      [SalesRecallStatus.Completed]: 'fa-check-circle',
      [SalesRecallStatus.Rejected]: 'fa-times-circle',
    };
    return map[status] ?? 'fa-circle';
  }

  levelClass(level: RecallLevel): string {
    const map: Record<number, string> = {
      [RecallLevel.Level1]: 'ph-badge--rejected',
      [RecallLevel.Level2]: 'ph-badge--pending',
      [RecallLevel.Level3]: 'ph-badge--info',
    };
    return map[level] ?? 'ph-badge--neutral';
  }

  levelIcon(level: RecallLevel): string {
    const map: Record<number, string> = {
      [RecallLevel.Level1]: 'fa-radiation',
      [RecallLevel.Level2]: 'fa-exclamation-triangle',
      [RecallLevel.Level3]: 'fa-info-circle',
    };
    return map[level] ?? 'fa-circle';
  }
}
