using System.Threading.Tasks;

namespace SupplyCoreERP.Data;

public interface ISupplyCoreERPDbSchemaMigrator
{
    Task MigrateAsync();
}
