import { EnvironmentProviders, inject, makeEnvironmentProviders, provideAppInitializer } from '@angular/core';
import { SettingTabsService } from '@abp/ng.setting-management/config';
import { DlpSettingsComponent } from '../settings/dlp-settings/dlp-settings.component';
import { LlmProviderSettingsComponent } from '../settings/llm-provider-settings/llm-provider-settings.component';
import { McpSettingsComponent } from '../settings/mcp-settings/mcp-settings.component';
import { InventorySettingsComponent } from '../settings/inventory-settings/inventory-settings.component';

export function provideSettingsConfig(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideAppInitializer(() => {
      const settingTabs = inject(SettingTabsService);
      settingTabs.add([
        {
          name: 'DLP Settings',
          order: 101,
          component: DlpSettingsComponent,
        },
        {
          name: 'LLM Provider Settings',
          order: 102,
          component: LlmProviderSettingsComponent,
        },
        {
          name: 'MCP Server Settings',
          order: 103,
          component: McpSettingsComponent,
        },
        {
          name: 'Inventory Alert Settings',
          order: 104,
          component: InventorySettingsComponent,
        }
      ]);
    }),
  ]);
}
