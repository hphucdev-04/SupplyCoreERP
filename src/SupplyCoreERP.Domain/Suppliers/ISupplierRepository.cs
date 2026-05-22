using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Suppliers;

public interface ISupplierRepository : IRepository<Supplier, Guid>
{
    Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> IsNameExistsAsync(string name, Guid? excludeId = null);
    Task<Supplier> GetWithDetailsAsync(Guid id);
}
