using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SupplyCoreERP.Settings.Dtos;
using Volo.Abp;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace SupplyCoreERP.Settings;

[Authorize]
public class SettingAppService : SupplyCore, ISettingAppService
{
    // Dependencies
    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    // Constructor injection
    public SettingAppService(
        ISettingProvider settingProvider,
        ISettingManager settingManager)
    {
        _settingProvider = settingProvider;
        _settingManager = settingManager;
    }


    #region DLP Settings

    public async Task<DlpSettingsDto> GetDlpSettingsAsync()
    {
        string? dlpRulesJson = await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.DlpRules);
        if (string.IsNullOrEmpty(dlpRulesJson))
        {
            return new DlpSettingsDto();
        }

        try
        {
            List<DlpRuleDto>? rules = JsonSerializer.Deserialize<List<DlpRuleDto>>(dlpRulesJson, _jsonOptions);
            return new DlpSettingsDto { Rules = rules ?? new List<DlpRuleDto>() };
        }
        catch
        {
            return new DlpSettingsDto();
        }
    }

    public async Task UpdateDlpSettingsAsync(DlpSettingsDto input)
    {
        string dlpRulesJson = JsonSerializer.Serialize(input.Rules, _jsonOptions);
        await _settingManager.SetGlobalAsync(SupplyCoreERPSettings.DlpRules, dlpRulesJson);
    }

    public async Task ResetDlpSettingsAsync()
    {
        await _settingManager.SetGlobalAsync(SupplyCoreERPSettings.DlpRules, null);
    }

    #endregion

    #region LLM Provider Settings

    public async Task<LlmProviderSettingsDto> GetLlmProviderSettingsAsync()
    {
        string model = await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.LlmProviderModel) ?? "gemini-2.5-flash";
        string apiKey = await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.LlmProviderApiKey) ?? string.Empty;

        return new LlmProviderSettingsDto
        {
            Model = model,
            ApiKey = apiKey
        };
    }

    public async Task UpdateLlmProviderSettingsAsync(LlmProviderSettingsDto input)
    {
        await _settingManager.SetGlobalAsync(SupplyCoreERPSettings.LlmProviderModel, input.Model ?? string.Empty);
        await _settingManager.SetGlobalAsync(SupplyCoreERPSettings.LlmProviderApiKey, input.ApiKey ?? string.Empty);
    }

    public async Task ResetLlmProviderSettingsAsync()
    {
        await _settingManager.SetGlobalAsync(SupplyCoreERPSettings.LlmProviderModel, null);
        await _settingManager.SetGlobalAsync(SupplyCoreERPSettings.LlmProviderApiKey, null);
    }

    #endregion

    #region MCP Server Settings

    public async Task<McpSettingsDto> GetMcpSettingsAsync()
    {
        string baseUrl = await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.McpServerBaseUrl) ?? "http://localhost:3000";

        return new McpSettingsDto
        {
            BaseUrl = baseUrl
        };
    }

    public async Task UpdateMcpSettingsAsync(McpSettingsDto input)
    {
        await _settingManager.SetGlobalAsync(SupplyCoreERPSettings.McpServerBaseUrl, input.BaseUrl ?? string.Empty);
    }

    public async Task ResetMcpSettingsAsync()
    {
        await _settingManager.SetGlobalAsync(SupplyCoreERPSettings.McpServerBaseUrl, null);
    }

    #endregion

    #region Inventory Settings

    public async Task<InventorySettingsDto> GetInventorySettingsAsync()
    {
        string alertDaysStr = await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.ExpirationAlertDays) ?? "30";
        int.TryParse(alertDaysStr, out int alertDays);

        return new InventorySettingsDto
        {
            ExpirationAlertDays = alertDays > 0 ? alertDays : 30
        };
    }

    public async Task UpdateInventorySettingsAsync(InventorySettingsDto input)
    {
        if (input.ExpirationAlertDays <= 0)
        {
            throw new UserFriendlyException("Số ngày cảnh báo gần hết hạn phải lớn hơn 0.");
        }

        await _settingManager.SetGlobalAsync(SupplyCoreERPSettings.ExpirationAlertDays, input.ExpirationAlertDays.ToString());
    }

    public async Task ResetInventorySettingsAsync()
    {
        await _settingManager.SetGlobalAsync(SupplyCoreERPSettings.ExpirationAlertDays, null);
    }

    #endregion
}
