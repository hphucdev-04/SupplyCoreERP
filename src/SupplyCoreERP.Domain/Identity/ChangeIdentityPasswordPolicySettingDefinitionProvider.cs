using Volo.Abp.Identity.Settings;
using Volo.Abp.Settings;

namespace SupplyCoreERP.Identity;

public class ChangeIdentityPasswordPolicySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        SettingDefinition? requireNonAlphanumeric = context.GetOrNull(IdentitySettingNames.Password.RequireNonAlphanumeric);
        if (requireNonAlphanumeric != null)
        {
            requireNonAlphanumeric.DefaultValue = false.ToString();
        }

        SettingDefinition? requireLowercase = context.GetOrNull(IdentitySettingNames.Password.RequireLowercase);
        if (requireLowercase != null)
        {
            requireLowercase.DefaultValue = false.ToString();
        }

        SettingDefinition? requireUppercase = context.GetOrNull(IdentitySettingNames.Password.RequireUppercase);
        if (requireUppercase != null)
        {
            requireUppercase.DefaultValue = false.ToString();
        }

        SettingDefinition? requireDigit = context.GetOrNull(IdentitySettingNames.Password.RequireDigit);
        if (requireDigit != null)
        {
            requireDigit.DefaultValue = false.ToString();
        }
    }
}
