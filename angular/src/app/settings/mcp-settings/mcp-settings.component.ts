import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CoreModule } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { SettingService } from '../../proxy/settings/setting.service';

@Component({
  selector: 'app-mcp-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, CoreModule],
  templateUrl: './mcp-settings.component.html',
  styleUrls: ['./mcp-settings.component.scss']
})
export class McpSettingsComponent implements OnInit {
  baseUrl = '';
  originalBaseUrl = '';
  
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
    this.settingService.getMcpSettings().subscribe({
      next: (data) => {
        this.baseUrl = data.baseUrl || '';
        this.originalBaseUrl = this.baseUrl;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load MCP settings', err);
        this.toaster.error('::Settings:McpLoadError', '::Settings:McpLoadErrorTitle');
        this.isLoading = false;
      }
    });
  }

  saveSettings(): void {
    this.isSaving = true;
    this.settingService.updateMcpSettings({
      baseUrl: this.baseUrl
    }).subscribe({
      next: () => {
        this.originalBaseUrl = this.baseUrl;
        this.isSaving = false;
        this.toaster.success('::Settings:McpSaveSuccess', '::Settings:SuccessTitle');
      },
      error: (err) => {
        console.error('Failed to save MCP settings', err);
        this.toaster.error('::Settings:McpSaveError', '::Settings:ErrorTitle');
        this.isSaving = false;
      }
    });
  }

  resetSettings(): void {
    this.confirmation.warn('::Settings:McpResetConfirm', '::Settings:ConfirmTitle').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.isResetting = true;
        this.settingService.resetMcpSettings().subscribe({
          next: () => {
            this.isResetting = false;
            this.toaster.success('::Settings:McpResetSuccess', '::Settings:SuccessTitle');
            this.loadSettings();
          },
          error: (err) => {
            console.error('Failed to reset MCP settings', err);
            this.toaster.error('::Settings:McpResetError', '::Settings:ErrorTitle');
            this.isResetting = false;
          }
        });
      }
    });
  }

  resetChanges(): void {
    this.baseUrl = this.originalBaseUrl;
  }

  get isDirty(): boolean {
    return this.baseUrl !== this.originalBaseUrl;
  }
}
