import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { TicketType } from 'src/app/proxy/enums/warehouses/ticket-type.enum';
import { ApprovalStatus } from 'src/app/proxy/enums/warehouses/approval-status.enum';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { InventoryTicketDto } from 'src/app/proxy/tickets/dtos';
import { InventoryTicketService } from 'src/app/proxy/tickets';
import { WarehouseService } from 'src/app/proxy/warehouses';

@Component({
  selector: 'app-inventory-tickets',
  standalone: true,
  imports: [SharedModule, DrawerComponent],
  providers: [ListService],
  templateUrl: './tickets.component.html',
  styleUrls: ['./tickets.component.scss']
})
export class TicketsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  ticketData = { items: [], totalCount: 0 } as PagedResultDto<InventoryTicketDto>;
  warehouses: WarehouseDto[] = [];

  // Drawer & Form
  isDrawerOpen = false;
  form: FormGroup;
  isSaving = false;

  // Filters
  filterText = '';
  selectedType: TicketType | null = null;
  selectedStatus: ApprovalStatus | null = null;
  selectedWarehouseId: string | null = null;

  // Expose Enums to HTML
  TicketType = TicketType;
  ApprovalStatus = ApprovalStatus;

  ticketTypes = Object.keys(TicketType).filter(k => !isNaN(Number(k))).map(k => Number(k));
  approvalStatuses = Object.keys(ApprovalStatus).filter(k => !isNaN(Number(k))).map(k => Number(k));

  constructor(
    public readonly list: ListService,
    private ticketService: InventoryTicketService,
    private warehouseService: WarehouseService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadWarehouses();
    this.loadTickets();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ============================================================
  // LOAD DATA & LIST SERVICE
  // ============================================================
  loadWarehouses() {
    this.warehouseService.getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.warehouses = res.items;
      });
  }

  loadTickets() {
    const streamCreator = (query: any) => this.ticketService.getList({
      ...query,
      filter: this.filterText,
      type: this.selectedType,
      status: this.selectedStatus,
      warehouseId: this.selectedWarehouseId
    });

    this.list.hookToQuery(streamCreator)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.ticketData = res;
      });
  }

  applyFilters() {
    this.list.get();
  }

  clearFilters() {
    this.filterText = '';
    this.selectedType = null;
    this.selectedStatus = null;
    this.selectedWarehouseId = null;
    this.list.get();
  }

  // ============================================================
  // QUẢN LÝ FORM & CREATE
  // ============================================================
  buildForm() {
    this.form = this.fb.group({
      type: [TicketType.GoodsReceipt, [Validators.required]],
      warehouseId: [null, [Validators.required]],
      referenceDocumentId: [null], 
      note: ['', [Validators.maxLength(1000)]]
    });
  }

  openCreateDrawer() {
    this.form.reset({
      type: TicketType.GoodsReceipt,
      warehouseId: this.warehouses.length > 0 ? this.warehouses[0].id : null,
      referenceDocumentId: null,
      note: ''
    });
    this.isDrawerOpen = true;
  }

  closeDrawer() {
    this.isDrawerOpen = false;
  }

  save() {
    if (this.form.invalid) return;
    this.isSaving = true;

    // Lúc này Backend (TicketManager) sẽ check Rule chống spam và tự sinh mã Phiếu
    this.ticketService.create(this.form.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (newTicket) => {
          this.isSaving = false;
          this.closeDrawer();
          this.list.get(); 
          // Tạo xong tự động chuyển hướng sang trang chi tiết để thêm hàng
          this.goToDetail(newTicket.id);
        },
        error: () => {
          this.isSaving = false;
        }
      });
  }

  // ============================================================
  // ACTIONS (DELETE, NAVIGATE)
  // ============================================================
  deleteTicket(id: string, ticketNumber: string) {
    this.confirmation.warn('::TicketDeletionWarningMessage', '::AreYouSure', {
      messageLocalizationParams: [ticketNumber]
    }).subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.ticketService.delete(id)
          .pipe(takeUntil(this.destroy$))
          .subscribe(() => {
            this.list.get();
          });
      }
    });
  }

  goToDetail(id: string) {
    this.router.navigate(['/inventory/tickets', id]);
  }
}