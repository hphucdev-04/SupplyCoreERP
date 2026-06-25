import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import {
  DocumentPrintColumn,
  DocumentPrintField,
  DocumentPrintModel,
  DocumentPrintRow,
  DocumentPrintSection,
  DocumentPrintSignature,
  DocumentPrintSummaryItem,
} from '../../models/document-print.model';

export const DOCUMENT_PRINT_LAYOUT_STYLES = `
  @page { size: A4 portrait; margin: 12mm; }
  * { box-sizing: border-box; }
  body {
    margin: 0;
    font-family: Arial, sans-serif;
    color: #1f2937;
    background: #ffffff;
    font-size: 12px;
    line-height: 1.45;
  }
  .print-page {
    width: 100%;
  }
  .document-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 16px;
    border-bottom: 2px solid #1d4ed8;
    padding-bottom: 12px;
    margin-bottom: 16px;
  }
  .company-name {
    font-size: 13px;
    font-weight: 700;
    letter-spacing: 0;
    text-transform: uppercase;
    color: #1d4ed8;
    margin-bottom: 4px;
  }
  .document-title {
    font-size: 22px;
    font-weight: 700;
    text-transform: uppercase;
    color: #111827;
  }
  .document-number,
  .document-printed-at {
    margin-top: 4px;
    color: #4b5563;
  }
  .document-section {
    margin-bottom: 16px;
  }
  .section-title {
    margin-bottom: 8px;
    font-size: 13px;
    font-weight: 700;
    color: #111827;
  }
  .field-grid {
    display: grid;
    gap: 10px 16px;
  }
  .field-grid.columns-1 { grid-template-columns: 1fr; }
  .field-grid.columns-2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .field-grid.columns-3 { grid-template-columns: repeat(3, minmax(0, 1fr)); }
  .field-item {
    min-width: 0;
  }
  .field-label {
    font-size: 11px;
    color: #6b7280;
    margin-bottom: 2px;
  }
  .field-value {
    font-weight: 600;
    color: #111827;
    word-break: break-word;
  }
  .document-table {
    width: 100%;
    border-collapse: collapse;
  }
  .document-table th,
  .document-table td {
    border: 1px solid #d1d5db;
    padding: 8px 10px;
    vertical-align: top;
    white-space: pre-wrap;
  }
  .document-table th {
    background: #eff6ff;
    color: #1e3a8a;
    font-weight: 700;
  }
  .empty-cell {
    text-align: center;
    color: #6b7280;
    padding: 18px 10px;
  }
  .document-summary {
    margin-left: auto;
    width: min(320px, 100%);
    border-top: 1px solid #d1d5db;
    padding-top: 8px;
  }
  .summary-row {
    display: flex;
    justify-content: space-between;
    gap: 12px;
    padding: 4px 0;
  }
  .summary-label {
    color: #4b5563;
  }
  .summary-value {
    font-weight: 700;
    color: #111827;
    text-align: right;
  }
  .document-note {
    margin-top: 12px;
  }
  .note-content {
    white-space: pre-wrap;
  }
  .signature-grid {
    display: grid;
    gap: 24px;
    margin-top: 32px;
  }
  .signature-count-2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .signature-count-3 { grid-template-columns: repeat(3, minmax(0, 1fr)); }
  .signature-count-4 { grid-template-columns: repeat(4, minmax(0, 1fr)); }
  .signature-item {
    text-align: center;
  }
  .signature-label {
    font-weight: 700;
    color: #111827;
  }
  .signature-space {
    height: 72px;
  }
  .signature-name {
    font-weight: 600;
    color: #374151;
  }
`;

export function renderDocumentPrintLayout(model: DocumentPrintModel): string {
  return `
    <div class="print-page">
      <header class="document-header">
        <div>
          <div class="company-name">RxLogistics</div>
          <div class="document-title">${escapePrintHtml(model.title)}</div>
          ${model.documentNumber ? `<div class="document-number">${escapePrintHtml(model.documentNumber)}</div>` : ''}
        </div>
        ${model.printedAt ? `<div class="document-printed-at">In lúc: ${escapePrintHtml(model.printedAt)}</div>` : ''}
      </header>

      ${renderSections(model.sections ?? [])}
      ${renderTable(model.columns ?? [], model.rows ?? [])}
      ${renderSummary(model.summary ?? [])}
      ${renderNote(model.note)}
      ${renderSignatures(model.signatures ?? [])}
    </div>
  `;
}

