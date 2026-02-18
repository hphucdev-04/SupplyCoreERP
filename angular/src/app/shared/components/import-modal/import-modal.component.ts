import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { SharedModule } from '../../shared.module';

@Component({
  selector: 'app-import-modal',
  templateUrl: 'import-modal.component.html',
  styleUrl: 'import-modal.component.scss',
  imports: [SharedModule]
})
export class ImportModalComponent {
  // --- INPUTS ---
  @Input() visible = false;
  @Input() title = '::Import'; // Tiêu đề Modal
  @Input() templateName = 'Template.xlsx'; // Tên file mẫu khi tải về

  // Hàm API Import: Nhận vào File -> Trả về Observable
  @Input() importFn: (file: File) => Observable<any>;
  
  // Hàm API Template: Không nhận tham số -> Trả về Observable<Blob>
  @Input() templateFn: () => Observable<any>;

  // --- OUTPUTS ---
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() success = new EventEmitter<void>();

  // --- INTERNAL STATE ---
  file: File | null = null;
  isImporting = false;
  dragOver = false;

  constructor(private toaster: ToasterService) {}

  // Đóng modal
  close() {
    this.visible = false;
    this.visibleChange.emit(false);
    this.reset();
  }

  // Reset trạng thái
  reset() {
    this.file = null;
    this.isImporting = false;
    this.dragOver = false;
  }

  // --- LOGIC FILE & DRAG DROP ---
  onFileSelected(event: any) {
    if (event.target.files.length > 0) {
      this.file = event.target.files[0];
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;
    if (event.dataTransfer?.files.length > 0) {
      this.file = event.dataTransfer.files[0];
    }
  }

  // --- ACTIONS ---
  
  downloadTemplate() {
    if (!this.templateFn) return;
    
    this.templateFn().subscribe((blob: Blob) => {
      this.downloadBlob(blob, this.templateName);
    });
  }

  upload() {
    if (!this.file || !this.importFn) return;

    this.isImporting = true;
    
    this.importFn(this.file)
      .pipe(finalize(() => this.isImporting = false))
      .subscribe({
        next: () => {
          this.toaster.success('::ImportSuccess', '::Success');
          this.success.emit(); // Báo cho cha biết đã xong để reload data
          this.close();
        },
        error: (err) => {
          // Lỗi đã được ABP Handle, nhưng ta cần tắt loading
          console.error(err);
        }
      });
  }

  private downloadBlob(blob: Blob, fileName: string) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }
}