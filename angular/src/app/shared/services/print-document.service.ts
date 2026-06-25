import { Injectable } from '@angular/core';
import { DocumentPrintModel } from '../models/document-print.model';
import {
  DOCUMENT_PRINT_LAYOUT_STYLES,
  renderDocumentPrintLayout,
} from '../components/document-print-layout/document-print-layout.component';

@Injectable({
  providedIn: 'root',
})
export class PrintDocumentService {
  print(model: DocumentPrintModel) {
    const printWindow = window.open('', '_blank', 'width=1024,height=768');
    if (!printWindow) {
      return;
    }

    printWindow.document.write(this.buildHtml(model));
    printWindow.document.close();

    setTimeout(() => {
      printWindow.focus();
      printWindow.print();
      printWindow.close();
    }, 300);
  }

  private buildHtml(model: DocumentPrintModel): string {
    return `<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>${this.escapeHtml(model.title)}</title>
  <style>
    ${DOCUMENT_PRINT_LAYOUT_STYLES}
  </style>
</head>
<body>
  ${renderDocumentPrintLayout(model)}
</body>
</html>`;
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }
}
