namespace SupplyCoreERP.Settings.Dtos;

public class LlmProviderSettingsDto
{
    public string Model { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ModelSource { get; set; } = string.Empty;
    public string ApiKeySource { get; set; } = string.Empty;
}
