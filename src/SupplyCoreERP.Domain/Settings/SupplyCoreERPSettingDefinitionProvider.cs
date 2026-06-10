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
    }
}






