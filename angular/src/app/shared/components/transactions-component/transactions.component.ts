import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { InventoryTransactionService } from 'src/app/proxy/transactions';
import { InventoryTransactionDto } from 'src/app/proxy/transactions/dtos';
import { InventoryTransactionType } from 'src/app/proxy/enums/warehouses';
import { SharedModule } from 'src/app/shared/shared.module';
import { SearchComponent } from 'src/app/shared/components/search-component/search.component';
import { enumName } from 'src/app/shared/untils/enum.util';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [SharedModule, SearchComponent],
  providers: [ListService],
  templateUrl: './transactions.component.html',
  styleUrls: ['./transactions.component.scss']
})
export class TransactionsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // INPUTS ĐỂ NHẬN TỪ MODAL BALANCE-DETAIL
  @Input() isEmbedded: boolean = false;
  @Input() fixedProductId?: string;
  @Input() fixedBatchId?: string;
  @Input() fixedWarehouseId?: string;
  @Input() fixedBinId?: string;
  @Input() hideProduct: boolean = false;
  @Input() hideLocation: boolean = false;

  data = { items: [], totalCount: 0 } as PagedResultDto<InventoryTransactionDto>;

  // Biến phục vụ bộ lọc
  filterText = '';
  fromDate: string | null = null;
  toDate: string | null = null;

  TransactionType = InventoryTransactionType;
  enumName = enumName;

  constructor(
    public readonly list: ListService,
    private transactionService: InventoryTransactionService
  ) { }

  ngOnInit(): void {
    // Mặc định 10 dòng/trang để hiện thanh phân trang
    this.list.maxResultCount = 10;

    const streamCreator = (query: any) => {
      // Xử lý Timezone: ToDate lấy đến 23:59:59 cuối ngày
      const toDateParsed = this.toDate ? new Date(`${this.toDate}T23:59:59`).toISOString() : undefined;
      const fromDateParsed = this.fromDate ? new Date(`${this.fromDate}T00:00:00`).toISOString() : undefined;

      return this.transactionService.getList({
        ...query,
        filter: this.filterText,
        productId: this.fixedProductId || undefined,
        productBatchId: this.fixedBatchId || undefined,
        warehouseId: this.fixedWarehouseId || undefined,
        binId: this.fixedBinId || undefined,
        fromDate: fromDateParsed,
        toDate: toDateParsed
      });
    };

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

  onSearch(searchValue: string): void {
    this.filterText = searchValue;
    this.list.get();
  }

  onFilterChange(): void {
    this.list.get();
  }

  clearDateFilter(): void {
    this.fromDate = null;
    this.toDate = null;
    this.list.get();
  }

  // Hàm xác định Giao dịch làm TĂNG (+) hay GIẢM (-) tồn kho
  isIncrease(type: InventoryTransactionType): boolean {
    const increaseTypes = [
      InventoryTransactionType.PurchaseReceipt, // Nhập mua hàng
      InventoryTransactionType.ReturnInward,    // Khách trả lại
      InventoryTransactionType.RecallReceipt    // Nhập thu hồi
    ];
    return increaseTypes.includes(type);
  }
}