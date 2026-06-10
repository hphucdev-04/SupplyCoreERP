import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CoreModule } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { SettingService } from '../../proxy/settings/setting.service';

@Component({
  selector: 'app-llm-provider-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, CoreModule],
  templateUrl: './llm-provider-settings.component.html',
  styleUrls: ['./llm-provider-settings.component.scss']
})
export class LlmProviderSettingsComponent implements OnInit {
  model = '';
  apiKey = '';
  originalModel = '';
  originalApiKey = '';
  
  isLoading = false;
  isSaving = false;
  isResetting = false;

  constructor(
    private settingService: SettingService,
    private toaster: ToasterService,
    private confirmation: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.isLoading = true;
    this.settingService.getLlmProviderSettings().subscribe({
      next: (data) => {
        this.model = data.model || '';
        this.apiKey = data.apiKey || '';
        this.originalModel = this.model;
        this.originalApiKey = this.apiKey;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load LLM settings', err);
        this.toaster.error('::Settings:LlmLoadError', '::Settings:LlmLoadErrorTitle');
        this.isLoading = false;
      }
    });
  }

  saveSettings(): void {
    this.isSaving = true;
    this.settingService.updateLlmProviderSettings({
      model: this.model,
      apiKey: this.apiKey
    }).subscribe({
      next: () => {
        this.originalModel = this.model;
        this.originalApiKey = this.apiKey;
        this.isSaving = false;
        this.toaster.success('::Settings:LlmSaveSuccess', '::Settings:SuccessTitle');
      },
      error: (err) => {
        console.error('Failed to save LLM settings', err);
        this.toaster.error('::Settings:LlmSaveError', '::Settings:ErrorTitle');
        this.isSaving = false;
      }
    });
  }

  resetSettings(): void {
    this.confirmation.warn('::Settings:LlmResetConfirm', '::Settings:ConfirmTitle').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.isResetting = true;
        this.settingService.resetLlmProviderSettings().subscribe({
          next: () => {
            this.isResetting = false;
            this.toaster.success('::Settings:LlmResetSuccess', '::Settings:SuccessTitle');
            this.loadSettings();
          },
          error: (err) => {
            console.error('Failed to reset LLM settings', err);
            this.toaster.error('::Settings:LlmResetError', '::Settings:ErrorTitle');
            this.isResetting = false;
          }
        });
      }
    });
  }

  resetChanges(): void {
    this.model = this.originalModel;
    this.apiKey = this.originalApiKey;
  }

  get isDirty(): boolean {
    return this.model !== this.originalModel || this.apiKey !== this.originalApiKey;
  }
}
