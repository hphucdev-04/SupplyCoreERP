using System;
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
        string? modelFromEnvironment = GetEnvironmentValue(
            SupplyCoreERPSettings.LlmProviderModelEnvironmentVariable,
            SupplyCoreERPSettings.GeminiModelEnvironmentVariable);

        string? apiKeyFromEnvironment = GetEnvironmentValue(
            SupplyCoreERPSettings.LlmProviderApiKeyEnvironmentVariable,
            SupplyCoreERPSettings.GeminiApiKeyEnvironmentVariable);

        string modelFromSettings = await _settingManager.GetOrNullAsync(
            SupplyCoreERPSettings.LlmProviderModel,
            GlobalProviderName,
            null,
            fallback: true) ?? string.Empty;

        string apiKeyFromSettings = await _settingManager.GetOrNullAsync(
            SupplyCoreERPSettings.LlmProviderApiKey,
            GlobalProviderName,
            null,
            fallback: true) ?? string.Empty;

        string resolvedModel = FirstNonEmpty(modelFromEnvironment, modelFromSettings, "gemini-2.5-flash");
        string resolvedApiKey = FirstNonEmpty(apiKeyFromEnvironment, apiKeyFromSettings, string.Empty);

        return new LlmProviderSettingsDto
        {
            Model = resolvedModel,
            ApiKey = resolvedApiKey,
            ModelSource = ResolveSource(modelFromEnvironment, modelFromSettings, "default"),
            ApiKeySource = ResolveSource(apiKeyFromEnvironment, apiKeyFromSettings, "missing")
        };
    }

    private static string? GetEnvironmentValue(params string[] variableNames)
    {
        foreach (string variableName in variableNames)
        {
            string? value = Environment.GetEnvironmentVariable(variableName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static string FirstNonEmpty(string? primaryValue, string? secondaryValue, string fallbackValue)
    {
        if (!string.IsNullOrWhiteSpace(primaryValue))
        {
            return primaryValue.Trim();
        }

        if (!string.IsNullOrWhiteSpace(secondaryValue))
        {
            return secondaryValue.Trim();
        }

        return fallbackValue;
    }

    private static string ResolveSource(string? environmentValue, string? settingValue, string fallbackSource)
    {
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return "environment";
        }

        if (!string.IsNullOrWhiteSpace(settingValue))
        {
            return "database";
        }

        return fallbackSource;
    }
}
