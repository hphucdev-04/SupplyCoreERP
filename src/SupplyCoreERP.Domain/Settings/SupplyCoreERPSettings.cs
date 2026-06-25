namespace SupplyCoreERP.Settings;

public static class SupplyCoreERPSettings
{
    private const string Prefix = "SupplyCoreERP";
    public const string LlmProviderModelEnvironmentVariable = "SUPPLYCOREERP_LLM_PROVIDER_MODEL";
    public const string LlmProviderApiKeyEnvironmentVariable = "SUPPLYCOREERP_LLM_PROVIDER_API_KEY";
    public const string GeminiModelEnvironmentVariable = "GEMINI_MODEL";
    public const string GeminiApiKeyEnvironmentVariable = "GEMINI_API_KEY";

    //Add your own setting names here. Example:
    //public const string MySetting1 = Prefix + ".MySetting1";
    public const string DlpRules = Prefix + ".Agent.DlpRules";
    public const string LlmProviderModel = Prefix + ".LlmProvider.Model";
    public const string LlmProviderApiKey = Prefix + ".LlmProvider.ApiKey";
    public const string McpServerBaseUrl = Prefix + ".McpServer.BaseUrl";
    public const string ExpirationAlertDays = Prefix + ".Inventory.ExpirationAlertDays";
    public const string AgentMaxHistoryMessages = Prefix + ".Agent.MaxHistoryMessages";
}






