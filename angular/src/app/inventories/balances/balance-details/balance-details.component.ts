import { Component, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { InventoryBalanceService } from 'src/app/proxy/balances';
import { InventoryBalanceDetailDto } from 'src/app/proxy/balances/dtos';
import { TransactionsComponent } from 'src/app/shared/components/transactions-component/transactions.component';
import { SharedModule } from 'src/app/shared/shared.module';


@Component({
  selector: 'app-balance-detail-modal',
  standalone: true,
  imports: [SharedModule, TransactionsComponent],
  templateUrl: './balance-details.component.html'
})
export class BalanceDetailsComponent implements OnDestroy {
  private destroy$ = new Subject<void>();

  // Modal State chuẩn ABP
  isVisible = false;
  balanceId = '';
  detail: InventoryBalanceDetailDto | null = null;
  isLoading = false;
  activeTab: 'info' | 'history' = 'info';

  constructor(private balanceService: InventoryBalanceService) { }

  public open(id: string) {
    this.balanceId = id;
    this.activeTab = 'info';
    this.detail = null;
    this.isVisible = true; // Kích hoạt abp-modal
    this.loadDetail(id);
  }

  public close() {
    this.isVisible = false;
  }

  private loadDetail(id: string) {
    this.isLoading = true;
    this.balanceService.get(id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe(res => {
        this.detail = res;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}