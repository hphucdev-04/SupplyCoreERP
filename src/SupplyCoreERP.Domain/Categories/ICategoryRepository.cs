using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Categories
{
    public interface ICategoryRepository : IRepository<Category, Guid>
    {
        Task<bool> IsNameExistsAsync(string name);
        Task<bool> IsNameExistsAsync(string name, Guid excludeId);
    }
}
