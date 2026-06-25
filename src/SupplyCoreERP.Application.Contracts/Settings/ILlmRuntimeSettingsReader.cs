using System.Threading.Tasks;
using SupplyCoreERP.Settings.Dtos;

namespace SupplyCoreERP.Settings;

public interface ILlmRuntimeSettingsReader
{
    Task<LlmProviderSettingsDto> GetCurrentAsync();
}
