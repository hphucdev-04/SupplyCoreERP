using System.Threading.Tasks;
using SupplyCoreERP.Settings.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;

namespace SupplyCoreERP.Settings;

public class LlmRuntimeSettingsReader : ILlmRuntimeSettingsReader, ITransientDependency
{
    private const string GlobalProviderName = "G";

    private readonly ISettingManager _settingManager;

    public LlmRuntimeSettingsReader(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    public async Task<LlmProviderSettingsDto> GetCurrentAsync()
    {
        string model = await _settingManager.GetOrNullAsync(
            SupplyCoreERPSettings.LlmProviderModel,
            GlobalProviderName,
            null,
            fallback: true) ?? "gemini-2.5-flash";

        string apiKey = await _settingManager.GetOrNullAsync(
            SupplyCoreERPSettings.LlmProviderApiKey,
            GlobalProviderName,
            null,
            fallback: true) ?? string.Empty;

        return new LlmProviderSettingsDto
        {
            Model = model,
            ApiKey = apiKey
        };
    }
}
