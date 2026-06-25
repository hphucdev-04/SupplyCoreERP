import { Directive, ElementRef, HostListener, Input, OnInit } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
  selector: '[appCurrencyFormat]',
  standalone: true,
})
export class CurrencyFormatDirective implements OnInit {

  @Input() locale: string = 'en-US';
  @Input() suffix: string = '';

  constructor(
    private el: ElementRef<HTMLInputElement>,
    private ngControl: NgControl
  ) { }

  ngOnInit(): void {
    this.formatValue(this.ngControl.value);
  }

  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const selectionStart = input.selectionStart ?? 0;

    // Strip tất cả ký tự không phải số (kể cả dấu phẩy đang có)
    const rawValue = input.value.replace(/[^\d]/g, '');
    const numberValue = rawValue === '' ? null : Number(rawValue);

    if (numberValue !== null && isNaN(numberValue)) return;

    const formatted = numberValue !== null
      ? numberValue.toLocaleString(this.locale)
      : '';

    // Tính cursor dựa trên số digit trước cursor trong raw string
    const digitsBeforeCursor = input.value
      .slice(0, selectionStart)
      .replace(/[^\d]/g, '').length;

    input.value = formatted;

    // Đặt lại cursor sau khi format
    let digitCount = 0;
    let newCursor = formatted.length;
    for (let i = 0; i < formatted.length; i++) {
      if (/\d/.test(formatted[i])) digitCount++;
      if (digitCount === digitsBeforeCursor) {
        newCursor = i + 1;
        break;
      }
    }
    input.setSelectionRange(newCursor, newCursor);

    this.ngControl.control?.setValue(numberValue, { emitEvent: true });
  }

  // Focus không cần strip nữa vì onInput đã handle real-time
  @HostListener('focus')
  onFocus(): void {
    // Reformat để đảm bảo hiển thị đúng khi focus vào
    this.formatValue(this.ngControl.value);
  }

  @HostListener('blur')
  onBlur(): void {
    this.formatValue(this.ngControl.value);
  }

  private formatValue(value: number | null | undefined): void {
    if (value === null || value === undefined || (value === 0 && this.el.nativeElement.value === '')) {
      this.el.nativeElement.value = '';
      return;
    }
    if (value === 0) {
      this.el.nativeElement.value = '0';
      return;
    }
    this.el.nativeElement.value = Number(value).toLocaleString(this.locale);
  }
}