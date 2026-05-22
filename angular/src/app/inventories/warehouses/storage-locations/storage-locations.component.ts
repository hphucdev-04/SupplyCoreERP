import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  AfterViewInit,
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { RoutesService, eLayoutType } from '@abp/ng.core';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { WarehouseDto, ZoneDto, BinDto } from 'src/app/proxy/warehouses/dtos';
import { ZoneType, zoneTypeOptions } from 'src/app/proxy/enums/warehouses';
import { StorageCondition, storageConditionOptions } from 'src/app/proxy/enums/medicines';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { Subject, forkJoin, lastValueFrom } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DragDropModule, CdkDragEnd } from '@angular/cdk/drag-drop';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';

type ResizeHandle = 'n' | 's' | 'e' | 'w' | 'nw' | 'ne' | 'sw' | 'se';

export interface CanvasZone extends ZoneDto {
  _isDirty?: boolean;
  _hasCollision?: boolean;
}

export interface CanvasBin extends BinDto {
  _isDirty?: boolean;
  _hasCollision?: boolean;
}

@Component({
  selector: 'app-storage-locations',
  standalone: true,
  imports: [DragDropModule, SharedModule, DrawerComponent],
  templateUrl: './storage-locations.component.html',
  styleUrls: ['./storage-locations.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorageLocationsComponent implements OnInit, OnDestroy, AfterViewInit {
  private destroy$ = new Subject<void>();

  warehouseId: string;
  warehouse: WarehouseDto;

  zones: CanvasZone[] = [];
  bins: CanvasBin[] = [];

  // ═══════════════════════════════════════════════════════════════════
  // ROOT-CAUSE FIX: Tách drag positions ra Map riêng.
  //
  // Vấn đề gốc: [cdkDragFreeDragPosition]="{ x: zone.positionX * scale, y: ... }"
  //   → Mỗi lần Angular CD chạy (sau bất kỳ async), expression này tạo object MỚI
  //   → CDK so sánh reference thấy "đổi" → reset vị trí về giá trị mới
  //   → Tất cả zone bị teleport về positionX/Y từ server (vị trí zone mới tạo)
  //
  // Giải pháp: Map<id, {x,y}> ổn định reference giữa các CD cycles.
  //   refreshMap() KHÔNG SET lại Map cho zones đã có (chỉ set cho zone mới).
  //   Chỉ update Map khi: user drag end, resize, zoom thay đổi.
  // ═══════════════════════════════════════════════════════════════════
  readonly zoneDragPos = new Map<string, { x: number; y: number }>();
  readonly binDragPos = new Map<string, { x: number; y: number }>();

  getDragPos(
    id: string,
    map: Map<string, { x: number; y: number }>,
    fallbackX: number,
    fallbackY: number,
  ) {
    if (!map.has(id)) map.set(id, { x: fallbackX, y: fallbackY });
    return map.get(id)!;
  }

  drawerType: 'ZONE' | 'BIN' | null = null;
  form: FormGroup;
  selectedZone: CanvasZone | null = null;
  selectedBin: CanvasBin | null = null;
  activeZone: CanvasZone | null = null;

  hasUnsavedChanges = false;
  isSaving = false;

  scale = 0.5;
  private _initialZoomDone = false;

  activeTab: 'zones' | 'bins' = 'zones';

  readonly ZOOM_LEVELS = [0.15, 0.25, 0.33, 0.5, 0.67, 0.75, 1.0, 1.25, 1.5, 2.0];
  readonly PX_PER_M = 20;
  private readonly SNAP_THRESHOLD = 12;
  private readonly SNAP_GRID = 5;

  isResizing = false;
  resizingItem: {
    item: CanvasZone | CanvasBin;
    type: 'zone' | 'bin';
    handle: ResizeHandle;
  } | null = null;
  resizeStartMouse = { x: 0, y: 0 };
  resizeStartRect = { x: 0, y: 0, w: 0, h: 0 };

  isRotating = false;
  rotatingItem: { item: any } | null = null;
  rotateCenterCanvas = { x: 0, y: 0 };

  isPanning = false;
  panStartMouse = { x: 0, y: 0 };
  panStartScroll = { x: 0, y: 0 };

  expandedZones = new Set<string>();

  zoneTypes = zoneTypeOptions;
  storageConditions = storageConditionOptions;
  ZoneType = ZoneType;
  StorageCondition = StorageCondition;

  private readonly ROUTE_NAME = '::Menu:StorageLocations';
  private canvasContainer: HTMLElement | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private routesService: RoutesService,
    private warehouseService: WarehouseService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.warehouseId = this.route.snapshot.paramMap.get('id');
    this.loadWarehouseInfo();
    this.refreshMap(null, true);
  }

  ngAfterViewInit() {
    this.canvasContainer = document.querySelector('.canvas-scroll-area') as HTMLElement;
  }

  ngOnDestroy() {
    this.routesService.remove([this.ROUTE_NAME]);
    this.destroy$.next();
    this.destroy$.complete();
    this.cleanupListeners();
  }

  private cleanupListeners() {
    document.removeEventListener('mousemove', this.onResize);
    document.removeEventListener('mouseup', this.onResizeEnd);
    document.removeEventListener('mousemove', this.onRotate);
    document.removeEventListener('mouseup', this.onRotateEnd);
    document.removeEventListener('mousemove', this.onCanvasPan);
    document.removeEventListener('mouseup', this.onCanvasPanEnd);
  }

  goBack() {
    this.router.navigate(['/inventory/warehouses']);
  }

  toM(px: number | null | undefined): number {
    return px == null ? 0 : +(px / this.PX_PER_M).toFixed(2);
  }
  toPx(m: number | null | undefined): number {
    return m == null ? 0 : Math.round(m * this.PX_PER_M);
  }
  snapToGrid(v: number): number {
    return Math.round(v / this.SNAP_GRID) * this.SNAP_GRID;
  }

  loadWarehouseInfo() {
    this.warehouseService
      .get(this.warehouseId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.warehouse = res;
        this.routesService.add([
          {
            path: `/inventory/warehouses/layouts/${this.warehouseId}`,
            name: this.ROUTE_NAME,
            parentName: '::Menu:Warehouses',
            iconClass: 'fas fa-map',
            layout: eLayoutType.application,
          },
        ]);
        if (!this._initialZoomDone && this.zones.length >= 0) this.doInitialZoomFit();
        this.cdr.markForCheck();
      });
  }

  // ═══════════════════════════════════════════════════════════════════
  // refreshMap — MUTATE in-place, NEVER replace array reference
  // ═══════════════════════════════════════════════════════════════════
  refreshMap(focusId: string | null = null, isInitial = false) {
    forkJoin({
      zones: this.warehouseService.getZones(this.warehouseId),
      bins: this.warehouseService.getStorageBins(this.warehouseId),
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ zones: svZones, bins: svBins }) => {
        // ─ ZONES ─────────────────────────────────────────────
        const exMap = new Map(this.zones.map(z => [z.id, z]));
        const svSet = new Set(svZones.map(z => z.id));

        // Remove deleted zones
        for (let i = this.zones.length - 1; i >= 0; i--) {
          if (!svSet.has(this.zones[i].id)) {
            this.zoneDragPos.delete(this.zones[i].id);
            this.zones.splice(i, 1);
          }
        }

        for (const sv of svZones) {
          const ex = exMap.get(sv.id);
          if (ex) {
            // Existing zone: update display fields only, KEEP position/size intact
            // so CDK drag positions are untouched
            ex.name = sv.name;
            ex.code = sv.code;
            ex.type = sv.type;
            ex.storageCondition = sv.storageCondition;
            ex.color = sv.color;
            ex.rotation = sv.rotation;
            if (!ex._isDirty) {
              // Safe to sync layout from server if no local unsaved changes
              const posChanged =
                ex.positionX !== sv.positionX ||
                ex.positionY !== sv.positionY ||
                ex.width !== sv.width ||
                ex.length !== sv.length;
              ex.positionX = sv.positionX;
              ex.positionY = sv.positionY;
              ex.width = sv.width;
              ex.length = sv.length;
              if (posChanged) {
                // Only update drag pos Map when server data actually changed
                this.zoneDragPos.set(ex.id, {
                  x: ex.positionX * this.scale,
                  y: ex.positionY * this.scale,
                });
              }
            }
            // Reset dirty flag after sync
            ex._isDirty = false;
          } else {
            // New zone from server → push + init drag pos
            const nz: CanvasZone = { ...sv, _isDirty: false, _hasCollision: false };
            this.zones.push(nz);
            this.zoneDragPos.set(nz.id, {
              x: nz.positionX * this.scale,
              y: nz.positionY * this.scale,
            });
          }
        }

        // ─ BINS ──────────────────────────────────────────────
        const exBinMap = new Map(this.bins.map(b => [b.id, b]));
        const svBinSet = new Set(svBins.map(b => b.id));

        for (let i = this.bins.length - 1; i >= 0; i--) {
          if (!svBinSet.has(this.bins[i].id)) {
            this.binDragPos.delete(this.bins[i].id);
            this.bins.splice(i, 1);
          }
        }

        for (const sb of svBins) {
          const eb = exBinMap.get(sb.id);
          if (eb) {
            eb.code = sb.code;
            eb.zoneId = sb.zoneId;
            eb.maxSKU = sb.maxSKU;
            eb.isBlocked = sb.isBlocked;
            eb.rotation = sb.rotation;
            if (!eb._isDirty) {
              eb.positionX = sb.positionX;
              eb.positionY = sb.positionY;
              eb.width = sb.width;
              eb.length = sb.length;
              this.binDragPos.set(eb.id, {
                x: eb.positionX * this.scale,
                y: eb.positionY * this.scale,
              });
            }
            eb._isDirty = false;
          } else {
            const nb: CanvasBin = { ...sb, _isDirty: false, _hasCollision: false };
            this.bins.push(nb);
            this.binDragPos.set(nb.id, {
              x: nb.positionX * this.scale,
              y: nb.positionY * this.scale,
            });
          }
        }

        this.hasUnsavedChanges = false;
        this.checkAllCollisions();

        if (focusId) {
          const fz = this.zones.find(z => z.id === focusId);
          if (fz) {
            this.activeZone = fz;
            this.activeTab = 'zones';
          }
        }

        if (isInitial && !this._initialZoomDone) this.doInitialZoomFit();

        this.cdr.markForCheck();
      });
  }

  // ─── INITIAL ZOOM FIT ────────────────────────────────────────────
  private doInitialZoomFit() {
    if (!this.warehouse) return;
    setTimeout(() => {
      if (!this.canvasContainer)
        this.canvasContainer = document.querySelector('.canvas-scroll-area') as HTMLElement;
      this.zoomFit(false); // false = don't rebuild drag pos (done separately)
      this.rebuildAllDragPositions();
      this._initialZoomDone = true;
      this.cdr.markForCheck();
    }, 250);
  }

  rebuildAllDragPositions() {
    this.zones.forEach(z =>
      this.zoneDragPos.set(z.id, { x: z.positionX * this.scale, y: z.positionY * this.scale }),
    );
    this.bins.forEach(b =>
      this.binDragPos.set(b.id, { x: b.positionX * this.scale, y: b.positionY * this.scale }),
    );
  }

  // ─── ZONE HELPERS ───────────────────────────────────────────────
  getColorForZoneType(type: ZoneType, condition?: StorageCondition): string {
    if (type === ZoneType.Storage) {
      switch (condition) {
        case StorageCondition.Normal:
          return '#00B894';
        case StorageCondition.Cool:
          return '#0984E3';
        case StorageCondition.Cold:
          return '#4A90D9';
        case StorageCondition.Frozen:
          return '#2D3FE7';
        default:
          return '#00B894';
      }
    }
    switch (type) {
      case ZoneType.Inbound:
        return '#00CEC9';
      case ZoneType.Outbound:
        return '#E17055';
      case ZoneType.Staging:
        return '#F9CA24';
      case ZoneType.Quarantine:
        return '#E84393';
      case ZoneType.ForkliftParking:
        return '#636E72';
      case ZoneType.Office:
        return '#A29BFE';
      default:
        return '#B2BEC3';
    }
  }

  getZoneTypeIcon(type: ZoneType): string {
    switch (type) {
      case ZoneType.Storage:
        return 'fa-th-large';
      case ZoneType.Inbound:
        return 'fa-arrow-circle-down';
      case ZoneType.Outbound:
        return 'fa-arrow-circle-up';
      case ZoneType.Staging:
        return 'fa-layer-group';
      case ZoneType.Quarantine:
        return 'fa-exclamation-circle';
      case ZoneType.ForkliftParking:
        return 'fa-dolly';
      case ZoneType.Office:
        return 'fa-building';
      default:
        return 'fa-square';
    }
  }

  getZoneTypeName(type: ZoneType, condition?: StorageCondition): string {
    if (type === ZoneType.Storage) {
      switch (condition) {
        case StorageCondition.Normal:
          return 'Normal Storage';
        case StorageCondition.Cool:
          return 'Cool Storage';
        case StorageCondition.Cold:
          return 'Cold Storage';
        case StorageCondition.Frozen:
          return 'Frozen Storage';
        default:
          return 'Storage';
      }
    }
    switch (type) {
      case ZoneType.Inbound:
        return 'Inbound';
      case ZoneType.Outbound:
        return 'Outbound';
      case ZoneType.Staging:
        return 'Staging';
      case ZoneType.Quarantine:
        return 'Quarantine';
      case ZoneType.ForkliftParking:
        return 'Parking';
      case ZoneType.Office:
        return 'Office';
      default:
        return 'Zone';
    }
  }

  isStorageZoneType(t: number): boolean {
    return t === ZoneType.Storage;
  }
  getBinCount(zid: string): number {
    return this.bins.filter(b => b.zoneId === zid).length;
  }
  getBinsOfZone(zid: string): CanvasBin[] {
    return this.bins.filter(b => b.zoneId === zid);
  }
  toggleZoneDropdown(id: string) {
    this.expandedZones.has(id) ? this.expandedZones.delete(id) : this.expandedZones.add(id);
  }
  isZoneExpanded(id: string): boolean {
    return this.expandedZones.has(id);
  }
  trackById(_: number, item: { id?: string }) {
    return item?.id;
  }

  // ─── CREATE FROM TEMPLATE ────────────────────────────────────────
  createZoneFromTemplate(type: ZoneType, condition: StorageCondition) {
    const mapW = this.warehouse?.mapWidth || 2000;
    const mapH = this.warehouse?.mapLength || 2000;
    const zW = this.toPx(10),
      zH = this.toPx(10);
    let posX = this.snapToGrid(mapW / 2 - zW / 2);
    let posY = this.snapToGrid(mapH / 2 - zH / 2);
    for (let i = 0; i < 30; i++) {
      if (
        !this.zones.some(z =>
          this.rectsOverlap({ positionX: posX, positionY: posY, width: zW, length: zH }, z),
        )
      )
        break;
      posX = this.snapToGrid(posX + this.toPx(3));
      posY = this.snapToGrid(posY + this.toPx(3));
      if (posX + zW > mapW) posX = 20;
      if (posY + zH > mapH) posY = 20;
    }
    this.warehouseService
      .createZone({
        warehouseId: this.warehouseId,
        name: this.getZoneTypeName(type, condition),
        type,
        storageCondition: condition,
        color: this.getColorForZoneType(type, condition),
        positionX: posX,
        positionY: posY,
        width: zW,
        length: zH,
        rotation: 0,
      } as any)
      .subscribe(c => this.refreshMap(c.id));
  }

  // ─── COLLISION & SNAP ────────────────────────────────────────────
  private rectsOverlap(a: any, b: any): boolean {
    return !(
      a.positionX + a.width <= b.positionX ||
      b.positionX + b.width <= a.positionX ||
      a.positionY + a.length <= b.positionY ||
      b.positionY + b.length <= a.positionY
    );
  }

  private applyMagneticSnap(item: any, targets: any[]) {
    const T = this.SNAP_THRESHOLD;
    for (const t of targets) {
      if (t.id === item.id) continue;
      if (Math.abs(item.positionX + item.width - t.positionX) < T)
        item.positionX = t.positionX - item.width;
      else if (Math.abs(item.positionX - (t.positionX + t.width)) < T)
        item.positionX = t.positionX + t.width;
      else if (Math.abs(item.positionX - t.positionX) < T) item.positionX = t.positionX;
      if (Math.abs(item.positionY + item.length - t.positionY) < T)
        item.positionY = t.positionY - item.length;
      else if (Math.abs(item.positionY - (t.positionY + t.length)) < T)
        item.positionY = t.positionY + t.length;
      else if (Math.abs(item.positionY - t.positionY) < T) item.positionY = t.positionY;
    }
  }

  checkAllCollisions() {
    this.zones.forEach(z => (z._hasCollision = false));
    for (let i = 0; i < this.zones.length; i++)
      for (let j = i + 1; j < this.zones.length; j++)
        if (this.rectsOverlap(this.zones[i], this.zones[j]))
          this.zones[i]._hasCollision = this.zones[j]._hasCollision = true;
    this.bins.forEach(b => (b._hasCollision = false));
    this.zones.forEach(z => {
      const bz = this.getBinsOfZone(z.id);
      for (let i = 0; i < bz.length; i++)
        for (let j = i + 1; j < bz.length; j++)
          if (this.rectsOverlap(bz[i], bz[j])) bz[i]._hasCollision = bz[j]._hasCollision = true;
    });
  }

  // ─── ZOOM ────────────────────────────────────────────────────────
  get scalePercent(): number {
    return Math.round(this.scale * 100);
  }

  zoomIn() {
    const n = this.ZOOM_LEVELS.find(z => z > this.scale + 0.001);
    if (n) {
      this.scale = n;
      this.rebuildAllDragPositions();
      this.cdr.markForCheck();
    }
  }

  zoomOut() {
    const p = [...this.ZOOM_LEVELS].reverse().find(z => z < this.scale - 0.001);
    if (p) {
      this.scale = p;
      this.rebuildAllDragPositions();
      this.cdr.markForCheck();
    }
  }

  zoomFit(doRebuild = true) {
    if (!this.canvasContainer)
      this.canvasContainer = document.querySelector('.canvas-scroll-area') as HTMLElement;
    if (!this.canvasContainer || !this.warehouse) return;
    const w = this.canvasContainer.clientWidth - 64;
    const h = this.canvasContainer.clientHeight - 64;
    const s = Math.max(
      0.15,
      Math.min(
        1.5,
        Math.min(w / (this.warehouse.mapWidth || 2000), h / (this.warehouse.mapLength || 2000)),
      ),
    );
    this.scale = +s.toFixed(3);
    if (doRebuild) this.rebuildAllDragPositions();
    setTimeout(() => {
      if (this.canvasContainer) {
        this.canvasContainer.scrollLeft = Math.max(
          0,
          (this.canvasContainer.scrollWidth - this.canvasContainer.clientWidth) / 2,
        );
        this.canvasContainer.scrollTop = Math.max(
          0,
          (this.canvasContainer.scrollHeight - this.canvasContainer.clientHeight) / 2,
        );
      }
    }, 50);
    this.cdr.markForCheck();
  }

  onWheel(event: WheelEvent) {
    if (event.ctrlKey || event.metaKey) {
      event.preventDefault();
      const ns = Math.max(0.15, Math.min(2.0, this.scale * (event.deltaY > 0 ? 0.9 : 1.1)));
      this.scale = +ns.toFixed(3);
      this.rebuildAllDragPositions();
      this.cdr.markForCheck();
    }
  }

  // ─── PAN ─────────────────────────────────────────────────────────
  onCanvasMouseDown(event: MouseEvent) {
    if (event.button === 1 || event.button === 2) {
      event.preventDefault();
      this.isPanning = true;
      this.panStartMouse = { x: event.clientX, y: event.clientY };
      if (this.canvasContainer)
        this.panStartScroll = {
          x: this.canvasContainer.scrollLeft,
          y: this.canvasContainer.scrollTop,
        };
      document.addEventListener('mousemove', this.onCanvasPan);
      document.addEventListener('mouseup', this.onCanvasPanEnd);
    }
  }

  onCanvasPan = (e: MouseEvent) => {
    if (!this.isPanning || !this.canvasContainer) return;
    this.canvasContainer.scrollLeft = this.panStartScroll.x - (e.clientX - this.panStartMouse.x);
    this.canvasContainer.scrollTop = this.panStartScroll.y - (e.clientY - this.panStartMouse.y);
  };

  onCanvasPanEnd = () => {
    this.isPanning = false;
    document.removeEventListener('mousemove', this.onCanvasPan);
    document.removeEventListener('mouseup', this.onCanvasPanEnd);
  };

  // ─── CANVAS CLICKS ───────────────────────────────────────────────
  onZoneSingleClick(zone: CanvasZone, event: MouseEvent) {
    if (this.isResizing || this.isRotating) return;
    event.stopPropagation();
    this.activeZone = zone;
    this.activeTab = 'bins';
    this.expandedZones.add(zone.id);
    this.cdr.markForCheck();
  }

  onZoneDoubleClick(zone: CanvasZone, event: MouseEvent) {
    event.stopPropagation();
    this.openZoneDrawer(zone);
  }

  onBinDoubleClick(bin: CanvasBin, event: MouseEvent) {
    event.stopPropagation();
    this.openBinDrawer(bin);
  }

  onCanvasClick(event: MouseEvent) {
    const t = event.target as HTMLElement;
    if (!t.closest('.zone-drag-wrapper') && !t.closest('.bin-drag-wrapper')) {
      this.activeZone = null;
      this.closeDrawer();
      this.cdr.markForCheck();
    }
  }

  // ─── DRAG ────────────────────────────────────────────────────────
  onZoneDragEnded(event: CdkDragEnd, zone: CanvasZone) {
    if (this.isResizing) {
      return;
    }
    const mapW = this.warehouse?.mapWidth || 2000,
      mapH = this.warehouse?.mapLength || 2000;
    const raw = event.source.getFreeDragPosition();
    const oldX = zone.positionX,
      oldY = zone.positionY;
    zone.positionX = this.snapToGrid(
      Math.max(0, Math.min(Math.round(raw.x / this.scale), mapW - zone.width)),
    );
    zone.positionY = this.snapToGrid(
      Math.max(0, Math.min(Math.round(raw.y / this.scale), mapH - zone.length)),
    );
    this.applyMagneticSnap(zone, this.zones);
    const dX = zone.positionX - oldX,
      dY = zone.positionY - oldY;
    if (this.zones.filter(z => z.id !== zone.id).some(z => this.rectsOverlap(zone, z))) {
      zone.positionX = oldX;
      zone.positionY = oldY;
    } else {
      zone._isDirty = true;
      this.hasUnsavedChanges = true;
      this.getBinsOfZone(zone.id).forEach(b => {
        b.positionX += dX;
        b.positionY += dY;
        b._isDirty = true;
        this.binDragPos.set(b.id, { x: b.positionX * this.scale, y: b.positionY * this.scale });
      });
    }
    this.zoneDragPos.set(zone.id, {
      x: zone.positionX * this.scale,
      y: zone.positionY * this.scale,
    });
    event.source.setFreeDragPosition(this.zoneDragPos.get(zone.id)!);
    this.checkAllCollisions();
    this.cdr.markForCheck();
  }

  onBinDragEnded(event: CdkDragEnd, bin: CanvasBin, zone: CanvasZone) {
    const raw = event.source.getFreeDragPosition();
    const oldX = bin.positionX,
      oldY = bin.positionY;
    let lx = this.snapToGrid(Math.round(raw.x / this.scale));
    let ly = this.snapToGrid(Math.round(raw.y / this.scale));
    lx = Math.max(zone.positionX, Math.min(lx, zone.positionX + zone.width - bin.width));
    ly = Math.max(zone.positionY, Math.min(ly, zone.positionY + zone.length - bin.length));
    bin.positionX = lx;
    bin.positionY = ly;
    this.applyMagneticSnap(bin, this.getBinsOfZone(zone.id));
    bin.positionX = Math.max(
      zone.positionX,
      Math.min(bin.positionX, zone.positionX + zone.width - bin.width),
    );
    bin.positionY = Math.max(
      zone.positionY,
      Math.min(bin.positionY, zone.positionY + zone.length - bin.length),
    );
    if (
      this.getBinsOfZone(zone.id)
        .filter(b => b.id !== bin.id)
        .some(b => this.rectsOverlap(bin, b))
    ) {
      bin.positionX = oldX;
      bin.positionY = oldY;
    } else {
      bin._isDirty = true;
      this.hasUnsavedChanges = true;
    }
    this.binDragPos.set(bin.id, { x: bin.positionX * this.scale, y: bin.positionY * this.scale });
    event.source.setFreeDragPosition(this.binDragPos.get(bin.id)!);
    this.checkAllCollisions();
    this.cdr.markForCheck();
  }

  // ─── RESIZE ──────────────────────────────────────────────────────
  onResizeStart(
    event: MouseEvent,
    item: CanvasZone | CanvasBin,
    type: 'zone' | 'bin',
    handle: ResizeHandle,
  ) {
    event.stopPropagation();
    event.preventDefault();
    this.isResizing = true;
    this.resizingItem = { item, type, handle };
    this.resizeStartMouse = { x: event.clientX, y: event.clientY };
    this.resizeStartRect = { x: item.positionX, y: item.positionY, w: item.width, h: item.length };
    document.addEventListener('mousemove', this.onResize);
    document.addEventListener('mouseup', this.onResizeEnd);
  }

  onResize = (event: MouseEvent) => {
    if (!this.isResizing || !this.resizingItem) return;
    const { item, type, handle } = this.resizingItem;
    const dx = (event.clientX - this.resizeStartMouse.x) / this.scale;
    const dy = (event.clientY - this.resizeStartMouse.y) / this.scale;
    const { x: ox, y: oy, w: ow, h: oh } = this.resizeStartRect;
    const min = type === 'zone' ? this.toPx(1) : this.toPx(0.5);
    const mapW = this.warehouse?.mapWidth || 2000,
      mapH = this.warehouse?.mapLength || 2000;
    let nx = ox,
      ny = oy,
      nw = ow,
      nh = oh;
    if (handle === 'e' || handle === 'ne' || handle === 'se')
      nw = Math.max(min, Math.min(ow + dx, mapW - ox));
    if (handle === 'w' || handle === 'nw' || handle === 'sw') {
      const px = Math.max(0, Math.min(ox + dx, ox + ow - min));
      nw = ow - (px - ox);
      nx = px;
    }
    if (handle === 's' || handle === 'sw' || handle === 'se')
      nh = Math.max(min, Math.min(oh + dy, mapH - oy));
    if (handle === 'n' || handle === 'nw' || handle === 'ne') {
      const py = Math.max(0, Math.min(oy + dy, oy + oh - min));
      nh = oh - (py - oy);
      ny = py;
    }
    item.positionX = this.snapToGrid(Math.round(nx));
    item.positionY = this.snapToGrid(Math.round(ny));
    item.width = this.snapToGrid(Math.round(nw));
    item.length = this.snapToGrid(Math.round(nh));
    if (type === 'zone') {
      this.applyMagneticSnap(item, this.zones);
      if (this.zones.filter(z => z.id !== item.id).some(z => this.rectsOverlap(item, z))) {
        item.positionX = ox;
        item.positionY = oy;
        item.width = ow;
        item.length = oh;
      } else {
        (item as CanvasZone)._isDirty = true;
        this.hasUnsavedChanges = true;
      }
      this.zoneDragPos.set(item.id, {
        x: item.positionX * this.scale,
        y: item.positionY * this.scale,
      });
    } else {
      const tz = this.zones.find(z => z.id === (item as CanvasBin).zoneId);
      if (tz) this.applyMagneticSnap(item, this.getBinsOfZone(tz.id));
      if (
        this.getBinsOfZone((item as CanvasBin).zoneId)
          .filter(b => b.id !== item.id)
          .some(b => this.rectsOverlap(item, b))
      ) {
        item.positionX = ox;
        item.positionY = oy;
        item.width = ow;
        item.length = oh;
      } else {
        (item as CanvasBin)._isDirty = true;
        this.hasUnsavedChanges = true;
      }
    }
    this.checkAllCollisions();
    this.cdr.markForCheck();
  };

  onResizeEnd = () => {
    this.isResizing = false;
    this.resizingItem = null;
    document.removeEventListener('mousemove', this.onResize);
    document.removeEventListener('mouseup', this.onResizeEnd);
    this.cdr.markForCheck();
  };

  // ─── ROTATE ──────────────────────────────────────────────────────
  onRotateStart(event: MouseEvent, item: any) {
    event.stopPropagation();
    event.preventDefault();
    this.isRotating = true;
    this.rotatingItem = { item };
    const el = (event.target as HTMLElement).closest('.zone-inner,.bin-inner') as HTMLElement;
    if (el) {
      const r = el.getBoundingClientRect();
      this.rotateCenterCanvas = { x: r.left + r.width / 2, y: r.top + r.height / 2 };
    }
    document.addEventListener('mousemove', this.onRotate);
    document.addEventListener('mouseup', this.onRotateEnd);
  }

  onRotate = (e: MouseEvent) => {
    if (!this.isRotating || !this.rotatingItem) return;
    let a = Math.round(
      Math.atan2(e.clientY - this.rotateCenterCanvas.y, e.clientX - this.rotateCenterCanvas.x) *
        (180 / Math.PI) +
        90,
    );
    if (a < 0) a += 360;
    if (a >= 360) a -= 360;
    if (e.shiftKey) a = Math.round(a / 15) * 15;
    this.rotatingItem.item.rotation = a;
    this.rotatingItem.item._isDirty = true;
    this.hasUnsavedChanges = true;
    this.cdr.markForCheck();
  };

  onRotateEnd = () => {
    this.isRotating = false;
    this.rotatingItem = null;
    document.removeEventListener('mousemove', this.onRotate);
    document.removeEventListener('mouseup', this.onRotateEnd);
  };

  // ─── FONT ────────────────────────────────────────────────────────
  getZoneFontSize(z: CanvasZone): number {
    return Math.max(8, Math.min(16, Math.min(z.width * this.scale, z.length * this.scale) * 0.12));
  }
  getBinFontSize(b: CanvasBin): number {
    return Math.max(7, Math.min(12, Math.min(b.width * this.scale, b.length * this.scale) * 0.22));
  }

  // ─── SAVE ────────────────────────────────────────────────────────
  async saveAllLayouts() {
    if (!this.hasUnsavedChanges || this.isSaving) return;
    this.isSaving = true;
    this.cdr.markForCheck();
    try {
      for (const z of this.zones.filter(z => z._isDirty)) {
        await lastValueFrom(this.warehouseService.updateZone(z.id, z as any));
        z._isDirty = false;
      }
      for (const b of this.bins.filter(b => b._isDirty)) {
        await lastValueFrom(this.warehouseService.updateStorageBin(b.id, b as any));
        b._isDirty = false;
      }
      this.hasUnsavedChanges = false;
    } catch (e) {
      console.error(e);
    } finally {
      this.isSaving = false;
      this.cdr.markForCheck();
    }
  }

  // ─── PRINT ───────────────────────────────────────────────────────
  printDiagram() {
    const mapW = this.warehouse?.mapWidth || 2000,
      mapH = this.warehouse?.mapLength || 2000;
    const printScale = Math.min(1100 / mapW, 760 / mapH, 1.0);
    const origScale = this.scale;
    this.scale = +printScale.toFixed(3);
    this.rebuildAllDragPositions();
    this.cdr.markForCheck();
    setTimeout(() => {
      const canvas = document.querySelector('.map-canvas') as HTMLElement;
      if (!canvas) {
        this.scale = origScale;
        this.rebuildAllDragPositions();
        return;
      }
      const styles = Array.from(document.styleSheets)
        .map(ss => {
          try {
            return Array.from(ss.cssRules)
              .map(r => r.cssText)
              .join('\n');
          } catch {
            return '';
          }
        })
        .join('\n');
      const cloned = canvas.cloneNode(true) as HTMLElement;
      cloned.style.position = 'relative';
      cloned.style.margin = '0 auto';
      const w = window.open('', '_blank', 'width=840,height=680');
      if (!w) return;
      w.document.write(`<!DOCTYPE html><html><head>
        <title>${this.warehouse?.name} — Warehouse Map</title>
        <style>
          @page{size:A3 landscape;margin:8mm}
          *{box-sizing:border-box}
          body{margin:0;background:#fff;font-family:sans-serif}
          .ph{display:flex;justify-content:space-between;align-items:flex-end;padding:8px 16px;border-bottom:2px solid #1E2330;margin-bottom:12px}
          .ph h1{margin:0;font-size:18px;color:#1E2330}
          .ph p{margin:0;font-size:11px;color:#666}
          .cw{display:flex;justify-content:center;padding:8px}
          ${styles}
          .rh,.rotate-handle,.zone-dirty-dot,.zone-bin-badge{display:none!important}
        </style></head><body>
        <div class="ph">
          <div><h1>📦 ${this.warehouse?.name}</h1>
          <p>${this.toM(mapW)} × ${this.toM(mapH)} m &nbsp;·&nbsp; ${this.zones.length} zones &nbsp;·&nbsp; ${this.bins.length} bins</p></div>
          <p>Printed: ${new Date().toLocaleString()}</p>
        </div>
        <div class="cw">${cloned.outerHTML}</div>
        </body></html>`);
      w.document.close();
      setTimeout(() => {
        w.print();
        w.close();
        this.scale = origScale;
        this.rebuildAllDragPositions();
        this.cdr.markForCheck();
      }, 800);
    }, 350);
  }

  // ─── ZONE DRAWER ─────────────────────────────────────────────────
  openZoneDrawer(zone?: CanvasZone) {
    this.selectedZone = zone || null;
    this.selectedBin = null;
    this.drawerType = 'ZONE';
    this.form = this.fb.group({
      warehouseId: [this.warehouseId],
      name: [zone?.name || '', [Validators.required, Validators.maxLength(255)]],
      type: [zone?.type ?? ZoneType.Storage, Validators.required],
      storageCondition: [zone?.storageCondition ?? StorageCondition.Normal, Validators.required],
      color: [zone?.color || this.getColorForZoneType(ZoneType.Storage, StorageCondition.Normal)],
      positionX: [this.toM(zone?.positionX ?? 0), [Validators.required, Validators.min(0)]],
      positionY: [this.toM(zone?.positionY ?? 0), [Validators.required, Validators.min(0)]],
      width: [this.toM(zone?.width ?? this.toPx(10)), [Validators.required, Validators.min(1)]],
      length: [this.toM(zone?.length ?? this.toPx(10)), [Validators.required, Validators.min(1)]],
      rotation: [zone?.rotation ?? 0, [Validators.min(0), Validators.max(360)]],
    });
    this.form
      .get('type')
      .valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(t => {
        if (
          t === ZoneType.Storage &&
          this.form.get('storageCondition').value === StorageCondition.Other
        )
          this.form.get('storageCondition').setValue(StorageCondition.Normal, { emitEvent: false });
        else if (t !== ZoneType.Storage)
          this.form.get('storageCondition').setValue(StorageCondition.Other, { emitEvent: false });
        this.form
          .get('color')
          .setValue(this.getColorForZoneType(t, this.form.get('storageCondition').value), {
            emitEvent: false,
          });
      });
    this.form.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(val => {
      if (!this.selectedZone || this.isSaving) return;
      this.selectedZone.positionX = this.toPx(val.positionX);
      this.selectedZone.positionY = this.toPx(val.positionY);
      this.selectedZone.width = this.toPx(val.width);
      this.selectedZone.length = this.toPx(val.length);
      this.selectedZone.color = val.color;
      this.selectedZone.rotation = parseFloat(val.rotation) || 0;
      this.zoneDragPos.set(this.selectedZone.id, {
        x: this.selectedZone.positionX * this.scale,
        y: this.selectedZone.positionY * this.scale,
      });
      this.checkAllCollisions();
      this.cdr.markForCheck();
    });
    this.cdr.markForCheck();
  }

  saveZone() {
    if (this.form.invalid) return;
    const v = this.form.value;
    const p = {
      ...v,
      positionX: this.toPx(v.positionX),
      positionY: this.toPx(v.positionY),
      width: this.toPx(v.width),
      length: this.toPx(v.length),
      rotation: parseFloat(v.rotation) || 0,
    };
    (this.selectedZone?.id
      ? this.warehouseService.updateZone(this.selectedZone.id, p)
      : this.warehouseService.createZone(p)
    ).subscribe(r => {
      this.closeDrawer();
      this.refreshMap(r?.id);
    });
  }

  deleteZone(zone: CanvasZone, event?: MouseEvent) {
    event?.stopPropagation();
    this.confirmation
      .warn('::ZoneDeletionWarningMessage', '::AreYouSure', {
        messageLocalizationParams: [zone.name],
      })
      .subscribe(s => {
        if (s !== Confirmation.Status.confirm) return;
        this.warehouseService.deleteZone(zone.id).subscribe(() => {
          if (this.activeZone?.id === zone.id) this.activeZone = null;
          this.refreshMap();
        });
      });
  }

  // ─── BIN DRAWER ──────────────────────────────────────────────────
  createBinInActiveZone() {
    if (this.activeZone) this.openBinDrawer();
  }

  openBinDrawer(bin?: CanvasBin) {
    const defaultZoneId = this.activeZone?.id || this.zones[0]?.id || null;
    const tz = this.zones.find(z => z.id === (bin?.zoneId || defaultZoneId));
    const dx = bin ? 0 : tz ? tz.positionX + (tz.width - this.toPx(2)) / 2 : 0;
    const dy = bin ? 0 : tz ? tz.positionY + (tz.length - this.toPx(2)) / 2 : 0;
    this.selectedBin = bin || null;
    this.selectedZone = null;
    this.drawerType = 'BIN';
    this.form = this.fb.group({
      warehouseId: [this.warehouseId],
      zoneId: [bin?.zoneId || defaultZoneId, Validators.required],
      positionX: [this.toM(bin?.positionX ?? dx), Validators.required],
      positionY: [this.toM(bin?.positionY ?? dy), Validators.required],
      width: [this.toM(bin?.width ?? this.toPx(2)), [Validators.required, Validators.min(0.5)]],
      length: [this.toM(bin?.length ?? this.toPx(2)), [Validators.required, Validators.min(0.5)]],
      rotation: [bin?.rotation ?? 0, [Validators.min(0), Validators.max(360)]],
      maxSKU: [bin?.maxSKU ?? 0, Validators.min(0)],
      isBlocked: [bin?.isBlocked ?? false],
    });
    this.form
      .get('zoneId')
      .valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(zid => {
        const nz = this.zones.find(z => z.id === zid);
        if (nz)
          this.form.patchValue(
            {
              positionX: this.toM(
                nz.positionX + (nz.width - this.toPx(this.form.get('width').value)) / 2,
              ),
              positionY: this.toM(
                nz.positionY + (nz.length - this.toPx(this.form.get('length').value)) / 2,
              ),
            },
            { emitEvent: false },
          );
      });
    this.form.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(val => {
      if (!this.selectedBin || this.isSaving) return;
      this.selectedBin.positionX = this.toPx(val.positionX);
      this.selectedBin.positionY = this.toPx(val.positionY);
      this.selectedBin.width = this.toPx(val.width);
      this.selectedBin.length = this.toPx(val.length);
      this.selectedBin.rotation = parseFloat(val.rotation) || 0;
      this.checkAllCollisions();
      this.cdr.markForCheck();
    });
    this.cdr.markForCheck();
  }

  saveBin() {
    if (this.form.invalid) return;
    const v = this.form.value;
    const p = {
      ...v,
      positionX: this.toPx(v.positionX),
      positionY: this.toPx(v.positionY),
      width: this.toPx(v.width),
      length: this.toPx(v.length),
      rotation: parseFloat(v.rotation) || 0,
    };
    const tz = this.zones.find(z => z.id === p.zoneId);
    if (tz) {
      p.positionX = Math.max(
        tz.positionX,
        Math.min(p.positionX, tz.positionX + tz.width - p.width),
      );
      p.positionY = Math.max(
        tz.positionY,
        Math.min(p.positionY, tz.positionY + tz.length - p.length),
      );
    }
    (this.selectedBin?.id
      ? this.warehouseService.updateStorageBin(this.selectedBin.id, p)
      : this.warehouseService.createStorageBin(p)
    ).subscribe(() => {
      this.closeDrawer();
      this.refreshMap();
    });
  }

  deleteBin(bin: CanvasBin, event?: MouseEvent) {
    event?.stopPropagation();
    this.confirmation
      .warn('::BinDeletionWarningMessage', '::AreYouSure', {
        messageLocalizationParams: [bin.code],
      })
      .subscribe(s => {
        if (s === Confirmation.Status.confirm)
          this.warehouseService.deleteStorageBin(bin.id).subscribe(() => this.refreshMap());
      });
  }

  closeDrawer() {
    this.drawerType = null;
    this.selectedZone = null;
    this.selectedBin = null;
    this.cdr.markForCheck();
  }
}
