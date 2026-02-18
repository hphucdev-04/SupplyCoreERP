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
  ) {}

  ngOnInit(): void {
    this.formatValue(this.ngControl.value);
  }

  //Khi người dùng gõ: strip dấu phẩy → parse số → format lại → set FormControl
  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;

    //Lấy raw value, xóa hết ký tự không phải số và dấu chấm
    const rawValue = input.value.replace(/,/g, '').replace(/[^\d.]/g, '');

    //Parse về number
    const numberValue = rawValue === '' ? null : Number(rawValue);

    if (numberValue !== null && isNaN(numberValue)) return;

    //Format display
    const formatted = numberValue !== null
      ? numberValue.toLocaleString(this.locale)
      : '';

    //Giữ vị trí cursor 
    const cursorPos = this.getCursorAdjustment(input.value, formatted, input.selectionStart ?? 0);
    input.value = formatted;
    input.setSelectionRange(cursorPos, cursorPos);

    //Cập nhật FormControl với giá trị số thuần
    this.ngControl.control?.setValue(numberValue, { emitEvent: true });
  }

  //Khi focus vào: chỉ hiện số thuần để dễ sửa 
  @HostListener('focus')
  onFocus(): void {
    const value = this.ngControl.value;
    if (value !== null && value !== undefined && value !== '') {
      this.el.nativeElement.value = String(value).replace(/,/g, '');
    }
  }

  //Khi blur ra: format lại với dấu phẩy
  @HostListener('blur')
  onBlur(): void {
    this.formatValue(this.ngControl.value);
  }

  private formatValue(value: number | null | undefined): void {
    if (value === null || value === undefined || value === 0 && this.el.nativeElement.value === '') {
      this.el.nativeElement.value = '';
      return;
    }
    if (value === 0) {
      this.el.nativeElement.value = '0';
      return;
    }
    this.el.nativeElement.value = Number(value).toLocaleString(this.locale);
  }

  private getCursorAdjustment(
    oldValue: string,
    newValue: string,
    oldCursor: number
  ): number {
    const oldCommasBefore = (oldValue.slice(0, oldCursor).match(/,/g) || []).length;
    const rawCursor = oldCursor - oldCommasBefore;

    let charCount = 0;
    for (let i = 0; i < newValue.length; i++) {
      if (newValue[i] !== ',') {
        charCount++;
      }
      if (charCount === rawCursor) {
        return i + 1;
      }
    }
    return newValue.length;
  }
}