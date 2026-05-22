using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SupplyCoreERP.Categories;

public class EfCoreCategoryRepository : EfCoreRepository<SupplyCoreERPDbContext, Category, Guid>, ICategoryRepository
{
    public EfCoreCategoryRepository(IDbContextProvider<SupplyCoreERPDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsNameExistsAsync(string name)
    {
        DbSet<Category> dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => x.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> IsNameExistsAsync(string name, Guid excludeId)
    {
        DbSet<Category> dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => x.Name.ToLower() == name.ToLower() && x.Id != excludeId);
    }
}
