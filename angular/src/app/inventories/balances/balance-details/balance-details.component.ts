import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { eLayoutType, RoutesService } from '@abp/ng.core';
import { InventoryBalanceService } from 'src/app/proxy/balances';
import { InventoryBalanceDetailDto } from 'src/app/proxy/balances/dtos';
import { TransactionsComponent } from 'src/app/shared/components/transactions-component/transactions.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ReservationStatus } from '../../../proxy/enums/balances/reservation-status.enum';
import { enumName } from '../../../shared/untils/enum.util';


@Component({
  selector: 'app-balance-details',
  standalone: true,
  imports: [SharedModule, TransactionsComponent],
  templateUrl: './balance-details.component.html'
})
export class BalanceDetailsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private readonly ROUTE_NAME = '::Menu:BalanceDetails';

  balanceId = '';
  detail: InventoryBalanceDetailDto | null = null;
  isLoading = false;
  activeTab: 'history' | 'reservations' = 'reservations';

  ReservationStatus = ReservationStatus;
  enumName = enumName;

  constructor(
    private route: ActivatedRoute,
    private balanceService: InventoryBalanceService,
    private routesService: RoutesService
  ) { }

  ngOnInit(): void {
    this.balanceId = this.route.snapshot.params['id'];
    if (this.balanceId) {
      this.loadDetail(this.balanceId);
    }
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
        this.routesService.add([
          {
            path: `/inventory/balances/details/${this.balanceId}`,
            name: this.ROUTE_NAME,
            parentName: '::Menu:Balances',
            iconClass: 'fas fa-cubes',
            layout: eLayoutType.application,
          },
        ]);
      });
  }

  ngOnDestroy(): void {
    this.routesService.remove([this.ROUTE_NAME]);
    this.destroy$.next();
    this.destroy$.complete();
  }
}