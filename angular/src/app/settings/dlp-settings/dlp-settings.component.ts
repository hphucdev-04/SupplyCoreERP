import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CoreModule } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { SettingService } from '../../proxy/settings/setting.service';

@Component({
  selector: 'app-dlp-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, CoreModule],
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
  isResetting = false;

  constructor(
    private settingService: SettingService,
    private toaster: ToasterService,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadDlpRules();
  }

  loadDlpRules(): void {
    this.isLoading = true;
    this.settingService.getDlpSettings().subscribe({
      next: (data) => {
        try {
          const parsed = data.rules || [];
          this.dlpJson = JSON.stringify(parsed, null, 2);
          this.originalJson = this.dlpJson;
          this.isValidJson = true;
          this.validationError = null;
        } catch (e: any) {
          this.isValidJson = false;
          this.validationError = '::Settings:DlpInvalidSyntax';
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load DLP rules', err);
        this.toaster.error('::Settings:DlpLoadError', '::Settings:DlpLoadErrorTitle');
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
        this.validationError = '::Settings:DlpInvalidFormat';
        return;
      }

      for (let i = 0; i < parsed.length; i++) {
        const item = parsed[i];
        if (!item.name || !item.pattern || !item.replacement) {
          this.isValidJson = false;
          this.validationError = '::Settings:DlpMissingFields';
          return;
        }
      }

      this.isValidJson = true;
      this.validationError = null;
    } catch (e: any) {
      this.isValidJson = false;
      this.validationError = '::Settings:DlpInvalidSyntax';
    }
  }

  saveDlpRules(): void {
    if (!this.isValidJson || this.isSaving) return;

    this.isSaving = true;
    let rules = [];
    try {
      rules = JSON.parse(this.dlpJson);
    } catch (e) {
      this.isSaving = false;
      return;
    }

    this.settingService.updateDlpSettings({ rules }).subscribe({
      next: () => {
        this.originalJson = this.dlpJson;
        this.isSaving = false;
        this.toaster.success('::Settings:DlpSaveSuccess', '::Settings:SuccessTitle');
      },
      error: (err) => {
        console.error('Failed to save DLP rules', err);
        this.toaster.error('::Settings:DlpSaveError', '::Settings:DlpSaveErrorTitle');
        this.isSaving = false;
      }
    });
  }

  resetDlpRules(): void {
    this.confirmation.warn('::Settings:DlpResetConfirm', '::Settings:ConfirmTitle').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.isResetting = true;
        this.settingService.resetDlpSettings().subscribe({
          next: () => {
            this.isResetting = false;
            this.toaster.success('::Settings:DlpResetSuccess', '::Settings:SuccessTitle');
            this.loadDlpRules();
          },
          error: (err) => {
            console.error('Failed to reset DLP rules', err);
            this.toaster.error('::Settings:DlpResetError', '::Settings:ErrorTitle');
            this.isResetting = false;
          }
        });
      }
    });
  }

  resetChanges(): void {
    this.dlpJson = this.originalJson;
    this.onJsonChange(this.dlpJson);
  }
}
