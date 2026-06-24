using System.Threading.Tasks;
using SupplyCoreERP.Settings.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Settings;

public interface ISettingAppService : IApplicationService
{
    Task<DlpSettingsDto> GetDlpSettingsAsync();
    Task UpdateDlpSettingsAsync(DlpSettingsDto input);

    Task<LlmProviderSettingsDto> GetLlmProviderSettingsAsync();
    Task UpdateLlmProviderSettingsAsync(LlmProviderSettingsDto input);

    Task<McpSettingsDto> GetMcpSettingsAsync();
    Task UpdateMcpSettingsAsync(McpSettingsDto input);

    Task<InventorySettingsDto> GetInventorySettingsAsync();
    Task UpdateInventorySettingsAsync(InventorySettingsDto input);

    Task ResetDlpSettingsAsync();
    Task ResetLlmProviderSettingsAsync();
    Task ResetMcpSettingsAsync();
    Task ResetInventorySettingsAsync();
}
