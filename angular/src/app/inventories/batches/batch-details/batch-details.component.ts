import { Component, OnDestroy, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { ListService, PagedResultDto } from "@abp/ng.core";
import { Subject, takeUntil } from "rxjs";
import { ProductBatchDto } from "src/app/proxy/batches/dtos";
import { ProductBatchService } from "src/app/proxy/batches";
import { InventoryBalanceService } from "src/app/proxy/balances";
import { InventoryTransactionService } from "src/app/proxy/transactions";
import { InventoryBalanceDto, InventoryReservationDto } from "src/app/proxy/balances/dtos";
import { InventoryTransactionDto } from "src/app/proxy/transactions/dtos";
import { BatchQAStatus } from "src/app/proxy/enums/warehouses/batch-qastatus.enum";
import { InventoryTransactionType } from "src/app/proxy/enums/warehouses/inventory-transaction-type.enum";
import { ReservationStatus } from "src/app/proxy/enums/balances/reservation-status.enum";
import { SharedModule } from "src/app/shared/shared.module";

@Component({
  selector: 'app-batch-details',
  standalone: true,
  imports: [SharedModule],
  templateUrl: 'batch-details.component.html',
  styleUrls: ['batch-details.component.scss'],
  providers: [ListService]
})
export class BatchDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  batchId: string;
  batch: ProductBatchDto = {} as ProductBatchDto;
  
  balances: InventoryBalanceDto[] = [];
  transactions: InventoryTransactionDto[] = [];
  reservations: InventoryReservationDto[] = [];

  activeTab = 'info'; // 'info', 'balances', 'transactions', 'reservations'
  
  readonly BatchQAStatus = BatchQAStatus;
  readonly InventoryTransactionType = InventoryTransactionType;
  readonly ReservationStatus = ReservationStatus;

  constructor(
    private route: ActivatedRoute,
    private batchService: ProductBatchService,
    private balanceService: InventoryBalanceService,
    private transactionService: InventoryTransactionService,
    public readonly list: ListService
  ) {}

  ngOnInit(): void {
    this.batchId = this.route.snapshot.params['id'];
    if (this.batchId) {
      this.loadBatch();
      this.loadBalances();
      this.loadTransactions();
      this.loadReservations();
    }
  }

  loadBatch() {
    this.batchService.get(this.batchId).subscribe(res => {
      this.batch = res;
    });
  }

  loadBalances() {
    this.balanceService.getList({ productBatchId: this.batchId, maxResultCount: 1000 }).subscribe(res => {
      this.balances = res.items;
    });
  }

  loadTransactions() {
    this.transactionService.getList({ productBatchId: this.batchId, maxResultCount: 1000, sorting: "creationTime DESC" }).subscribe(res => {
      this.transactions = res.items;
    });
  }

  loadReservations() {
    this.balanceService.getReservationList({ productBatchId: this.batchId, maxResultCount: 1000 }).subscribe(res => {
      this.reservations = res.items;
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get totalQuantity(): number {
    return this.balances.reduce((acc, curr) => acc + (curr.quantity || 0), 0);
  }

  get totalAvailable(): number {
    return this.balances.reduce((acc, curr) => acc + (curr.availableQuantity || 0), 0);
  }

  get totalLocked(): number {
    return this.balances.reduce((acc, curr) => acc + (curr.lockedQuantity || 0), 0);
  }
}
