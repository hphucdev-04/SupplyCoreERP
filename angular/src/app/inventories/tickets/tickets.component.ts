import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { WarehouseDto } from 'src/app/proxy/warehouses/dtos';
import { TicketType, ticketTypeOptions } from 'src/app/proxy/enums/warehouses/ticket-type.enum';
import { ApprovalStatus, approvalStatusOptions } from 'src/app/proxy/enums/warehouses/approval-status.enum';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search-component/search.component';
import { InventoryTicketDto } from 'src/app/proxy/tickets/dtos';
import { InventoryTicketService } from 'src/app/proxy/tickets';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { enumName } from 'src/app/shared/untils/enum.util';
import { TicketDetailsComponent } from './tickets-details/ticket-details.component';

@Component({
  selector: 'app-inventory-tickets',
  standalone: true,
  imports: [SharedModule, DrawerComponent, SearchComponent, TicketDetailsComponent],
  providers: [ListService],
  templateUrl: './tickets.component.html',
  styleUrls: ['./tickets.component.scss']
})
export class TicketsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data
  data = { items: [], totalCount: 0 } as PagedResultDto<InventoryTicketDto>;
  warehouses: WarehouseDto[] = [];

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

  // Modal detail state
  @ViewChild('detailModal') detailModal: TicketDetailsComponent;

  constructor(
    public readonly list: ListService,
    private ticketService: InventoryTicketService,
    private warehouseService: WarehouseService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.buildForm();
    this.loadLookups();

    const streamCreator = (query: any) => this.ticketService.getList({
      ...query,
      filter: this.filterText,
      type: this.filterType,
      status: this.filterStatus,
      warehouseId: this.filterWarehouseId
    });

    this.list.maxResultCount = 10;
    this.list.hookToQuery(streamCreator)
      .pipe(takeUntil(this.destroy$))
      .subscribe((response) => {
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
    this.warehouseService.getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.warehouses = res.items;
      });
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
    this.detailModal.open(id);
  }

  onTicketSaved() {
    this.list.get(); // Reload list after modal changes
  }

  deleteTicket(id: string, ticketNumber: string): void {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure', {
        messageLocalizationParams: [ticketNumber]
      })
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.ticketService.delete(id)
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

  closeDrawer(): void {
    this.isDrawerOpen = false;
  }

  save(): void {
    if (this.form.invalid) return;
    this.isSaving = true;

    this.ticketService.create(this.form.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (newTicket) => {
          this.isSaving = false;
          this.closeDrawer();
          this.list.get();
          // Open Modal Detail directly after creation
          this.viewDetail(newTicket.id);
        },
        error: () => {
          this.isSaving = false;
        }
      });
  }
}