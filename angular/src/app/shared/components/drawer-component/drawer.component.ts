import { CoreModule } from '@abp/ng.core';
import { Component, EventEmitter, Input, Output, ContentChild, TemplateRef, AfterContentInit } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-drawer',
  imports: [CoreModule],
  templateUrl: './drawer.component.html',
  styleUrls: ['./drawer.component.scss']
})
export class DrawerComponent implements AfterContentInit {
  @Input() isOpen = false;
  @Input() title = '';
  @Input() width: 'sm' | 'md' | 'lg' | 'xl' | 'auto' = 'md';
  @Input() height: 'sm' | 'md' | 'lg' | 'auto' = 'md';
  @Input() showFooter = true;
  @Input() saveDisabled = false;
  @Input() saveButtonText = '::Save';
  @Input() cancelButtonText = '::Cancel';
  @Input() position: 'right' | 'bottom' = 'right';

  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<void>();

  @ContentChild('drawerFooter') customFooter: TemplateRef<any>;
  hasCustomFooter = false;

  get drawerWidth(): string {
    if (this.position === 'bottom') return '100%';
    const widthMap = {
      sm: '300px',
      md: '400px',
      lg: '600px',
      xl: '800px',
      auto: 'auto'
    };
    return widthMap[this.width];
  }

  get drawerHeight(): string {
    if (this.position === 'right') return '100%';
    const heightMap = {
      sm: '30vh',
      md: '50vh',
      lg: '80vh',
      auto: 'auto'
    };
    return heightMap[this.height];
  }

  ngAfterContentInit(): void {
    this.hasCustomFooter = !!this.customFooter;
  }

  onClose(): void {
    this.close.emit();
  }

  onSave(): void {
    this.save.emit();
  }

  onOverlayClick(): void {
    this.onClose();
  }
}
