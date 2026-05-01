import { Directive, HostBinding, HostListener, Input } from '@angular/core';

@Directive({
  selector: '[copyText]'
})
export class CopyDirective {
  @Input('copyText') textToCopy = '';

  @HostBinding('style.cursor')
  cursor = 'pointer';

  @HostBinding('attr.title')
  title = 'Double click to copy';

  private defaultTitle = 'Double click to copy';

  @HostListener('dblclick')
  async copy() {
    if (!this.textToCopy) return;

    await navigator.clipboard.writeText(this.textToCopy);

    this.title = 'Copied ✓';

    setTimeout(() => {
      this.title = this.defaultTitle;
    }, 1500);
  }
}