import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CoreModule } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { SettingService } from '../../proxy/settings/setting.service';

@Component({
  selector: 'app-inventory-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, CoreModule],
  templateUrl: './inventory-settings.component.html',
  styleUrls: ['./inventory-settings.component.scss']
})
export class InventorySettingsComponent implements OnInit {
  expirationAlertDays: number | null = null;
  originalExpirationAlertDays: number | null = null;
  
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
    this.settingService.getInventorySettings().subscribe({
      next: (data) => {
        this.expirationAlertDays = data.expirationAlertDays;
        this.originalExpirationAlertDays = this.expirationAlertDays;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load Inventory settings', err);
        this.toaster.error('::Settings:InventoryLoadError', '::Settings:InventoryLoadErrorTitle');
        this.isLoading = false;
      }
    });
  }

  saveSettings(): void {
    if (this.expirationAlertDays === null || this.expirationAlertDays <= 0) {
      this.toaster.error('::Settings:InventoryInvalidDays', '::Settings:InventoryInvalidDaysTitle');
      return;
    }

    this.isSaving = true;
    this.settingService.updateInventorySettings({
      expirationAlertDays: this.expirationAlertDays
    }).subscribe({
      next: () => {
        this.originalExpirationAlertDays = this.expirationAlertDays;
        this.isSaving = false;
        this.toaster.success('::Settings:InventorySaveSuccess', '::Settings:SuccessTitle');
      },
      error: (err) => {
        console.error('Failed to save Inventory settings', err);
        this.toaster.error('::Settings:InventorySaveError', '::Settings:ErrorTitle');
        this.isSaving = false;
      }
    });
  }

  resetSettings(): void {
    this.confirmation.warn('::Settings:InventoryResetConfirm', '::Settings:ConfirmTitle').subscribe((status) => {
      if (status === Confirmation.Status.confirm) {
        this.isResetting = true;
        this.settingService.resetInventorySettings().subscribe({
          next: () => {
            this.isResetting = false;
            this.toaster.success('::Settings:InventoryResetSuccess', '::Settings:SuccessTitle');
            this.loadSettings();
          },
          error: (err) => {
            console.error('Failed to reset Inventory settings', err);
            this.toaster.error('::Settings:InventoryResetError', '::Settings:ErrorTitle');
            this.isResetting = false;
          }
        });
      }
    });
  }

  resetChanges(): void {
    this.expirationAlertDays = this.originalExpirationAlertDays;
  }

  get isDirty(): boolean {
    return this.expirationAlertDays !== this.originalExpirationAlertDays;
  }
}
