namespace SupplyCoreERP.Settings;

public static class SupplyCoreERPSettings
{
    private const string Prefix = "SupplyCoreERP";

    //Add your own setting names here. Example:
    //public const string MySetting1 = Prefix + ".MySetting1";
    public const string DlpRules = Prefix + ".Agent.DlpRules";
    public const string LlmProviderModel = Prefix + ".LlmProvider.Model";
    public const string LlmProviderApiKey = Prefix + ".LlmProvider.ApiKey";
    public const string McpServerBaseUrl = Prefix + ".McpServer.BaseUrl";
    public const string ExpirationAlertDays = Prefix + ".Inventory.ExpirationAlertDays";
}