@Component({
  selector: 'app-document-print-layout',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './document-print-layout.component.html',
  styleUrls: ['./document-print-layout.component.scss'],
})
export class DocumentPrintLayoutComponent {
  @Input({ required: true }) model!: DocumentPrintModel;

  getColumnClass(section: DocumentPrintSection): string {
    const columns = section.columns && section.columns > 0 ? section.columns : 2;
    return `columns-${columns}`;
  }

  getCellValue(row: DocumentPrintRow, column: DocumentPrintColumn): string {
    return formatPrintCellValue(row[column.key]);
  }

  getSignatureCountClass(signatures: DocumentPrintSignature[] | undefined): string {
    return `signature-count-${signatures?.length ?? 0}`;
  }
}

function renderSections(sections: DocumentPrintSection[]): string {
  if (!sections.length) {
    return '';
  }

  return sections
    .map(section => {
      const columns = section.columns && section.columns > 0 ? section.columns : 2;
      return `
        <section class="document-section">
          ${section.title ? `<div class="section-title">${escapePrintHtml(section.title)}</div>` : ''}
          <div class="field-grid columns-${columns}">
            ${section.fields.map(field => renderField(field)).join('')}
          </div>
        </section>
      `;
    })
    .join('');
}

function renderField(field: DocumentPrintField): string {
  return `
    <div class="field-item">
      <div class="field-label">${escapePrintHtml(field.label)}</div>
      <div class="field-value">${escapePrintHtml(field.value ?? '')}</div>
    </div>
  `;
}

function renderTable(columns: DocumentPrintColumn[], rows: DocumentPrintRow[]): string {
  if (!columns.length) {
    return '';
  }

  return `
    <section class="document-section">
      <table class="document-table">
        <thead>
          <tr>
            ${columns
              .map(
                column =>
                  `<th style="${column.width ? `width:${column.width};` : ''} text-align:${column.align ?? 'left'}">${escapePrintHtml(column.header)}</th>`,
              )
              .join('')}
          </tr>
        </thead>
        <tbody>
          ${
            rows.length
              ? rows
                  .map(
                    row => `
              <tr>
                ${columns
                  .map(
                    column =>
                      `<td style="text-align:${column.align ?? 'left'}">${escapePrintHtml(formatPrintCellValue(row[column.key]))}</td>`,
                  )
                  .join('')}
              </tr>
            `,
                  )
                  .join('')
              : `<tr><td colspan="${columns.length}" class="empty-cell">Không có dữ liệu</td></tr>`
          }
        </tbody>
      </table>
    </section>
  `;
}

function renderSummary(summary: DocumentPrintSummaryItem[]): string {
  if (!summary.length) {
    return '';
  }

  return `
    <section class="document-summary">
      ${summary
        .map(
          item => `
            <div class="summary-row">
              <span class="summary-label">${escapePrintHtml(item.label)}</span>
              <span class="summary-value">${escapePrintHtml(item.value ?? '')}</span>
            </div>
          `,
        )
        .join('')}
    </section>
  `;
}

function renderNote(note?: string | null): string {
  if (!note) {
    return '';
  }

  return `
    <section class="document-note">
      <div class="section-title">Ghi chú</div>
      <div class="note-content">${escapePrintHtml(note)}</div>
    </section>
  `;
}

function renderSignatures(signatures: DocumentPrintSignature[]): string {
  if (!signatures.length) {
    return '';
  }

  return `
    <section class="signature-grid signature-count-${signatures.length}">
      ${signatures
        .map(
          signature => `
            <div class="signature-item">
              <div class="signature-label">${escapePrintHtml(signature.label)}</div>
              <div class="signature-space"></div>
              <div class="signature-name">${escapePrintHtml(signature.name ?? '')}</div>
            </div>
          `,
        )
        .join('')}
    </section>
  `;
}

function formatPrintCellValue(value: string | number | null | undefined): string {
  if (value === null || value === undefined) {
    return '';
  }

  return String(value);
}

function escapePrintHtml(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}
