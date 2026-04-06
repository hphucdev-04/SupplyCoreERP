import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { RoutesService, eLayoutType } from '@abp/ng.core';
import { WarehouseService } from 'src/app/proxy/warehouses';
import { WarehouseDto, ZoneDto, BinDto } from 'src/app/proxy/warehouses/dtos';
import { ZoneType, zoneTypeOptions } from 'src/app/proxy/enums/warehouses';
import { StorageCondition, storageConditionOptions } from 'src/app/proxy/enums/medicines';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { Subject, forkJoin, lastValueFrom } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { DragDropModule, CdkDragEnd } from '@angular/cdk/drag-drop';
import { SharedModule } from 'src/app/shared/shared.module';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';


type ResizeHandle = 'n' | 's' | 'e' | 'w' | 'nw' | 'ne' | 'sw' | 'se';

@Component({
  selector: 'app-storage-locations',
  standalone: true,
  imports: [DragDropModule, SharedModule, DrawerComponent],
  templateUrl: './storage-locations.component.html',
  styleUrls: ['./storage-locations.component.scss'],
})
export class StorageLocationsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  warehouseId: string;
  warehouse: WarehouseDto;

  zones: ZoneDto[] = [];
  bins: BinDto[] = [];

  drawerType: 'ZONE' | 'BIN' | null = null;
  form: FormGroup;
  selectedZone: ZoneDto | null = null;
  selectedBin: BinDto | null = null;

  activeZone: ZoneDto | null = null;

  hasUnsavedChanges = false;
  isSaving = false;
  scale = 0.5;
  activeTab: 'zones' | 'bins' = 'zones';

  // --- TỶ LỆ QUY ĐỔI (THỰC TẾ) ---
  // 1 mét (m) = 20 pixels (px) trên bản vẽ
  readonly PX_PER_M = 20; 
  private readonly SNAP_THRESHOLD = 15; // Lực hít nam châm (px)

  // Resize state
  isResizing = false;
  resizingItem: { item: any; type: 'zone' | 'bin'; handle: ResizeHandle } | null = null;
  resizeStartMouse = { x: 0, y: 0 };
  resizeStartRect  = { x: 0, y: 0, w: 0, h: 0 }; 

  // Rotate state
  isRotating = false;
  rotatingItem: { item: any } | null = null;
  rotateCenterCanvas = { x: 0, y: 0 }; 
  
  // Pan state (right-click drag)
  isPanning = false;
  panStartMouse = { x: 0, y: 0 };
  panStartScroll = { x: 0, y: 0 };
  private canvasElement: HTMLElement | null = null;
  
  // Dropdown state for bins panel
  expandedZones: Set<string> = new Set();

  zoneTypes = zoneTypeOptions;
  storageConditions = storageConditionOptions;
  
  // Expose enums to template
  ZoneType = ZoneType;
  StorageCondition = StorageCondition;

  private readonly ROUTE_NAME = '::Menu:StorageLocations:Dynamic';
  private zoneCounter = 1;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private routesService: RoutesService,
    private warehouseService: WarehouseService,
    private confirmation: ConfirmationService,
    private fb: FormBuilder,
  ) {}

  ngOnInit() {
    this.warehouseId = this.route.snapshot.paramMap.get('id');
    this.loadWarehouseInfo();
    this.refreshMap();
    
    // Get canvas scroll container after view init
    setTimeout(() => {
      this.canvasElement = document.querySelector('.map-wrapper .card-body') as HTMLElement;
    }, 100);
  }

  ngOnDestroy() {
    this.routesService.remove([this.ROUTE_NAME]);
    this.destroy$.next();
    this.destroy$.complete();
    document.removeEventListener('mousemove', this.onResize);
    document.removeEventListener('mouseup',   this.onResizeEnd);
    document.removeEventListener('mousemove', this.onRotate);
    document.removeEventListener('mouseup',   this.onRotateEnd);
    document.removeEventListener('mousemove', this.onCanvasPan);
    document.removeEventListener('mouseup',   this.onCanvasPanEnd);
  }

  goBack() {
    this.router.navigate(['/inventory/warehouses']);
  }

  // ============================================================
  // HELPER CHUYỂN ĐỔI MÉT <--> PIXEL
  // ============================================================
  toM(px: number | undefined | null): number {
    if (px == null) return 0;
    return Number((px / this.PX_PER_M).toFixed(2));
  }

  toPx(m: number | undefined | null): number {
    if (m == null) return 0;
    return Math.round(m * this.PX_PER_M);
  }

  loadWarehouseInfo() {
    this.warehouseService.get(this.warehouseId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(res => {
        this.warehouse = res;
        const currentPath = `/inventory/warehouses/${this.warehouseId}/locations`;
        this.routesService.add([{
          path: currentPath,
          name: this.ROUTE_NAME,
          parentName: '::Menu:Warehouses',
          iconClass: 'fas fa-map',
          layout: eLayoutType.application,
        }]);
      });
  }

  refreshMap() {
    forkJoin({
      zones: this.warehouseService.getZones(this.warehouseId),
      bins:  this.warehouseService.getStorageBins(this.warehouseId),
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe(({ zones, bins }) => {
      this.zones = zones;
      this.bins  = bins;
      
      this.zones.forEach(z => (z as any)['_isDirty'] = false);
      this.bins.forEach(b => (b as any)['_isDirty'] = false);
      
      this.hasUnsavedChanges = false;
      this.checkAllCollisions();
    });
  }

  getBinCount(zoneId: string): number {
    return this.bins.filter(b => b.zoneId === zoneId).length;
  }

  getZoneName(id: string): string {
    return this.zones.find(z => z.id === id)?.name || '---';
  }
  
  toggleZoneDropdown(zoneId: string) {
    if (this.expandedZones.has(zoneId)) {
      this.expandedZones.delete(zoneId);
    } else {
      this.expandedZones.add(zoneId);
    }
  }
  
  isZoneExpanded(zoneId: string): boolean {
    return this.expandedZones.has(zoneId);
  }

  getRandomColor(): string {
    const colors = ['#FF6B6B', '#4ECDC4', '#45B7D1', '#FFA07A', '#98D8C8', '#F7DC6F', '#BB8FCE', '#85C1E2'];
    return colors[Math.floor(Math.random() * colors.length)];
  }
  
  getColorForZoneType(type: ZoneType, condition?: StorageCondition): string {
    if (type === ZoneType.Storage) {
      switch (condition) {
        case StorageCondition.Normal: return '#4ECDC4';
        case StorageCondition.Cool: return '#45B7D1';
        case StorageCondition.Cold: return '#5DADE2';
        case StorageCondition.Frozen: return '#3498DB';
        default: return '#4ECDC4';
      }
    }
    
    switch (type) {
      case ZoneType.Inbound: return '#2ECC71';
      case ZoneType.Outbound: return '#E74C3C';
      case ZoneType.Staging: return '#F39C12';
      case ZoneType.Quarantine: return '#E67E22';
      case ZoneType.ForkliftParking: return '#95A5A6';
      case ZoneType.Office: return '#9B59B6';
      default: return '#BDC3C7';
    }
  }
  
  getZoneTypeName(type: ZoneType, condition?: StorageCondition): string {
    if (type === ZoneType.Storage) {
      switch (condition) {
        case StorageCondition.Normal: return 'Normal Storage';
        case StorageCondition.Cool: return 'Cool Storage';
        case StorageCondition.Cold: return 'Cold Storage';
        case StorageCondition.Frozen: return 'Frozen Storage';
        default: return 'Storage';
      }
    }
    
    switch (type) {
      case ZoneType.Inbound: return 'Inbound';
      case ZoneType.Outbound: return 'Outbound';
      case ZoneType.Staging: return 'Staging';
      case ZoneType.Quarantine: return 'Quarantine';
      case ZoneType.ForkliftParking: return 'Parking';
      case ZoneType.Office: return 'Office';
      default: return 'Zone';
    }
  }

  isStorageZoneType(type: number): boolean {
    return type === ZoneType.Storage;
  }

  // ============================================================
  // CREATE ZONE FROM TEMPLATE - Click vào shape để tạo zone ngay
  // ============================================================
  createZoneFromTemplate(type: ZoneType, condition: StorageCondition) {
    const zoneName = this.getZoneTypeName(type, condition);
    const zoneCode = `ZONE-${this.zoneCounter++}`;
    const color = this.getColorForZoneType(type, condition);
    
    // Đặt zone ở giữa canvas
    const centerX = ((this.warehouse?.mapWidth || 1000) / 2) - this.toPx(5);
    const centerY = ((this.warehouse?.mapLength || 1000) / 2) - this.toPx(5);
    
    const newZone: any = {
      warehouseId: this.warehouseId,
      code: zoneCode,
      name: zoneName,
      type: type,
      storageCondition: condition,
      color: color,
      positionX: Math.max(0, centerX),
      positionY: Math.max(0, centerY),
      width: this.toPx(10),
      length: this.toPx(10),
      rotation: 0,
    };
    
    this.warehouseService.createZone(newZone).subscribe(created => {
      this.refreshMap();
      setTimeout(() => {
        const zone = this.zones.find(z => z.id === created.id);
        if (zone) {
          this.openZoneDrawer(zone);
        }
      }, 100);
    });
  }

  // ============================================================
  // LOGIC HÍT NAM CHÂM VÀ VA CHẠM (PIXELS)
  // ============================================================
  private rectsOverlap(a: any, b: any): boolean {
    if (a.positionX + a.width  <= b.positionX) return false; 
    if (b.positionX + b.width  <= a.positionX) return false; 
    if (a.positionY + a.length <= b.positionY) return false; 
    if (b.positionY + b.length <= a.positionY) return false; 
    return true; 
  }

  private applyMagneticSnap(movingItem: any, targetItems: any[]) {
    for (const target of targetItems) {
      if (target.id === movingItem.id) continue;

      if (Math.abs((movingItem.positionX + movingItem.width) - target.positionX) < this.SNAP_THRESHOLD) {
        movingItem.positionX = target.positionX - movingItem.width;
      }
      else if (Math.abs(movingItem.positionX - (target.positionX + target.width)) < this.SNAP_THRESHOLD) {
        movingItem.positionX = target.positionX + target.width;
      }
      else if (Math.abs(movingItem.positionX - target.positionX) < this.SNAP_THRESHOLD) {
        movingItem.positionX = target.positionX;
      }

      if (Math.abs((movingItem.positionY + movingItem.length) - target.positionY) < this.SNAP_THRESHOLD) {
        movingItem.positionY = target.positionY - movingItem.length;
      }
      else if (Math.abs(movingItem.positionY - (target.positionY + target.length)) < this.SNAP_THRESHOLD) {
        movingItem.positionY = target.positionY + target.length;
      }
      else if (Math.abs(movingItem.positionY - target.positionY) < this.SNAP_THRESHOLD) {
        movingItem.positionY = target.positionY;
      }
    }
  }

  checkAllCollisions() {
    this.zones.forEach(z => (z as any)['_hasCollision'] = false);
    for (let i = 0; i < this.zones.length; i++) {
      for (let j = i + 1; j < this.zones.length; j++) {
        if (this.rectsOverlap(this.zones[i], this.zones[j])) {
          (this.zones[i] as any)['_hasCollision'] = true;
          (this.zones[j] as any)['_hasCollision'] = true;
        }
      }
    }

    this.bins.forEach(b => (b as any)['_hasCollision'] = false);
    this.zones.forEach(zone => {
      const binsInZone = this.getBinsOfZone(zone.id);
      for (let i = 0; i < binsInZone.length; i++) {
        for (let j = i + 1; j < binsInZone.length; j++) {
          if (this.rectsOverlap(binsInZone[i], binsInZone[j])) {
            (binsInZone[i] as any)['_hasCollision'] = true;
            (binsInZone[j] as any)['_hasCollision'] = true;
          }
        }
      }
    });
  }

  // ============================================================
  // ZOOM & PAN CONTROLS
  // ============================================================
  onWheel(event: WheelEvent) {
    // Zoom with Ctrl + Wheel
    if (event.ctrlKey || event.metaKey) {
      event.preventDefault();
      const delta = event.deltaY > 0 ? -0.1 : 0.1;
      const newScale = Math.max(0.2, Math.min(2.0, this.scale + delta));
      this.scale = newScale;
    }
  }

  onCanvasMouseDown(event: MouseEvent) {
    // Pan with right-click
    if (event.button === 2) {
      event.preventDefault();
      this.isPanning = true;
      this.panStartMouse = { x: event.clientX, y: event.clientY };
      if (this.canvasElement) {
        this.panStartScroll = {
          x: this.canvasElement.scrollLeft,
          y: this.canvasElement.scrollTop
        };
      }
      document.addEventListener('mousemove', this.onCanvasPan);
      document.addEventListener('mouseup', this.onCanvasPanEnd);
      
      // Change cursor
      if (this.canvasElement) {
        this.canvasElement.style.cursor = 'grabbing';
      }
    }
  }

  onCanvasPan = (event: MouseEvent) => {
    if (!this.isPanning || !this.canvasElement) return;
    
    const deltaX = event.clientX - this.panStartMouse.x;
    const deltaY = event.clientY - this.panStartMouse.y;
    
    this.canvasElement.scrollLeft = this.panStartScroll.x - deltaX;
    this.canvasElement.scrollTop = this.panStartScroll.y - deltaY;
  };

  onCanvasPanEnd = () => {
    this.isPanning = false;
    document.removeEventListener('mousemove', this.onCanvasPan);
    document.removeEventListener('mouseup', this.onCanvasPanEnd);
    
    if (this.canvasElement) {
      this.canvasElement.style.cursor = '';
    }
  };

  onZoneSingleClick(zone: ZoneDto, event: MouseEvent) {
    if (this.isResizing) return;
    event.stopPropagation();
    this.activeZone = zone;
    this.activeTab  = 'bins';
  }

  onZoneDoubleClick(zone: ZoneDto, event: MouseEvent) {
    event.stopPropagation();
    this.openZoneDrawer(zone);
  }

  onBinDoubleClick(bin: BinDto, event: MouseEvent) {
    event.stopPropagation();
    this.openBinDrawer(bin);
  }

  onCanvasClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.zone-block') && !target.closest('.bin-block')) {
      this.activeZone = null;
      this.closeDrawer();
    }
  }

  // ============================================================
  // DRAG & DROP
  // ============================================================
  onZoneDragEnded(event: CdkDragEnd, zone: ZoneDto) {
    const raw  = event.source.getFreeDragPosition();
    const mapW = (this.warehouse?.mapWidth  || 1000) * this.scale;
    const mapH = (this.warehouse?.mapLength || 1000) * this.scale;

    const clampedX = Math.max(0, Math.min(raw.x, mapW - zone.width  * this.scale));
    const clampedY = Math.max(0, Math.min(raw.y, mapH - zone.length * this.scale));

    const oldX = zone.positionX;
    const oldY = zone.positionY;
    let newX = Math.round(clampedX / this.scale);
    let newY = Math.round(clampedY / this.scale);

    zone.positionX = newX;
    zone.positionY = newY;

    this.applyMagneticSnap(zone, this.zones);

    const deltaX = zone.positionX - oldX;
    const deltaY = zone.positionY - oldY;

    const collision = this.zones.filter(z => z.id !== zone.id).some(z => this.rectsOverlap(zone, z));
    
    if (collision) {
      zone.positionX = oldX;
      zone.positionY = oldY;
      event.source.setFreeDragPosition({ x: oldX * this.scale, y: oldY * this.scale });
    } else {
      event.source.setFreeDragPosition({ x: zone.positionX * this.scale, y: zone.positionY * this.scale });
      (zone as any)['_isDirty'] = true; 
      
      this.getBinsOfZone(zone.id).forEach(bin => {
        bin.positionX += deltaX;
        bin.positionY += deltaY;
        (bin as any)['_isDirty'] = true; 
      });
      this.hasUnsavedChanges = true;
    }
    this.checkAllCollisions();
  }

  getBinsOfZone(zoneId: string): BinDto[] {
    return this.bins.filter(b => b.zoneId === zoneId);
  }

  getBinAbsX(bin: BinDto): number { return bin.positionX; }
  getBinAbsY(bin: BinDto): number { return bin.positionY; }

  onBinDragEnded(event: CdkDragEnd, bin: BinDto, zone: ZoneDto) {
    const raw = event.source.getFreeDragPosition();

    let logicX = Math.round(raw.x / this.scale);
    let logicY = Math.round(raw.y / this.scale);

    const oldX = bin.positionX;
    const oldY = bin.positionY;

    logicX = Math.max(zone.positionX, Math.min(logicX, zone.positionX + zone.width - bin.width));
    logicY = Math.max(zone.positionY, Math.min(logicY, zone.positionY + zone.length - bin.length));

    bin.positionX = logicX;
    bin.positionY = logicY;

    const binsInZone = this.getBinsOfZone(zone.id);
    this.applyMagneticSnap(bin, binsInZone);

    bin.positionX = Math.max(zone.positionX, Math.min(bin.positionX, zone.positionX + zone.width - bin.width));
    bin.positionY = Math.max(zone.positionY, Math.min(bin.positionY, zone.positionY + zone.length - bin.length));

    const hasCollision = binsInZone.filter(b => b.id !== bin.id).some(b => this.rectsOverlap(bin, b));

    if (hasCollision) {
      bin.positionX = oldX;
      bin.positionY = oldY;
      event.source.setFreeDragPosition({ x: oldX * this.scale, y: oldY * this.scale });
    } else {
      event.source.setFreeDragPosition({ x: bin.positionX * this.scale, y: bin.positionY * this.scale });
      (bin as any)['_isDirty'] = true; 
      this.hasUnsavedChanges = true;
    }

    this.checkAllCollisions();
  }

  async saveAllLayouts() {
    if (!this.hasUnsavedChanges) return;
    this.isSaving = true;

    try {
      const dirtyZones = this.zones.filter(z => (z as any)['_isDirty']);
      const dirtyBins = this.bins.filter(b => (b as any)['_isDirty']);

      for (const zone of dirtyZones) {
        await lastValueFrom(this.warehouseService.updateZone(zone.id, zone as any));
      }
      for (const bin of dirtyBins) {
        await lastValueFrom(this.warehouseService.updateStorageBin(bin.id, bin as any));
      }

      this.hasUnsavedChanges = false;
      this.refreshMap();
    } catch (error) {
      console.error('Save Layout Error:', error);
    } finally {
      this.isSaving = false;
    }
  }

  // ============================================================
  // DRAWER & FORM CRUD (DÙNG MÉT - M)
  // ============================================================
  openZoneDrawer(zone?: ZoneDto) {
    this.selectedZone = zone || null;
    this.selectedBin  = null;
    this.drawerType   = 'ZONE';

    // Khởi tạo form bằng giá trị đã đổi sang MÉT
    this.form = this.fb.group({
      warehouseId:      [this.warehouseId],
      code:             [zone?.code || '',  [Validators.required, Validators.maxLength(50)]],
      name:             [zone?.name || '',  [Validators.required, Validators.maxLength(255)]],
      type:             [zone?.type ?? ZoneType.Storage, [Validators.required]],
      storageCondition: [zone?.storageCondition ?? StorageCondition.Other, [Validators.required]],
      color:            [zone?.color || this.getRandomColor()],
      positionX:        [this.toM(zone?.positionX || 0), [Validators.required, Validators.min(0)]],
      positionY:        [this.toM(zone?.positionY || 0), [Validators.required, Validators.min(0)]],
      width:            [this.toM(zone?.width  || this.toPx(10)), [Validators.required, Validators.min(0.5)]], // Tối thiểu 0.5m
      length:           [this.toM(zone?.length || this.toPx(10)), [Validators.required, Validators.min(0.5)]],
      rotation:         [zone?.rotation || 0, [Validators.min(0), Validators.max(360)]],
    });

    this.form.get('type').valueChanges.subscribe(type => {
      if (type === ZoneType.Storage) {
        if (this.form.get('storageCondition').value === StorageCondition.Other)
          this.form.get('storageCondition').setValue(StorageCondition.Normal);
      } else {
        this.form.get('storageCondition').setValue(StorageCondition.Other);
      }
    });

    this.form.get('rotation').valueChanges.subscribe(val => {
      if (this.selectedZone) this.selectedZone.rotation = parseFloat(val) || 0;
    });

    // Preview trực tiếp vị trí khi gõ số mét trên form
    this.form.valueChanges.subscribe(val => {
      if (this.selectedZone && !this.isSaving) {
        this.selectedZone.positionX = this.toPx(val.positionX);
        this.selectedZone.positionY = this.toPx(val.positionY);
        this.selectedZone.width = this.toPx(val.width);
        this.selectedZone.length = this.toPx(val.length);
        this.checkAllCollisions();
      }
    });
  }

  saveZone() {
    if (this.form.invalid) return;
    
    // Đổi từ Mét trả về Pixel để gửi xuống Backend
    const payload = { 
      ...this.form.value, 
      positionX: this.toPx(this.form.value.positionX),
      positionY: this.toPx(this.form.value.positionY),
      width:     this.toPx(this.form.value.width),
      length:    this.toPx(this.form.value.length),
      rotation:  parseFloat(this.form.value.rotation) || 0 
    };

    const request = this.selectedZone?.id
      ? this.warehouseService.updateZone(this.selectedZone.id, payload)
      : this.warehouseService.createZone(payload);
      
    request.subscribe(() => { this.closeDrawer(); this.refreshMap(); });
  }

  deleteZone(zone: ZoneDto, event?: MouseEvent) {
    event?.stopPropagation();
    this.confirmation.warn('::ZoneDeletionWarningMessage', '::AreYouSure', {
      messageLocalizationParams: [zone.name],
    }).subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.warehouseService.deleteZone(zone.id).subscribe(() => {
          if (this.activeZone?.id === zone.id) this.activeZone = null;
          this.refreshMap();
        });
      }
    });
  }

  openBinDrawer(bin?: BinDto) {
    const defaultZoneId = this.activeZone?.id || (this.zones[0]?.id ?? null);
    const targetZone = this.zones.find(z => z.id === (bin?.zoneId || defaultZoneId));
    
    let defaultX = 0, defaultY = 0;
    if (!bin && targetZone) {
      defaultX = targetZone.positionX + (targetZone.width - this.toPx(2)) / 2;
      defaultY = targetZone.positionY + (targetZone.length - this.toPx(2)) / 2;
    }
    
    this.selectedBin  = bin || null;
    this.selectedZone = null;
    this.drawerType   = 'BIN';

    const isEditing = !!bin;
    const originalZoneId = bin?.zoneId;

    // Khởi tạo form bằng MÉT
    this.form = this.fb.group({
      warehouseId: [this.warehouseId],
      zoneId:      [bin?.zoneId || defaultZoneId, [Validators.required]],
      code:        [bin?.code || '', [Validators.required, Validators.maxLength(50)]],
      positionX:   [this.toM(bin?.positionX ?? defaultX), [Validators.required]],
      positionY:   [this.toM(bin?.positionY ?? defaultY), [Validators.required]],
      width:       [this.toM(bin?.width  || this.toPx(2)), [Validators.required, Validators.min(0.5)]], // 0.5m min
      length:      [this.toM(bin?.length || this.toPx(2)), [Validators.required, Validators.min(0.5)]],
      rotation:    [bin?.rotation || 0, [Validators.min(0), Validators.max(360)]],
      maxSKU:      [bin?.maxWeight || 0, [Validators.min(0)]],
      isBlocked:   [bin?.isBlocked || false],
    });

    this.form.get('zoneId').valueChanges.subscribe(newZoneId => {
      const newZone = this.zones.find(z => z.id === newZoneId);
      if (newZone) {
        if (isEditing && newZoneId !== originalZoneId) {
          const newX = newZone.positionX + (newZone.width - this.toPx(this.form.get('width').value)) / 2;
          const newY = newZone.positionY + (newZone.length - this.toPx(this.form.get('length').value)) / 2;
          this.form.patchValue({ positionX: this.toM(newX), positionY: this.toM(newY) }, { emitEvent: false });
        } else if (!isEditing) {
          const newX = newZone.positionX + (newZone.width - this.toPx(this.form.get('width').value)) / 2;
          const newY = newZone.positionY + (newZone.length - this.toPx(this.form.get('length').value)) / 2;
          this.form.patchValue({ positionX: this.toM(newX), positionY: this.toM(newY) }, { emitEvent: false });
        }
      }
    });

    this.form.get('rotation').valueChanges.subscribe(val => {
      if (this.selectedBin) this.selectedBin.rotation = parseFloat(val) || 0;
    });

    this.form.valueChanges.subscribe(val => {
      if (this.selectedBin && !this.isSaving) {
        this.selectedBin.positionX = this.toPx(val.positionX);
        this.selectedBin.positionY = this.toPx(val.positionY);
        this.selectedBin.width = this.toPx(val.width);
        this.selectedBin.length = this.toPx(val.length);
        this.checkAllCollisions();
      }
    });
  }

  createBinInActiveZone() {
    if (!this.activeZone) return;
    this.openBinDrawer();
  }

  saveBin() {
    if (this.form.invalid) return;
    
    // Chuyển lại từ Mét sang Pixel để gửi DB
    const payload = { 
      ...this.form.value, 
      positionX: this.toPx(this.form.value.positionX),
      positionY: this.toPx(this.form.value.positionY),
      width:     this.toPx(this.form.value.width),
      length:    this.toPx(this.form.value.length),
      rotation:  parseFloat(this.form.value.rotation) || 0,
    };

    const targetZone = this.zones.find(z => z.id === payload.zoneId);
    if (targetZone) {
      const maxX = targetZone.positionX + targetZone.width - payload.width;
      const maxY = targetZone.positionY + targetZone.length - payload.length;
      
      if (payload.positionX < targetZone.positionX || payload.positionY < targetZone.positionY || 
          payload.positionX > maxX || payload.positionY > maxY) {
        payload.positionX = targetZone.positionX + (targetZone.width - payload.width) / 2;
        payload.positionY = targetZone.positionY + (targetZone.length - payload.length) / 2;
      }
    }

    const request = this.selectedBin?.id
      ? this.warehouseService.updateStorageBin(this.selectedBin.id, payload)
      : this.warehouseService.createStorageBin(payload);
      
    request.subscribe(() => { this.closeDrawer(); this.refreshMap(); });
  }

  deleteBin(bin: BinDto, event?: MouseEvent) {
    event?.stopPropagation();
    this.confirmation.warn('::BinDeletionWarningMessage', '::AreYouSure', {
      messageLocalizationParams: [bin.code],
    }).subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.warehouseService.deleteStorageBin(bin.id).subscribe(() => this.refreshMap());
      }
    });
  }

  closeDrawer() {
    this.drawerType   = null;
    this.selectedZone = null;
    this.selectedBin  = null;
  }

  // ============================================================
  // ROTATE & RESIZE LOGIC (PIXELS)
  // ============================================================
  onRotateStart(event: MouseEvent, item: any) {
    event.stopPropagation();
    event.preventDefault();
    this.isRotating   = true;
    this.rotatingItem = { item };

    const inner = (event.target as HTMLElement).closest('.resizable-block') as HTMLElement;
    if (inner) {
      const rect = inner.getBoundingClientRect();
      this.rotateCenterCanvas = { x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 };
    }
    document.addEventListener('mousemove', this.onRotate);
    document.addEventListener('mouseup',   this.onRotateEnd);
  }

  onRotate = (event: MouseEvent) => {
    if (!this.isRotating || !this.rotatingItem) return;
    const { item } = this.rotatingItem;
    const dx = event.clientX - this.rotateCenterCanvas.x;
    const dy = event.clientY - this.rotateCenterCanvas.y;

    let angle = Math.round(Math.atan2(dy, dx) * (180 / Math.PI) + 90);
    if (angle < 0)   angle += 360;
    if (angle >= 360) angle -= 360;

    item.rotation = angle;
    item['_isDirty'] = true;
    this.hasUnsavedChanges = true;
  };

  onRotateEnd = () => {
    this.isRotating   = false;
    this.rotatingItem = null;
    document.removeEventListener('mousemove', this.onRotate);
    document.removeEventListener('mouseup',   this.onRotateEnd);
  };

  onResizeStart(event: MouseEvent, item: any, type: 'zone' | 'bin', handle: ResizeHandle) {
    event.stopPropagation();
    event.preventDefault();
    this.isResizing      = true;
    this.resizingItem    = { item, type, handle };
    this.resizeStartMouse = { x: event.clientX, y: event.clientY };
    this.resizeStartRect  = { x: item.positionX, y: item.positionY, w: item.width, h: item.length };
    document.addEventListener('mousemove', this.onResize);
    document.addEventListener('mouseup',   this.onResizeEnd);
  }

  onResize = (event: MouseEvent) => {
    if (!this.isResizing || !this.resizingItem) return;

    const { item, type, handle } = this.resizingItem;
    const dx = (event.clientX - this.resizeStartMouse.x) / this.scale;
    const dy = (event.clientY - this.resizeStartMouse.y) / this.scale;
    const { x: ox, y: oy, w: ow, h: oh } = this.resizeStartRect;
    
    // Tối thiểu là 1m (20px) cho Zone, 0.5m (10px) cho Bin
    const minSize  = type === 'zone' ? this.toPx(1) : this.toPx(0.5); 
    const mapW     = this.warehouse?.mapWidth  || 1000;
    const mapH     = this.warehouse?.mapLength || 1000;

    let newX = ox, newY = oy, newW = ow, newH = oh;

    if (handle === 'e' || handle === 'ne' || handle === 'se') newW = Math.max(minSize, Math.min(ow + dx, mapW - ox));
    if (handle === 'w' || handle === 'nw' || handle === 'sw') {
      const proposedX = Math.max(0, Math.min(ox + dx, ox + ow - minSize));
      newW = ow - (proposedX - ox);
      newX = proposedX;
    }
    if (handle === 's' || handle === 'sw' || handle === 'se') newH = Math.max(minSize, Math.min(oh + dy, mapH - oy));
    if (handle === 'n' || handle === 'nw' || handle === 'ne') {
      const proposedY = Math.max(0, Math.min(oy + dy, oy + oh - minSize));
      newH = oh - (proposedY - oy);
      newY = proposedY;
    }

    item.positionX = Math.round(newX); 
    item.positionY = Math.round(newY);
    item.width = Math.round(newW);     
    item.length = Math.round(newH);

    if (type === 'zone') {
      this.applyMagneticSnap(item, this.zones);

      if (this.zones.filter(z => z.id !== item.id).some(z => this.rectsOverlap(item, z))) {
        item.positionX = ox; item.positionY = oy; item.width = ow; item.length = oh; 
      } else {
        item['_isDirty'] = true;
        this.hasUnsavedChanges = true;
      }
      this.checkAllCollisions();
    } else {
      const targetZone = this.zones.find(z => z.id === item.zoneId);
      if (targetZone) {
        this.applyMagneticSnap(item, this.getBinsOfZone(targetZone.id));
      }
      
      const hasCollision = this.getBinsOfZone(item.zoneId).filter(b => b.id !== item.id).some(b => this.rectsOverlap(item, b));
      if (hasCollision) {
        item.positionX = ox; item.positionY = oy; item.width = ow; item.length = oh; 
      } else {
        item['_isDirty'] = true;
        this.hasUnsavedChanges = true;
      }
      this.checkAllCollisions();
    }
  };

  onResizeEnd = () => {
    this.isResizing   = false;
    this.resizingItem = null;
    document.removeEventListener('mousemove', this.onResize);
    document.removeEventListener('mouseup',   this.onResizeEnd);
    document.removeEventListener('mousemove', this.onRotate);
    document.removeEventListener('mouseup',   this.onRotateEnd);
  };

  getCursor(handle: ResizeHandle): string {
    const map: Record<ResizeHandle, string> = {
      n: 'n-resize', s: 's-resize', e: 'e-resize', w: 'w-resize',
      nw: 'nw-resize', ne: 'ne-resize', sw: 'sw-resize', se: 'se-resize',
    };
    return map[handle];
  }
}