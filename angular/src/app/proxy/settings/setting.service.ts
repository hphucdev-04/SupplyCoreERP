import type { DlpSettingsDto, InventorySettingsDto, LlmProviderSettingsDto, McpSettingsDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SettingService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  getDlpSettings = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, DlpSettingsDto>({
      method: 'GET',
      url: '/api/app/setting/dlp-settings',
    },
    { apiName: this.apiName,...config });
  

  getInventorySettings = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventorySettingsDto>({
      method: 'GET',
      url: '/api/app/setting/inventory-settings',
    },
    { apiName: this.apiName,...config });
  

  getLlmProviderSettings = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, LlmProviderSettingsDto>({
      method: 'GET',
      url: '/api/app/setting/llm-provider-settings',
    },
    { apiName: this.apiName,...config });
  

  getMcpSettings = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, McpSettingsDto>({
      method: 'GET',
      url: '/api/app/setting/mcp-settings',
    },
    { apiName: this.apiName,...config });
  

  resetDlpSettings = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/setting/reset-dlp-settings',
    },
    { apiName: this.apiName,...config });
  

  resetInventorySettings = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/setting/reset-inventory-settings',
    },
    { apiName: this.apiName,...config });
  

  resetLlmProviderSettings = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/setting/reset-llm-provider-settings',
    },
    { apiName: this.apiName,...config });
  

  resetMcpSettings = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/setting/reset-mcp-settings',
    },
    { apiName: this.apiName,...config });
  

  updateDlpSettings = (input: DlpSettingsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/setting/dlp-settings',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateInventorySettings = (input: InventorySettingsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/setting/inventory-settings',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateLlmProviderSettings = (input: LlmProviderSettingsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/setting/llm-provider-settings',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateMcpSettings = (input: McpSettingsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/setting/mcp-settings',
      body: input,
    },
    { apiName: this.apiName,...config });
}