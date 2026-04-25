import {
  Component, ElementRef,
  EventEmitter, HostListener, Input, Output,
} from '@angular/core';
import { SharedModule } from '../../shared.module';
import { DropdownSearchComponent } from '../dropdownsearch-component/dropdown-search.component';

export interface FilterSlot {
  key: string;
  label: string;
  type: 'searchable-select' | 'radio';
  items: any[];
  labelKey?: string;
  valueKey?: string;
  colors?: Record<any, string>;
  value?: any;
}

@Component({
  selector: 'app-filter',
  standalone: true,
  imports: [SharedModule, DropdownSearchComponent],
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.scss'],
})
export class FilterComponent {

  // ── Mỗi filter field là 1 Input riêng ──────────────────────
  @Input() set slot1(s: FilterSlot) { this._set(0, s); }
  @Input() set slot2(s: FilterSlot) { this._set(1, s); }
  @Input() set slot3(s: FilterSlot) { this._set(2, s); }
  @Input() set slot4(s: FilterSlot) { this._set(3, s); }
  @Input() set slot5(s: FilterSlot) { this._set(4, s); }

  @Output() applied = new EventEmitter<Record<string, any>>();

  isOpen = false;
  slots: FilterSlot[] = [];

  private _set(idx: number, slot: FilterSlot): void {
    if (!slot) return;
    const existing = this.slots[idx];
    // Giữ nguyên value khi items reload
    this.slots[idx] = { ...slot, value: existing?.value ?? slot.value ?? null };
  }

  get activeSlots(): FilterSlot[] {
    return this.slots.filter(Boolean);
  }

  constructor(private el: ElementRef) { }

  @HostListener('document:click')
  onDocumentClick(): void { if (this.isOpen) this.isOpen = false; }

  toggleOpen(): void { this.isOpen = !this.isOpen; }
  close(): void { this.isOpen = false; }

  applyAndClose(): void {
    const result: Record<string, any> = {};
    this.activeSlots.forEach(s => (result[s.key] = s.value ?? null));
    this.applied.emit(result);
    this.isOpen = false;
  }

  getActiveCount(): number {
    return this.activeSlots.filter(
      s => s.value !== null && s.value !== undefined && s.value !== ''
    ).length;
  }

  clearAll(): void {
    this.activeSlots.forEach(s => (s.value = null));
    const result: Record<string, any> = {};
    this.activeSlots.forEach(s => (result[s.key] = null));
    this.applied.emit(result);
  }

  toDropdownItems(slot: FilterSlot) {
    const lk = slot.labelKey ?? 'name';
    const vk = slot.valueKey ?? 'id';
    return (slot.items ?? []).map(i => ({ id: i[vk], name: i[lk] }));
  }

  getColor(slot: FilterSlot, value: any): string | undefined {
    return slot.colors?.[value];
  }
}