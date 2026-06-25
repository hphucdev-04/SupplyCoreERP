export interface DocumentPrintField {
  label: string;
  value?: string | null;
}

export interface DocumentPrintSection {
  title?: string;
  fields: DocumentPrintField[];
  columns?: number;
}

export interface DocumentPrintColumn {
  key: string;
  header: string;
  align?: 'left' | 'center' | 'right';
  width?: string;
}

export interface DocumentPrintRow {
  [key: string]: string | number | null | undefined;
}

export interface DocumentPrintSummaryItem {
  label: string;
  value?: string | null;
}

export interface DocumentPrintSignature {
  label: string;
  name?: string | null;
}

export interface DocumentPrintModel {
  title: string;
  documentNumber?: string | null;
  printedAt?: string | null;
  sections?: DocumentPrintSection[];
  columns?: DocumentPrintColumn[];
  rows?: DocumentPrintRow[];
  summary?: DocumentPrintSummaryItem[];
  note?: string | null;
  signatures?: DocumentPrintSignature[];
}
