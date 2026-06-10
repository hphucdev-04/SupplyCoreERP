import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  OnChanges,
  SimpleChanges,
  forwardRef,
  HostListener,
  ElementRef,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface DropdownItem {
  id?: string | number | null;
  name?: string;
  [key: string]: any;
}

@Component({
  selector: 'app-dropdown-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dropdown-search.component.html',
  styleUrl: './dropdown-search.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DropdownSearchComponent),
      multi: true,
    },
  ],
})
export class DropdownSearchComponent
  implements ControlValueAccessor, OnInit, OnDestroy, OnChanges
{
  @Input() items: DropdownItem[] = [];
  @Input() placeholder = 'Select...';
  @Input() searchPlaceholder = 'Search...';
  @Input() labelKey = 'name';
  @Input() valueKey = 'id';
  @Input() disabled = false;
  @Input() clearable = true;
  /** Show null option like "-- All --" for filter mode */
  @Input() nullLabel: string | null = null;
  @Output() valueChange = new EventEmitter<any>();

  isOpen = false;
  searchText = '';
  selectedItem: DropdownItem | null = null;

  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private elRef: ElementRef, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges): void {
    // Re-resolve selected label when items load after value is set
    if (changes['items'] && this.selectedItem) {
      const found = this.items.find(
        (i) => i[this.valueKey] === this.selectedItem![this.valueKey]
      );
      if (found) {
        this.selectedItem = found;
        this.cdr.markForCheck();
      }
    }
  }

  ngOnDestroy(): void {}

  // ── ControlValueAccessor ──────────────────────────────────────────
  writeValue(value: any): void {
    if (value === null || value === undefined) {
      this.selectedItem = null;
    } else {
      this.selectedItem =
        this.items.find((i) => i[this.valueKey] === value) ??
        ({ [this.valueKey]: value, [this.labelKey]: value } as DropdownItem);
    }
    this.cdr.markForCheck();
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    this.cdr.markForCheck();
  }

  // ── Computed ──────────────────────────────────────────────────────
  get filteredItems(): DropdownItem[] {
    if (!this.searchText.trim()) return this.items;
    const q = this.searchText.toLowerCase();
    return this.items.filter((i) =>
      String(i[this.labelKey]).toLowerCase().includes(q)
    );
  }

  get displayLabel(): string {
    if (this.selectedItem === null) {
      return this.nullLabel ?? '';
    }
    return String(this.selectedItem[this.labelKey] ?? '');
  }

  get hasValue(): boolean {
    return this.selectedItem !== null;
  }

  // ── Interactions ──────────────────────────────────────────────────
  toggle(): void {
    if (this.disabled) return;
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.searchText = '';
    }
    this.onTouched();
    this.cdr.markForCheck();
  }

  select(item: DropdownItem | null): void {
    this.selectedItem = item;
    const val = item ? item[this.valueKey] : null;
    this.onChange(val);
    this.valueChange.emit(val);
    this.isOpen = false;
    this.searchText = '';
    this.cdr.markForCheck();
  }

  clear(event: Event): void {
    event.stopPropagation();
    this.select(null);
  }

  isSelected(item: DropdownItem): boolean {
    if (!this.selectedItem) return false;
    return item[this.valueKey] === this.selectedItem[this.valueKey];
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elRef.nativeElement.contains(event.target)) {
      if (this.isOpen) {
        this.isOpen = false;
        this.cdr.markForCheck();
      }
    }
  }
}