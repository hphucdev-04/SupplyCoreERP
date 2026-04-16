import { Component, ElementRef, EventEmitter, HostListener, Input, Output } from '@angular/core';
import { SharedModule } from '../../shared.module';

export interface FilterOption {
  label: string;
  value: any;
  color?: string;
}

export interface FilterConfig {
  key: string;
  label: string;
  type: 'radio' | 'searchable-select' | 'checkbox';
  options: FilterOption[];
  value?: any;
  _searchTerm?: string;
}

@Component({
  selector: 'app-filter',
  standalone: true,
  imports: [SharedModule],
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.scss']
})
export class FilterComponent {
  @Input() config: FilterConfig[] = [];
  @Output() filterApplied = new EventEmitter<Record<string, any>>();

  isOpen = false;

  constructor(private el: ElementRef) { }

  @HostListener('document:click')
  onDocumentClick(): void {
    if (this.isOpen) {
      this.isOpen = false;
    }
  }

  toggleOpen(): void {
    this.isOpen = !this.isOpen;
  }

  close(): void {
    this.isOpen = false;
  }

  applyAndClose(): void {
    this.applyFilters();
    this.isOpen = false;
  }

  getActiveCount(): number {
    return this.config.filter(f => f.value !== null && f.value !== undefined && f.value !== '').length;
  }

  getSelectedLabel(field: FilterConfig): string {
    const opt = field.options.find(o => o.value === field.value);
    return opt ? opt.label : '';
  }

  getFilteredOptions(field: FilterConfig): FilterOption[] {
    const term = (field._searchTerm || '').toLowerCase().trim();
    return field.options.filter(opt => {
      if (opt.value === null) return false;
      if (!term) return true;
      return opt.label.toLowerCase().includes(term);
    });
  }

  selectOption(field: FilterConfig, value: any): void {
    field.value = value;
    field._searchTerm = '';
  }

  clearField(field: FilterConfig): void {
    field.value = null;
    field._searchTerm = '';
  }

  clearAll(): void {
    this.config.forEach(f => {
      f.value = null;
      f._searchTerm = '';
    });
    this.applyFilters();
  }

  applyFilters(): void {
    const result: Record<string, any> = {};
    this.config.forEach(f => {
      result[f.key] = f.value ?? null;
    });
    this.filterApplied.emit(result);
  }
}