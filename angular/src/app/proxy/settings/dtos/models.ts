
export interface DlpRuleDto {
  name?: string;
  pattern?: string;
  replacement?: string;
}

export interface DlpSettingsDto {
  rules?: DlpRuleDto[];
}

export interface InventorySettingsDto {
  expirationAlertDays?: number;
}

export interface LlmProviderSettingsDto {
  model?: string;
  apiKey?: string;
}

export interface McpSettingsDto {
  baseUrl?: string;
}
