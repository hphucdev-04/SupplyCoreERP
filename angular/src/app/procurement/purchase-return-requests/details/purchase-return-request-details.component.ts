import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { Subject, forkJoin, of } from 'rxjs';
import { takeUntil, switchMap, catchError } from 'rxjs/operators';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { DropdownSearchComponent } from 'src/app/shared/components/dropdownsearch-component/dropdown-search.component';
import { PurchaseReturnRequestService } from 'src/app/proxy/purchase-return-requests';
import { PurchaseReturnRequestDto } from 'src/app/proxy/purchase-return-requests/dtos';
import { PurchaseOrderService } from 'src/app/proxy/purchase-orders';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { PurchaseReturnRequestStatus } from 'src/app/proxy/enums/orders/purchase-return-request-status.enum';
import { PurchaseReturnStatus } from 'src/app/proxy/enums/orders/purchase-return-status.enum';
import { PurchaseReturnType, purchaseReturnTypeOptions } from 'src/app/proxy/enums/orders/purchase-return-type.enum';
import { enumName } from 'src/app/shared/untils/enum.util';

interface SelectablePOLine {
  id: string;
  purchaseOrderId: string;
  purchaseOrderCode: string;
  productId: string;
  productCode: string;
  productName: string;
  unitId: string;
  unitName: string;
  conversionFactor: number;
  receivedQuantity: number;
  unitPrice: number;
  taxRate: number;
  selected: boolean;
  returnQuantity: number;
  depreciationRate: number;
  supplierName?: string;
  supplierCode?: string;
  returnType?: PurchaseReturnType;
}

