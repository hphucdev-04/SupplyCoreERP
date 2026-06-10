import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { AgentService } from '../../../proxy/agent/agent.service';

interface DlpRule {
  Name: string;
  Pattern: string;
  Replacement: string;
}

@Component({
  selector: 'app-dlp-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dlp-settings.component.html',
  styleUrls: ['./dlp-settings.component.scss']
})
export class DlpSettingsComponent implements OnInit {
  dlpJson = '[]';
  originalJson = '[]';
  isValidJson = true;
  validationError: string | null = null;
  isLoading = false;
  isSaving = false;

  constructor(
    private agentService: AgentService,
    private toaster: ToasterService
  ) {}

  ngOnInit(): void {
    this.loadDlpRules();
  }

  loadDlpRules(): void {
    this.isLoading = true;
    this.agentService.getDlpRulesJson().subscribe({
      next: (data) => {
        try {
          const parsed = JSON.parse(data);
          this.dlpJson = JSON.stringify(parsed, null, 2);
          this.originalJson = this.dlpJson;
          this.isValidJson = true;
          this.validationError = null;
        } catch (e) {
          this.dlpJson = data;
          this.isValidJson = false;
          this.validationError = 'Dữ liệu cấu hình hiện tại không đúng định dạng JSON.';
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load DLP rules', err);
        this.toaster.error('Không thể tải cấu hình DLP từ Server.', 'Lỗi tải cấu hình');
        this.isLoading = false;
      }
    });
  }

  onJsonChange(value: string): void {
    this.dlpJson = value;
    if (!value.trim()) {
      this.isValidJson = true;
      this.validationError = null;
      return;
    }

    try {
      const parsed = JSON.parse(value);
      if (!Array.isArray(parsed)) {
        this.isValidJson = false;
        this.validationError = 'Cấu trúc DLP Rules bắt buộc phải là một Mảng (Array).';
        return;
      }

      for (let i = 0; i < parsed.length; i++) {
        const item = parsed[i];
        if (!item.Name || !item.Pattern || !item.Replacement) {
          this.isValidJson = false;
          this.validationError = `Rule thứ ${i + 1} thiếu thuộc tính bắt buộc (phải có: Name, Pattern, Replacement).`;
          return;
        }
      }

      this.isValidJson = true;
      this.validationError = null;
    } catch (e: any) {
      this.isValidJson = false;
      this.validationError = `Lỗi cú pháp JSON: ${e.message}`;
    }
  }

  saveDlpRules(): void {
    if (!this.isValidJson || this.isSaving) return;

    this.isSaving = true;
    let jsonToSave = this.dlpJson;
    try {
      jsonToSave = JSON.stringify(JSON.parse(this.dlpJson));
    } catch (e) {}

    this.agentService.updateDlpRulesJson(jsonToSave).subscribe({
      next: () => {
        this.originalJson = this.dlpJson;
        this.isSaving = false;
        this.toaster.success('Cập nhật cấu hình DLP Rules thành công!', 'Thành công');
      },
      error: (err) => {
        console.error('Failed to save DLP rules', err);
        this.toaster.error('Có lỗi xảy ra khi lưu cấu hình DLP. Vui lòng kiểm tra lại.', 'Lỗi lưu dữ liệu');
        this.isSaving = false;
      }
    });
  }

  resetChanges(): void {
    this.dlpJson = this.originalJson;
    this.onJsonChange(this.dlpJson);
  }
}
