import { Component, OnInit, OnDestroy } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { LocalizationService } from '@abp/ng.core';
import { NotificationHubService } from '../shared/services/signalR/notification.hub.service';
import { WarehouseService } from '../proxy/warehouses/warehouse.service';
import { CategoryService } from '../proxy/categories/category.service';
import { DashboardService } from '../proxy/dashboard/dashboard.service';
import { DashboardFilterInput } from '../proxy/dashboard/dtos/models';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NgApexchartsModule } from 'ng-apexcharts';
import { DropdownSearchComponent } from '../shared/components/dropdownsearch-component/dropdown-search.component';
import { SearchComponent } from '../shared/components/search-component/search.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  standalone: true,
  imports: [SharedModule, NgApexchartsModule, DropdownSearchComponent, SearchComponent],
})
export class HomeComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  loading = true;
  activeTab = 'overview';
  Math = Math;

  // Dữ liệu bộ lọc toàn cục
  warehouses: any[] = [];
  selectedWarehouseId: string | null = null;
  categories: any[] = [];
  selectedCategoryId: string | null = null;
  selectedDays: number = 7;

  // Truy xuất nguồn gốc lô thuốc (Traceability)
  selectedBatchId: string | null = null;
  batchSearchText = '';
  batchLookupResults: any[] = [];
  batchTraceData: any = null;
  traceLoading = false;
  lookupLoading = false;

  // Dữ liệu cho các tab
  overviewData: any = null;
  nearExpiryBatches: any[] = [];
  alreadyExpiredBatches: any[] = [];
  debtOverviewData: any = null;
  topCustomerDebts: any[] = [];
  topSupplierDebts: any[] = [];

  // Options biểu đồ ApexCharts (Sử dụng Light Mode và Brand Color từ style.scss)
  financialChartOptions: any;
  overviewCapacityChartOptions: any;
  salesStatusChartOptions: any;
  procurementStatusChartOptions: any;
  warehouseCapacityChartOptions: any;
  transactionDistributionChartOptions: any;
  categoryDistributionChartOptions: any;
  debtComparisonChartOptions: any;
  
  // Biểu đồ mới bổ sung theo yêu cầu nghiệp vụ
  ticketStatusChartOptions: any;
  batchQAStatusChartOptions: any;
  physicalMovementChartOptions: any;

  constructor(
    private notificationHubService: NotificationHubService,
    private localizationService: LocalizationService,
    private warehouseService: WarehouseService,
    private categoryService: CategoryService,
    private dashboardService: DashboardService
  ) {}

  ngOnInit(): void {
    this.initChartOptions();
    this.loadWarehouses();
    this.loadCategories();
    this.loadCurrentTabData();
    this.connectSignalR();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Lấy các query parameters lọc
  getFilterParams(): DashboardFilterInput {
    const params: DashboardFilterInput = {};
    if (this.selectedWarehouseId) {
      params.warehouseId = this.selectedWarehouseId;
    }
    if (this.selectedCategoryId) {
      params.categoryId = this.selectedCategoryId;
    }
    if (this.selectedDays) {
      params.days = this.selectedDays;
    }
    return params;
  }

  loadWarehouses(): void {
    this.warehouseService.getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.warehouses = res.items || [];
        },
        error: err => {
          console.error('Không thể lấy danh sách kho hàng:', err);
        }
      });
  }

  loadCategories(): void {
    this.categoryService.getList({ maxResultCount: 1000, skipCount: 0 } as any)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => {
          this.categories = res.items || [];
        },
        error: err => {
          console.error('Không thể lấy danh sách nhóm thuốc:', err);
        }
      });
  }

  switchTab(tab: string): void {
    if (this.activeTab === tab) return;
    this.activeTab = tab;
    this.loadCurrentTabData();
  }

  onFilterChange(): void {
    this.loadCurrentTabData();
  }

  loadCurrentTabData(): void {
    this.loading = true;
    if (this.activeTab === 'overview') {
      this.loadOverviewData();
    } else if (this.activeTab === 'sales-procurement') {
      this.loadSalesProcurementData();
    } else if (this.activeTab === 'inventory-balance') {
      this.loadInventoryBalanceData();
    } else if (this.activeTab === 'medicine-batch') {
      this.loadMedicineBatchData();
    } else if (this.activeTab === 'debt') {
      this.loadDebtData();
    } else if (this.activeTab === 'traceability') {
      this.loading = false;
    }
  }

  // --- TẢI DỮ LIỆU TỪNG TAB ---

  loadOverviewData(): void {
    const filter = this.getFilterParams();
    forkJoin({
      overview: this.dashboardService.getOverview(filter),
      trends: this.dashboardService.getFinancialTrends(filter),
    }).subscribe({
      next: res => {
        this.overviewData = res.overview;

        // 1. Cập nhật biểu đồ doanh số vs mua hàng
        const dates = res.trends.map((t: any) => t.date);
        const sales = res.trends.map((t: any) => t.salesAmount);
        const procurements = res.trends.map((t: any) => t.procurementAmount);

        this.financialChartOptions = {
          ...this.financialChartOptions,
          series: [
            { name: this.localizationService.instant('::Dashboard.KPI.SalesRevenue'), data: sales },
            {
              name: this.localizationService.instant('::Dashboard.KPI.ProcurementCost'),
              data: procurements,
            },
          ],
          xaxis: {
            ...this.financialChartOptions.xaxis,
            categories: dates,
          },
        };

        // 2. Cập nhật biểu đồ lấp đầy tổng hệ thống
        const occupied = res.overview.averageCapacityPercent;
        const available = Math.max(0, 100 - occupied);

        this.overviewCapacityChartOptions = {
          ...this.overviewCapacityChartOptions,
          series: [occupied, available],
          plotOptions: {
            pie: {
              donut: {
                ...this.overviewCapacityChartOptions.plotOptions.pie.donut,
                labels: {
                  ...this.overviewCapacityChartOptions.plotOptions.pie.donut.labels,
                  total: {
                    show: true,
                    label: this.localizationService.instant('::Dashboard.KPI.AverageCapacity'),
                    formatter: () => occupied + '%',
                  },
                },
              },
            },
          },
        };

        this.loading = false;
      },
      error: err => {
        console.error('Không thể lấy dữ liệu Overview:', err);
        this.loading = false;
      },
    });
  }

  loadSalesProcurementData(): void {
    const filter = this.getFilterParams();
    forkJoin({
      sales: this.dashboardService.getSalesStatusDistribution(filter),
      procurement: this.dashboardService.getProcurementStatusDistribution(filter),
      overview: this.dashboardService.getOverview(filter),
      transactions: this.dashboardService.getInventoryTransactionDistribution(filter),
    }).subscribe({
      next: res => {
        this.overviewData = res.overview;

        // 1. Cập nhật trạng thái SO
        const salesLabels = res.sales.map((s: any) => s.statusName);
        const salesValues = res.sales.map((s: any) => s.count);
        this.salesStatusChartOptions = {
          ...this.salesStatusChartOptions,
          series: salesValues,
          labels: salesLabels,
        };

        // 2. Cập nhật trạng thái PO
        const procLabels = res.procurement.map((p: any) => p.statusName);
        const procValues = res.procurement.map((p: any) => p.count);
        this.procurementStatusChartOptions = {
          ...this.procurementStatusChartOptions,
          series: procValues,
          labels: procLabels,
        };

        // 3. Cơ cấu giao dịch để thấy lượng thu hồi và xuất trả
        const txLabels = res.transactions.map((t: any) => t.transactionTypeName);
        const txValues = res.transactions.map((t: any) => t.count);
        this.transactionDistributionChartOptions = {
          ...this.transactionDistributionChartOptions,
          series: txValues,
          labels: txLabels,
        };

        this.loading = false;
      },
      error: err => {
        console.error('Không thể lấy dữ liệu SO/PO:', err);
        this.loading = false;
      },
    });
  }

  loadInventoryBalanceData(): void {
    const filter = this.getFilterParams();
    forkJoin({
      capacities: this.dashboardService.getWarehouseCapacities(filter),
      tickets: this.dashboardService.getInventoryTicketStatusDistribution(filter),
      movements: this.dashboardService.getPhysicalMovementTrends(filter),
      overview: this.dashboardService.getOverview(filter),
    }).subscribe({
      next: res => {
        this.overviewData = res.overview;

        // 1. Sức chứa từng kho (Stacked Bar Chart: Available vs Reserved vs Safe Free)
        const whNames = res.capacities.map((c: any) => c.warehouseName);
        const availableVol = res.capacities.map((c: any) => c.availableVolume);
        const reservedVol = res.capacities.map((c: any) => c.reservedVolume);
        const freeVol = res.capacities.map((c: any) => {
          const free = c.safeMaxVolume - c.occupiedVolume;
          return free > 0 ? free : 0;
        });

        this.warehouseCapacityChartOptions = {
          ...this.warehouseCapacityChartOptions,
          series: [
            { name: this.localizationService.instant('::Dashboard.WarehouseCapacity.Available'), data: availableVol },
            { name: this.localizationService.instant('::Dashboard.WarehouseCapacity.Reserved'), data: reservedVol },
            { name: this.localizationService.instant('::Dashboard.WarehouseCapacity.Free'), data: freeVol }
          ],
          xaxis: {
            ...this.warehouseCapacityChartOptions.xaxis,
            categories: whNames,
          },
        };

        // 2. Trạng thái phiếu kho
        const tLabels = res.tickets.map((t: any) => t.statusName);
        const tValues = res.tickets.map((t: any) => t.count);
        this.ticketStatusChartOptions = {
          ...this.ticketStatusChartOptions,
          series: tValues,
          labels: tLabels,
        };

        // 3. Xu hướng luân chuyển vật lý Nhập - Xuất hàng ngày
        const mDates = res.movements.map((m: any) => m.date);
        const inboundData = res.movements.map((m: any) => m.inboundVolume);
        const outboundData = res.movements.map((m: any) => m.outboundVolume);

        this.physicalMovementChartOptions = {
          ...this.physicalMovementChartOptions,
          series: [
            { name: this.localizationService.instant('::Dashboard.PhysicalMovement.Inbound'), data: inboundData },
            { name: this.localizationService.instant('::Dashboard.PhysicalMovement.Outbound'), data: outboundData }
          ],
          xaxis: {
            ...this.physicalMovementChartOptions.xaxis,
            categories: mDates,
          }
        };

        this.loading = false;
      },
      error: err => {
        console.error('Không thể lấy dữ liệu Kho & Tồn:', err);
        this.loading = false;
      },
    });
  }

  loadMedicineBatchData(): void {
    const filter = this.getFilterParams();
    forkJoin({
      categories: this.dashboardService.getMedicineCategoryDistribution(filter),
      batchQA: this.dashboardService.getBatchQAStatusDistribution(filter),
      nearExpiry: this.dashboardService.getNearExpiryBatches(filter),
      expired: this.dashboardService.getAlreadyExpiredBatches(filter),
    }).subscribe({
      next: res => {
        // 1. Cơ cấu nhóm dược phẩm
        const catLabels = res.categories.map((c: any) => c.categoryName);
        const catValues = res.categories.map((c: any) => c.totalQuantity);
        this.categoryDistributionChartOptions = {
          ...this.categoryDistributionChartOptions,
          series: catValues,
          labels: catLabels,
        };

        // 2. Cơ cấu trạng thái QA của Lô thuốc
        const qaLabels = res.batchQA.map((q: any) => q.statusName);
        const qaValues = res.batchQA.map((q: any) => q.count);
        this.batchQAStatusChartOptions = {
          ...this.batchQAStatusChartOptions,
          series: qaValues,
          labels: qaLabels,
        };

        // 3. Danh sách lô cận hạn và hết hạn
        this.nearExpiryBatches = res.nearExpiry;
        this.alreadyExpiredBatches = res.expired;

        this.loading = false;
      },
      error: err => {
        console.error('Không thể lấy dữ liệu Thuốc & Lô:', err);
        this.loading = false;
      },
    });
  }

  loadDebtData(): void {
    const filter = this.getFilterParams();
    forkJoin({
      overview: this.dashboardService.getDebtOverview(filter),
      customers: this.dashboardService.getTopCustomerDebts(filter),
      suppliers: this.dashboardService.getTopSupplierDebts(filter),
    }).subscribe({
      next: res => {
        this.debtOverviewData = res.overview;
        this.topCustomerDebts = res.customers;
        this.topSupplierDebts = res.suppliers;

        // Cập nhật biểu đồ cột so sánh
        const receivable = res.overview.totalReceivableDebt;
        const payable = res.overview.totalPayableDebt;

        this.debtComparisonChartOptions = {
          ...this.debtComparisonChartOptions,
          series: [
            {
              name: this.localizationService.instant('::Dashboard.Debt.CurrentDebtLabel'),
              data: [receivable, payable],
            },
          ],
        };

        this.loading = false;
      },
      error: err => {
        console.error('Không thể lấy dữ liệu Công nợ:', err);
        this.loading = false;
      },
    });
  }

  connectSignalR(): void {
    this.notificationHubService.connect();
    this.notificationHubService.received$.pipe(takeUntil(this.destroy$)).subscribe(notification => {
      if (notification.title === '[System] InventoryChanged') {
        this.loadCurrentTabData();
      }
    });
  }

  // --- CẤU HÌNH BIỂU ĐỒ BAN ĐẦU (LIGHT MODE + BRAND COLOR) ---

  initChartOptions(): void {
    const defaultFont = "'Inter', 'Segoe UI', system-ui, sans-serif";
    const brandColor = '#00B37E'; // Pharmacy Green từ style.scss

    // 1. Biểu đồ Cán cân tài chính: SO vs PO (Area Chart)
    this.financialChartOptions = {
      series: [],
      chart: {
        type: 'area',
        height: 320,
        background: 'transparent',
        fontFamily: defaultFont,
        foreColor: '#64748B',
        toolbar: { show: false },
      },
      colors: [brandColor, '#3b82f6'],
      stroke: { curve: 'smooth', width: 2.5 },
      fill: {
        type: 'gradient',
        gradient: {
          shadeIntensity: 0.5,
          opacityFrom: 0.2,
          opacityTo: 0.02,
        },
      },
      dataLabels: { enabled: false },
      xaxis: {
        categories: [],
        axisBorder: { show: false },
        axisTicks: { show: false },
      },
      yaxis: {
        labels: {
          formatter: (val: number) => val.toLocaleString() + ' đ',
        },
      },
      grid: {
        borderColor: '#E2E8F0',
        strokeDashArray: 4,
      },
      theme: { mode: 'light' },
    };

    // 2. Biểu đồ Phân bổ lấp đầy tổng thể (Donut Chart)
    this.overviewCapacityChartOptions = {
      series: [],
      chart: {
        type: 'donut',
        height: 280,
        background: 'transparent',
        fontFamily: defaultFont,
        foreColor: '#64748B',
      },
      labels: [
        this.localizationService.instant('::Dashboard.CapacityAllocation.Occupied'),
        this.localizationService.instant('::Dashboard.CapacityAllocation.Available'),
      ],
      colors: ['#6366f1', '#F1F5F9'],
      legend: {
        position: 'bottom',
        fontSize: '11px',
      },
      dataLabels: { enabled: false },
      plotOptions: {
        pie: {
          donut: {
            size: '75%',
            labels: {
              show: true,
              total: {
                show: true,
                label: this.localizationService.instant('::Dashboard.KPI.AverageCapacity'),
                formatter: () => '0%',
              },
            },
          },
        },
      },
      stroke: { show: true, width: 2, colors: ['#ffffff'] },
      theme: { mode: 'light' },
    };

    // 3. Biểu đồ Trạng thái đơn SO (Pie Chart)
    this.salesStatusChartOptions = {
      series: [],
      chart: {
        type: 'pie',
        height: 300,
        background: 'transparent',
        fontFamily: defaultFont,
        foreColor: '#64748B',
      },
      labels: [],
      colors: [brandColor, '#3b82f6', '#f59e0b', '#ef4444', '#8b5cf6', '#64748B'],
      legend: {
        position: 'bottom',
        fontSize: '11px',
      },
      stroke: { show: true, width: 2, colors: ['#ffffff'] },
      theme: { mode: 'light' },
    };

    // 4. Biểu đồ Trạng thái đơn PO (Donut Chart)
    this.procurementStatusChartOptions = {
      series: [],
      chart: {
        type: 'donut',
        height: 300,
        background: 'transparent',
        fontFamily: defaultFont,
        foreColor: '#64748B',
      },
      labels: [],
      colors: ['#8b5cf6', '#a855f7', '#f59e0b', brandColor, '#ef4444', '#64748B'],
      legend: {
        position: 'bottom',
        fontSize: '11px',
      },
      stroke: { show: true, width: 2, colors: ['#ffffff'] },
      theme: { mode: 'light' },
    };

    // 5. Biểu đồ Sức chứa từng kho (Stacked Bar Chart: Available vs Reserved vs Free)
    this.warehouseCapacityChartOptions = {
      series: [],
      chart: {
        type: 'bar',
        height: 320,
        stacked: true,
        background: 'transparent',
        fontFamily: defaultFont,
        foreColor: '#64748B',
        toolbar: { show: false },
      },
      plotOptions: {
        bar: {
          borderRadius: 4,
          horizontal: true,
          barHeight: '55%',
        },
      },
      colors: [brandColor, '#f59e0b', '#E2E8F0'], // Available (xanh), Reserved (vàng), Safe Free (xám nhạt)
      dataLabels: {
        enabled: true,
        formatter: (val: number) => {
          if (val === 0) return '';
          return (val / 1000000).toFixed(2) + ' m³';
        },
        style: { fontSize: '10px', colors: ['#fff', '#fff', '#64748B'] },
      },
      xaxis: {
        categories: [],
        labels: {
          formatter: (val: number) => (val / 1000000).toFixed(1) + ' m³',
        },
      },
      grid: {
        borderColor: '#E2E8F0',
        strokeDashArray: 4,
      },
      legend: {
        position: 'bottom',
        fontSize: '11px',
      },
      theme: { mode: 'light' },
    };

    // 6. Biểu đồ Cơ cấu giao dịch kho (Donut Chart)
    this.transactionDistributionChartOptions = {
      series: [],
      chart: {
        type: 'donut',
        height: 280,
        background: 'transparent',
        fontFamily: defaultFont,
        foreColor: '#64748B',
      },
      labels: [],
      colors: ['#0694a2', '#8b5cf6', '#ec4899', '#f59e0b', '#3b82f6', '#ef4444'],
      legend: {
        position: 'bottom',
        fontSize: '11px',
      },
      stroke: { show: true, width: 2, colors: ['#ffffff'] },
      theme: { mode: 'light' },
    };

    // 7. Biểu đồ Cơ cấu nhóm dược phẩm (Donut Chart)
    this.categoryDistributionChartOptions = {
      series: [],
      chart: {
        type: 'donut',
        height: 280,
        background: 'transparent',
        fontFamily: defaultFont,
        foreColor: '#64748B',
      },
      labels: [],
      colors: ['#ec4899', '#8b5cf6', '#3b82f6', brandColor, '#f59e0b', '#6366f1'],
      legend: {
        position: 'bottom',
        fontSize: '11px',
      },
      stroke: { show: true, width: 2, colors: ['#ffffff'] },
      theme: { mode: 'light' },
    };

    // 8. Biểu đồ Cột Công nợ: Phải thu vs. Phải trả (Column Chart)
    this.debtComparisonChartOptions = {
      series: [],
      chart: {
        type: 'bar',
        height: 320,
        background: 'transparent',
        fontFamily: defaultFont,
        foreColor: '#64748B',
        toolbar: { show: false },
      },
      plotOptions: {
        bar: {
          columnWidth: '45%',
          distributed: true,
          borderRadius: 4,
        },
      },
      colors: ['#10b981', '#ef4444'], // xanh cho phải thu, đỏ cho phải trả
      dataLabels: {
        enabled: true,
        formatter: (val: number) => val.toLocaleString() + ' đ',
        style: { fontSize: '11px', colors: ['#333'] },
      },
      stroke: { show: false },
      xaxis: {
        categories: [
          this.localizationService.instant('::Dashboard.Debt.ReceivableLabel'),
          this.localizationService.instant('::Dashboard.Debt.PayableLabel'),
        ],
        axisBorder: { show: false },
        axisTicks: { show: false },
      },
      yaxis: {
        labels: {
          formatter: (val: number) => val.toLocaleString() + ' đ',
        },
      },
      grid: {
        borderColor: '#E2E8F0',
        strokeDashArray: 4,
      },
      legend: { show: false },
      theme: { mode: 'light' },
    };

    // 9. Biểu đồ Luân chuyển Nhập - Xuất vật lý (Column Chart)
    this.physicalMovementChartOptions = {
      series: [],
      chart: {
        type: 'bar',
        height: 320,
        background: 'transparent',
        fontFamily: defaultFont,
        foreColor: '#64748B',
        toolbar: { show: false },
      },
      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: '55%',
          borderRadius: 4,
        },
      },
      colors: ['#3b82f6', '#ef4444'], // Nhập (xanh dương), Xuất (đỏ)
      dataLabels: { enabled: false },
      stroke: {
        show: true,
        width: 2,
        colors: ['transparent']
      },
      xaxis: {
        categories: [],
        axisBorder: { show: false },
        axisTicks: { show: false },
      },
      yaxis: {
        labels: {
          formatter: (val: number) => (val / 1000000).toFixed(2) + ' m³',
        },
      },
      grid: {
        borderColor: '#E2E8F0',
        strokeDashArray: 4,
      },
      legend: {
        position: 'bottom',
        fontSize: '11px',
      },
      theme: { mode: 'light' },
    };

    // 10. Biểu đồ Donut Trạng thái Phiếu kho
    this.ticketStatusChartOptions = {
      series: [],
      chart: {
        type: 'donut',
        height: 300,
        background: 'transparent',
        fontFamily: defaultFont,
        foreColor: '#64748B',
      },
      labels: [],
      colors: ['#64748B', '#f59e0b', brandColor, '#ef4444'], // Draft, Pending, Approved, Rejected
      legend: {
        position: 'bottom',
        fontSize: '11px',
      },
      stroke: { show: true, width: 2, colors: ['#ffffff'] },
      theme: { mode: 'light' },
    };

    // 11. Biểu đồ Donut Trạng thái QA Lô hàng
    this.batchQAStatusChartOptions = {
      series: [],
      chart: {
        type: 'donut',
        height: 300,
        background: 'transparent',
        fontFamily: defaultFont,
        foreColor: '#64748B',
      },
      labels: [],
      colors: ['#3b82f6', brandColor, '#ef4444', '#8b5cf6', '#64748B'], // PendingQA, Approved, Rejected, Recalled, Expired
      legend: {
        position: 'bottom',
        fontSize: '11px',
      },
      stroke: { show: true, width: 2, colors: ['#ffffff'] },
      theme: { mode: 'light' },
    };
  }

  onBatchSearch(query: string): void {
    this.batchSearchText = query;
    if (!query || !query.trim()) {
      this.batchLookupResults = [];
      return;
    }

    this.lookupLoading = true;
    this.dashboardService.getBatchLookup(query.trim())
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: res => {
        this.batchLookupResults = res || [];
        this.lookupLoading = false;
      },
      error: err => {
        console.error('Không tìm thấy lô thuốc:', err);
        this.batchLookupResults = [];
        this.lookupLoading = false;
      }
    });
  }

  selectBatch(batch: any): void {
    this.selectedBatchId = batch.id;
    this.batchSearchText = batch.batchNumber;
    this.batchLookupResults = [];
    this.traceLoading = true;

    this.dashboardService.getBatchTraceDetails(batch.id)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: res => {
        this.batchTraceData = res;
        this.traceLoading = false;
      },
      error: err => {
        console.error('Không thể truy xuất chi tiết lô thuốc:', err);
        this.batchTraceData = null;
        this.traceLoading = false;
      }
    });
  }

  clearBatchSearch(): void {
    this.selectedBatchId = null;
    this.batchSearchText = '';
    this.batchLookupResults = [];
    this.batchTraceData = null;
  }
}
