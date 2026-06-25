using Volo.Abp.Settings;

namespace SupplyCoreERP.Settings;

public class SupplyCoreERPSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(SupplyCoreERPSettings.MySetting1));
        context.Add(new SettingDefinition(
            SupplyCoreERPSettings.DlpRules,
            defaultValue: """[{"Name":"TAXCODE","Pattern":"\\b\\d{10}(?:-\\d{3})?\\b","Replacement":"[REDACTED_TAXCODE]"},{"Name":"PHONENUMBER","Pattern":"(?:\\+84|0)[35789]\\d{8}\\b","Replacement":"[REDACTED_PHONENUMBER]"},{"Name":"EMAIL","Pattern":"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}","Replacement":"[REDACTED_EMAIL]"}]""",
            isVisibleToClients: false
        ));

        context.Add(new SettingDefinition(
            SupplyCoreERPSettings.LlmProviderModel,
            defaultValue: "",
            isVisibleToClients: false
        ));

        context.Add(new SettingDefinition(
            SupplyCoreERPSettings.LlmProviderApiKey,
            defaultValue: "AQ.Ab8RN6LtJZGdEXlfMP1ayfA_Ulh-G44_Iwjj5oysFN6WH3gc1g",
            isVisibleToClients: false,
            isEncrypted: true
        ));

        context.Add(new SettingDefinition(
            SupplyCoreERPSettings.McpServerBaseUrl,
            defaultValue: "https://rxlogistics-mcp.up.railway.app/",
            isVisibleToClients: false
        ));

        context.Add(new SettingDefinition(
            SupplyCoreERPSettings.ExpirationAlertDays,
            defaultValue: "30",
            isVisibleToClients: false
        ));

        context.Add(new SettingDefinition(
            SupplyCoreERPSettings.AgentMaxHistoryMessages,
            defaultValue: "20",
            isVisibleToClients: true
        ));
    }
}






