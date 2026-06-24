using System.Collections.Generic;

namespace SupplyCoreERP.Settings.Dtos;

public class DlpRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string Replacement { get; set; } = string.Empty;
}

public class DlpSettingsDto
{
    public List<DlpRuleDto> Rules { get; set; } = new();
}
