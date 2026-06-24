import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { TicketType, ticketTypeOptions } from 'src/app/proxy/enums/warehouses/ticket-type.enum';
import {
  ApprovalStatus,
  approvalStatusOptions,
} from 'src/app/proxy/enums/warehouses/approval-status.enum';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search-component/search.component';
import { InventoryTicketDto } from 'src/app/proxy/tickets/dtos';
import { InventoryTicketService } from 'src/app/proxy/tickets';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { PurchaseOrderService } from 'src/app/proxy/purchase-orders';
import { PurchaseOrderDto } from 'src/app/proxy/purchase-orders/dtos';
import { PurchaseOrderStatus } from 'src/app/proxy/enums/orders/purchase-order-status.enum';
import { enumName } from 'src/app/shared/untils/enum.util';
import { DropdownSearchComponent } from 'src/app/shared/components/dropdownsearch-component/dropdown-search.component';

import { SalesOrderService } from 'src/app/proxy/sales-orders';
import { SalesOrderDto } from 'src/app/proxy/sales-orders/dtos';
import { SalesOrderStatus } from 'src/app/proxy/enums/orders/sales-order-status.enum';
import { SalesRecallService } from 'src/app/proxy/sales-recalls/sales-recall.service';
import { PurchaseReturnService } from 'src/app/proxy/purchase-returns/purchase-return.service';
import { SalesRecallStatus } from 'src/app/proxy/enums/orders/sales-recall-status.enum';
import { PurchaseReturnStatus } from 'src/app/proxy/enums/orders/purchase-return-status.enum';

