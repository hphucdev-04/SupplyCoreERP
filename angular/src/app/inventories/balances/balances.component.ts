import { Component, OnInit, OnDestroy } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Router } from '@angular/router';

import { InventoryBalanceService } from 'src/app/proxy/balances';
import { InventoryBalanceDto } from 'src/app/proxy/balances/dtos';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { MedicineService } from 'src/app/proxy/medicines';

import { SharedModule } from 'src/app/shared/shared.module';
import { SearchComponent } from 'src/app/shared/components/search-component/search.component';
import { TransactionsComponent } from 'src/app/shared/components/transactions-component/transactions.component';


@Component({
  selector: 'app-balances',
  standalone: true,
  imports: [SharedModule, SearchComponent, TransactionsComponent],
  providers: [ListService],
  templateUrl: './balances.component.html',
  styleUrls: ['./balances.component.scss']
})
export class BalancesComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  activeTab: 'balances' | 'transactions' = 'balances';

  data = { items: [], totalCount: 0 } as PagedResultDto<InventoryBalanceDto>;

  warehouses: any[] = [];
  medicines: any[] = [];

  filterText = '';
  filterWarehouseId: string = null;
  filterMedicineId: string = null;

  constructor(
    public readonly list: ListService,
    private balanceService: InventoryBalanceService,
    private warehouseService: WarehouseService,
    private medicineService: MedicineService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadLookups();

    const streamCreator = (query: any) => this.balanceService.getList({
      ...query,
      filter: this.filterText,
      warehouseId: this.filterWarehouseId,
      productId: this.filterMedicineId
    });

    this.list.maxResultCount = 15;
    this.list.hookToQuery(streamCreator)
      .pipe(takeUntil(this.destroy$))
      .subscribe((response) => this.data = response);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLookups() {
    this.warehouseService.getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$)).subscribe(res => this.warehouses = res.items);

    this.medicineService.getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$)).subscribe(res => this.medicines = res.items);
  }

  onSearch(searchValue: string): void {
    this.filterText = searchValue;
    this.list.get();
  }

  onFilterChange() {
    this.list.get();
  }

  // ĐIỀU HƯỚNG SANG ROUTE CHI TIẾT
  openDetail(id: string) {
    this.router.navigate(['/inventory/balances/details', id]);
  }
}