@Component({
  selector: 'app-purchase-return-request-details',
  standalone: true,
  imports: [SharedModule, DrawerComponent, DropdownSearchComponent],
  templateUrl: './purchase-return-request-details.component.html',
  styleUrls: ['./purchase-return-request-details.component.scss'],
})
export class PurchaseReturnRequestDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private readonly ROUTE_NAME = '::Menu:PurchaseReturnRequestDetails';
  private readonly requestService = inject(PurchaseReturnRequestService);
  private readonly poService = inject(PurchaseOrderService);
  private readonly warehouseService = inject(WarehouseService);
  private readonly routesService = inject(RoutesService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly toaster = inject(ToasterService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  requestId: string;
  requestDto: PurchaseReturnRequestDto;
  warehouses: WarehouseDto[] = [];
  poLines: SelectablePOLine[] = [];
  loading = true;
  loadingLines = false;

  // Edit master drawer
  isEditDrawerOpen = false;
  editForm: FormGroup;
  isSavingEdit = false;

  // Add lines drawer bottom
  isAddLineOpen = false;
  isSavingLines = false;

  PurchaseReturnRequestStatus = PurchaseReturnRequestStatus;
  PurchaseReturnStatus = PurchaseReturnStatus;
  PurchaseReturnType = PurchaseReturnType;
  purchaseReturnTypeOptions = purchaseReturnTypeOptions;
  readonly enumName = enumName;

  ngOnInit(): void {
    this.requestId = this.route.snapshot.params['id'];
    if (this.requestId) {
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
    this.router.navigate(['/procurement/purchase-return-requests']);
  }

  // ── Data Loading ─────────────────────────────────────────
  loadData() {
    this.loading = true;
    this.requestService
      .get(this.requestId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.requestDto = res;
          this.loading = false;
          this.loadLookups();

          this.routesService.add([
            {
              path: `/procurement/purchase-return-requests/details/${this.requestDto.id}`,
              name: this.ROUTE_NAME,
              parentName: '::Menu:Purchase-Return-Requests',
              iconClass: 'fas fa-file-invoice',
              layout: eLayoutType.application,
              requiredPolicy: 'Procurement.PurchaseReturnRequest',
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
  }

  loadAvailablePOLines() {
    if (!this.requestDto?.warehouseId) return;

    this.loadingLines = true;
    this.poService
      .getList({
        maxResultCount: 1000,
        warehouseId: this.requestDto.warehouseId,
      })
      .pipe(
        takeUntil(this.destroy$),
        switchMap(res => {
          // Lọc các PO ở trạng thái Approved (3) hoặc Completed (5)
          const validPOs = res.items.filter(po => po.status === 3 || po.status === 5);
          if (!validPOs.length) {
            return of([]);
          }
          // Fetch chi tiết của từng PO để lấy các dòng lines
          const poRequests = validPOs.map(po =>
            this.poService.get(po.id).pipe(
              catchError(() => of(null))
            )
          );
          return forkJoin(poRequests);
        })
      )
      .subscribe({
        next: poDetailsList => {
          const lines: SelectablePOLine[] = [];
          
          poDetailsList.forEach(po => {
            if (!po) return;
            po.lines?.forEach(line => {
              if ((line.receivedQuantity || 0) > 0) {
                const existingLine = this.requestDto.lines?.find(
                  l => l.purchaseOrderLineId === line.id
                );
                
                lines.push({
                  id: line.id,
                  purchaseOrderId: po.id,
                  purchaseOrderCode: po.code || '',
                  productId: line.productId || '',
                  productCode: line.productCode || '',
                  productName: line.productName || '',
                  unitId: line.unitId || '',
                  unitName: line.unitName || '',
                  conversionFactor: line.conversionFactor || 1,
                  receivedQuantity: line.receivedQuantity || 0,
                  unitPrice: line.unitPrice || 0,
                  taxRate: line.taxRate || 0,
                  selected: !!existingLine,
                  returnQuantity: existingLine ? existingLine.quantity : line.receivedQuantity,
                  depreciationRate: existingLine ? existingLine.depreciationRate : 0,
                  supplierName: po.supplierName || '',
                  supplierCode: po.supplierCode || '',
                  returnType: existingLine ? existingLine.returnType : PurchaseReturnType.Defective,
                });
              }
            });
          });

          this.poLines = lines;
          this.loadingLines = false;
        },
        error: () => {
          this.loadingLines = false;
          this.toaster.error('::ErrorLoadingPOLines', '::Error');
        }
      });
  }

  // ── Forms ─────────────────────────────────────────────────
  buildForms() {
    this.editForm = this.fb.group({
      warehouseId: [null, [Validators.required]],
      requestDate: [null, [Validators.required]],
      note: ['', [Validators.maxLength(1000)]],
    });
  }

  // ── Edit Master ───────────────────────────────────────────
  openEditDrawer() {
    this.editForm.patchValue({
      warehouseId: this.requestDto.warehouseId,
      requestDate: this.requestDto.requestDate?.split('T')[0] ?? null,
      note: this.requestDto.note ?? '',
    });
    this.isEditDrawerOpen = true;
  }

  closeEditDrawer() {
    this.isEditDrawerOpen = false;
  }

  saveEdit() {
    if (this.editForm.invalid) return;
    this.isSavingEdit = true;
    this.requestService
      .update(this.requestId, this.editForm.value)
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
    this.loadAvailablePOLines();
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

    // Validate số lượng và khấu hao
    for (const line of selectedLines) {
      if (line.returnQuantity <= 0) {
        this.toaster.error('::QuantityMustBeGreaterThanZero', '::Error');
        return;
      }
      if (line.returnQuantity > line.receivedQuantity) {
        this.toaster.error('::ReturnQtyCannotExceedReceivedQty', '::Error');
        return;
      }
      if (line.returnType === PurchaseReturnType.Defective && line.depreciationRate !== 0) {
        this.toaster.error('::DefectiveCannotHaveDepreciation', '::Error');
        return;
      }
      if (line.depreciationRate < 0 || line.depreciationRate > 100) {
        this.toaster.error('::DepreciationRateMustBeBetween0And100', '::Error');
        return;
      }
    }

    this.isSavingLines = true;

    // Để đơn giản và nhất quán: gọi API addLine cho từng dòng được chọn chưa có trong db
    // Hoặc add tất cả, backend sẽ báo lỗi trùng nếu dòng đó đã có. Để chuyên nghiệp, ta chỉ gọi addLine
    // đối với các dòng chưa tồn tại trong requestDto.lines
    const requests = selectedLines.map(line => {
      const isExisting = this.requestDto.lines?.some(l => l.purchaseOrderLineId === line.id);
      if (isExisting) {
        // Nếu đã tồn tại, ta gọi updateLine
        const existingLine = this.requestDto.lines.find(l => l.purchaseOrderLineId === line.id);
        return this.requestService.updateLine(this.requestId, existingLine.id, {
          quantity: line.returnQuantity,
          depreciationRate: line.returnType === PurchaseReturnType.Defective ? 0 : line.depreciationRate,
          returnType: line.returnType
        });
      } else {
        // Nếu chưa tồn tại, gọi addLine
        return this.requestService.addLine(this.requestId, {
          productId: line.productId,
          unitId: line.unitId,
          conversionFactor: line.conversionFactor,
          purchaseOrderId: line.purchaseOrderId,
          purchaseOrderLineId: line.id,
          quantity: line.returnQuantity,
          originalUnitPrice: line.unitPrice,
          depreciationRate: line.returnType === PurchaseReturnType.Defective ? 0 : line.depreciationRate,
          taxRate: line.taxRate,
          returnType: line.returnType
        });
      }
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
          this.requestService
            .removeLine(this.requestId, lineId)
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
    if (!this.requestDto.lines?.length) {
      this.toaster.warn('::EmptyLines', '::Warning');
      return;
    }
    
    this.confirmation
      .info('::ConfirmSendToApprove', '::AreYouSure')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.requestService.sendToApprove(this.requestId).subscribe(() => {
            this.loadData();
            this.toaster.success('::SendToApproveSuccess', '::Success');
          });
        }
      });
  }

  approveAndSplit() {
    this.confirmation
      .info('::ConfirmApproveAndSplit', '::AreYouSure')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.requestService.approveAndSplit(this.requestId).subscribe(() => {
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
          this.requestService.reject(this.requestId).subscribe(() => {
            this.loadData();
            this.toaster.success('::RejectSuccess', '::Success');
          });
        }
      });
  }

  viewRelatedTicket(ticketId: string) {
    this.router.navigate(['/procurement/purchase-returns/details', ticketId]);
  }

  isEditable(): boolean {
    return (
      this.requestDto?.status === PurchaseReturnRequestStatus.Draft ||
      this.requestDto?.status === PurchaseReturnRequestStatus.PendingApproval
    );
  }

  canSendToApprove(): boolean {
    return this.requestDto?.status === PurchaseReturnRequestStatus.Draft && !!this.requestDto?.lines?.length;
  }

  statusClass(status: PurchaseReturnRequestStatus): string {
    const map: Record<number, string> = {
      [PurchaseReturnRequestStatus.Draft]: 'ph-badge--neutral',
      [PurchaseReturnRequestStatus.PendingApproval]: 'ph-badge--pending',
      [PurchaseReturnRequestStatus.Approved]: 'ph-badge--info',
      [PurchaseReturnRequestStatus.Rejected]: 'ph-badge--rejected',
      [PurchaseReturnRequestStatus.Processed]: 'ph-badge--approved',
    };
    return map[status] ?? 'ph-badge--neutral';
  }

  statusIcon(status: PurchaseReturnRequestStatus): string {
    const map: Record<number, string> = {
      [PurchaseReturnRequestStatus.Draft]: 'fa-pencil',
      [PurchaseReturnRequestStatus.PendingApproval]: 'fa-clock-o',
      [PurchaseReturnRequestStatus.Approved]: 'fa-check',
      [PurchaseReturnRequestStatus.Rejected]: 'fa-times-circle',
      [PurchaseReturnRequestStatus.Processed]: 'fa-check-circle',
    };
    return map[status] ?? 'fa-circle';
  }

  returnTypeClass(type: PurchaseReturnType): string {
    const map: Record<number, string> = {
      [PurchaseReturnType.Defective]: 'ph-badge--rejected',
      [PurchaseReturnType.Commercial]: 'ph-badge--info',
    };
    return map[type] ?? 'ph-badge--neutral';
  }

  relatedTicketStatusClass(status: number): string {
    const map: Record<number, string> = {
      1: 'ph-badge--neutral',
      2: 'ph-badge--pending',
      3: 'ph-badge--info',
      4: 'ph-badge--primary',
      5: 'ph-badge--approved',
      6: 'ph-badge--rejected',
    };
    return map[status] ?? 'ph-badge--neutral';
  }

  relatedTicketStatusIcon(status: number): string {
    const map: Record<number, string> = {
      1: 'fa-pencil',
      2: 'fa-clock-o',
      3: 'fa-check',
      4: 'fa-truck',
      5: 'fa-check-circle',
      6: 'fa-times-circle',
    };
    return map[status] ?? 'fa-circle';
  }
}