@Component({
  selector: 'app-inventory-tickets',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, DropdownSearchComponent],
  providers: [ListService],
  templateUrl: './tickets.component.html',
  styleUrls: ['./tickets.component.scss'],
})
export class TicketsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  data = { items: [], totalCount: 0 } as PagedResultDto<InventoryTicketDto>;
  warehouses: WarehouseDto[] = [];
  approvedPurchaseOrders: any[] = [];
  approvedSalesOrders: any[] = [];
  activeSalesRecalls: any[] = [];
  activePurchaseReturns: any[] = [];
  selectableReferences: any[] = [];

  // Drawer state
  isDrawerOpen = false;
  form: FormGroup;
  isSaving = false;

  // Filters
  filterText = '';
  filterType: number = null;
  filterStatus: number = null;
  filterWarehouseId: string = null;

  // Dropdown Data & Enums
  ticketTypeOptions = ticketTypeOptions;
  approvalStatusOptions = approvalStatusOptions;
  TicketType = TicketType;
  ApprovalStatus = ApprovalStatus;
  readonly enumName = enumName;

  constructor(
    public readonly list: ListService,
    private ticketService: InventoryTicketService,
    private warehouseService: WarehouseService,
    private poService: PurchaseOrderService,
    private soService: SalesOrderService,
    private recallService: SalesRecallService,
    private returnService: PurchaseReturnService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadLookups();

    // Lắng nghe sự thay đổi của loại phiếu để lọc chứng từ tham chiếu
    this.form
      .get('type')
      .valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.onTypeChange();
      });

    const streamCreator = (query: any) =>
      this.ticketService.getList({
        ...query,
        filter: this.filterText,
        type: this.filterType,
        status: this.filterStatus,
        warehouseId: this.filterWarehouseId,
      });

    this.list.maxResultCount = 10;
    this.list
      .hookToQuery(streamCreator)
      .pipe(takeUntil(this.destroy$))
      .subscribe(response => {
        this.data = response;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ============================================================
  // LOAD LOOKUPS
  // ============================================================
  loadLookups() {
    this.warehouseService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.warehouses = res.items;
      });

    // Load POs Approved/Receiving
    this.poService
      .getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.approvedPurchaseOrders = res.items
          .filter(
            po =>
              po.status === PurchaseOrderStatus.Approved ||
              po.status === PurchaseOrderStatus.Receiving,
          )
          .map(po => ({
            id: po.id,
            code: po.code,
            displayName: `${po.code} (${po.supplierName})`,
          }));

        // Nếu đang chọn GoodsReceipt thì cập nhật ngay list reference
        if (this.form.get('type').value === TicketType.GoodsReceipt) {
          this.selectableReferences = this.approvedPurchaseOrders;
        }
      });

    // Load SOs Approved/Delivering
    this.soService
      .getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.approvedSalesOrders = res.items
          .filter(
            so =>
              so.status === SalesOrderStatus.Approved || so.status === SalesOrderStatus.Delivering,
          )
          .map(so => ({
            id: so.id,
            code: so.code,
            displayName: `${so.code} (${so.customerName})`,
          }));

        // Nếu đang chọn GoodsIssue thì cập nhật ngay list reference
        if (this.form.get('type').value === TicketType.GoodsIssue) {
          this.selectableReferences = this.approvedSalesOrders;
        }
      });

    // Load SalesRecalls
    this.recallService
      .getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.activeSalesRecalls = res.items
          .filter(
            r =>
              r.status === SalesRecallStatus.Approved ||
              r.status === SalesRecallStatus.Recalling,
          )
          .map(r => ({
            id: r.id,
            code: r.code,
            displayName: `${r.code} (${r.productName})`,
          }));

        if (this.form.get('type').value === TicketType.RecallReceipt) {
          this.selectableReferences = this.activeSalesRecalls;
        }
      });

    // Load PurchaseReturns
    this.returnService
      .getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.activePurchaseReturns = res.items
          .filter(
            r =>
              r.status === PurchaseReturnStatus.Approved ||
              r.status === PurchaseReturnStatus.Returning,
          )
          .map(r => ({
            id: r.id,
            code: r.code,
            displayName: `${r.code} (${r.supplierName})`,
          }));

        if (this.form.get('type').value === TicketType.ReturnOutward) {
          this.selectableReferences = this.activePurchaseReturns;
        }
      });
  }

  onTypeChange() {
    const rawValue = this.form.get('type').value;
    const type = rawValue !== null && rawValue !== undefined ? Number(rawValue) : null;

    this.form.get('referenceDocumentId').setValue(null);

    if (type === TicketType.GoodsReceipt) {
      this.selectableReferences = this.approvedPurchaseOrders;
    } else if (type === TicketType.GoodsIssue) {
      this.selectableReferences = this.approvedSalesOrders;
    } else if (type === TicketType.RecallReceipt) {
      this.selectableReferences = this.activeSalesRecalls;
    } else if (type === TicketType.ReturnOutward) {
      this.selectableReferences = this.activePurchaseReturns;
    } else {
      this.selectableReferences = [];
    }
  }

  // ============================================================
  // ACTIONS & FILTERS
  // ============================================================
  onSearch(searchValue: string): void {
    this.filterText = searchValue;
    this.list.get();
  }

  onFilterChange() {
    this.list.get();
  }

  viewDetail(id: string): void {
    this.router.navigate(['/inventory/tickets/details', id]);
  }

  deleteTicket(id: string, ticketNumber: string): void {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure', {
        messageLocalizationParams: [ticketNumber],
      })
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.ticketService
            .delete(id)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
              this.list.get();
            });
        }
      });
  }

  // ============================================================
  // FORM HANDLING
  // ============================================================
  buildForm() {
    this.form = this.fb.group({
      type: [null, [Validators.required]],
      warehouseId: [null, [Validators.required]],
      referenceDocumentId: [null, [Validators.required]], // Bắt buộc link
      note: ['', [Validators.maxLength(1000)]],
    });
  }

  openCreateDrawer() {
    this.form.reset({
      type: null,
      warehouseId: null,
      referenceDocumentId: null,
      note: '',
    });
    this.selectableReferences = [];
    this.isDrawerOpen = true;
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
  }

  save(): void {
    if (this.form.invalid) return;
    this.isSaving = true;

    const payload = { ...this.form.value };

    // Tìm mã chứng từ tham chiếu
    const selectedRef = this.selectableReferences.find(r => r.id === payload.referenceDocumentId);
    if (selectedRef) {
      payload.referenceDocumentNumber = selectedRef.code;
    }

    this.ticketService
      .create(payload)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: newTicket => {
          this.isSaving = false;
          this.closeDrawer();
          this.list.get();
          this.viewDetail(newTicket.id);
        },
        error: () => {
          this.isSaving = false;
        },
      });
  }
}
