using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.EntityFrameworkCore;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SupplyCoreERP.Suppliers;

public class SupplierRepository : EfCoreRepository<SupplyCoreERPDbContext, Supplier, Guid>, ISupplierRepository
{
    public SupplierRepository(IDbContextProvider<SupplyCoreERPDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> IsCodeExistsAsync(string code, Guid? excludeId = null)
    {
        var dbSet = await GetDbSetAsync();
        var normalizedCode = code.Trim().ToUpper();
        return await dbSet.AnyAsync(x => x.Code == normalizedCode && x.Id != excludeId);
    }

    public async Task<bool> IsNameExistsAsync(string name, Guid? excludeId = null)
    {
        var dbSet = await GetDbSetAsync();
        var normalizedName = name.Trim().ToLower();
        return await dbSet.AnyAsync(x => x.Name.ToLower() == normalizedName && x.Id != excludeId);
    }

    public async Task<Supplier> GetWithDetailsAsync(Guid id)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(x => x.Country)
            .Include(x => x.City)
            .Include(x => x.Area)
            .Include(x => x.SupplierProducts)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(Supplier), id);
    }
}
