import { Directive, ElementRef, HostListener } from '@angular/core';

@Directive({
  selector: '[enterSubmit]',
  standalone: true
})
export class EnterSubmitDirective {
  constructor(private el: ElementRef<HTMLElement>) {}

  @HostListener('document:keydown', ['$event'])
  onKeydown(event: KeyboardEvent) {
    if (event.key !== 'Enter') return;

    const target = event.target as HTMLElement;

    // chỉ xử lý nếu focus đang ở trong drawer này
    if (!this.el.nativeElement.contains(target)) return;

    // giữ Enter mặc định cho textarea
    if (
      target.tagName === 'TEXTAREA' ||
      target.tagName === 'BUTTON' ||
      target.isContentEditable
    ) {
      return;
    }

    const saveButton =
      this.el.nativeElement.querySelector<HTMLButtonElement>('.ph-btn-save');

    if (!saveButton || saveButton.disabled) return;

    event.preventDefault();
    saveButton.click();
  }
}