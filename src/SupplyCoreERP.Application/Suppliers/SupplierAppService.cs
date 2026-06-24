using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Suppliers.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Suppliers;

public class SupplierAppService : SupplyCore, ISupplierAppService
{
    // Dependency
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<SupplierProduct, Guid> _supplierProductRepo;
    private readonly ISupplierManager _supplierManager;

    // Constructor injection
    public SupplierAppService(
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<SupplierProduct, Guid> supplierProductRepo,
        ISupplierManager supplierManager)
    {
        _supplierRepository = supplierRepository;
        _supplierProductRepo = supplierProductRepo;
        _supplierManager = supplierManager;
    }

    #region Supplier
    public async Task<SupplierDetailDto> GetAsync(Guid id)
    {
        IQueryable<Supplier> query = await _supplierRepository.GetQueryableAsync();
        Supplier supplier = await query
            .Include(x => x.Country)
            .Include(x => x.City)
            .Include(x => x.Area)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(Supplier), id);

        return ObjectMapper.Map<Supplier, SupplierDetailDto>(supplier);
    }

    public async Task<PagedResultDto<SupplierDto>> GetListAsync(GetSupplierListDto input)
    {
        IQueryable<Supplier> query = await _supplierRepository.GetQueryableAsync();
        query = query
            .Include(x => x.City)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x =>
                x.Name.Contains(input.Filter) ||
                x.Code.Contains(input.Filter))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

        int totalCount = await query.CountAsync();
        List<Supplier> items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<SupplierDto>(
            totalCount,
            ObjectMapper.Map<List<Supplier>, List<SupplierDto>>(items));
    }

    public async Task<SupplierDetailDto> CreateAsync(CreateUpdateSupplierDto input)
    {
        Supplier supplier = await _supplierManager.CreateAsync(
            input.Name, input.TaxCode, input.PhoneNumber, input.Email,
            input.RepresentativeName, input.Gender, input.Note,
            input.Address, input.CountryId, input.CityId, input.AreaId,
            input.DebtLimit, input.PaymentTermDays);

        supplier.SetActive(input.IsActive);
        await _supplierRepository.InsertAsync(supplier);
        return ObjectMapper.Map<Supplier, SupplierDetailDto>(supplier);
    }

    public async Task<SupplierDetailDto> UpdateAsync(Guid id, CreateUpdateSupplierDto input)
    {
        Supplier supplier = await _supplierRepository.GetAsync(id);
        await _supplierManager.UpdateAsync(
            supplier, input.Name, input.TaxCode, input.PhoneNumber, input.Email,
            input.RepresentativeName, input.Gender, input.Note,
            input.Address, input.CountryId, input.CityId, input.AreaId,
            input.DebtLimit, input.PaymentTermDays);

        supplier.SetActive(input.IsActive);
        await _supplierRepository.UpdateAsync(supplier);
        return ObjectMapper.Map<Supplier, SupplierDetailDto>(supplier);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _supplierManager.DeleteAsync(id);
    }

    public async Task ToggleActiveAsync(Guid id)
    {
        Supplier supplier = await _supplierRepository.GetAsync(id);
        supplier.SetActive(!supplier.IsActive);
        await _supplierRepository.UpdateAsync(supplier);
    }
    #endregion

    #region Supplier Product

    public async Task<PagedResultDto<SupplierProductDto>> GetProductListAsync(Guid supplierId, GetSupplierProductListDto input)
    {
        IQueryable<SupplierProduct> query = await _supplierProductRepo.GetQueryableAsync();

        query = query
            .Include(x => x.Product).ThenInclude(p => p.BaseUnit)
            .Include(x => x.DefaultUnit)
            .Include(x => x.Conditions).ThenInclude(c => c.Unit)
            .Where(x => x.SupplierId == supplierId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x =>
                x.Product.Name.Contains(input.Filter) ||
                x.Product.Code.Contains(input.Filter))
            .WhereIf(input.IsPreferred.HasValue, x => x.IsPreferred == input.IsPreferred)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(input.MinPrice.HasValue, x => x.Conditions.Any(c => c.StandardPrice >= input.MinPrice))
            .WhereIf(input.MaxPrice.HasValue, x => x.Conditions.Any(c => c.StandardPrice <= input.MaxPrice));

        int totalCount = await query.CountAsync();

        List<SupplierProduct> result = await query
            .OrderBy(input.Sorting ?? "Product.Name ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<SupplierProductDto>(
            totalCount,
            ObjectMapper.Map<List<SupplierProduct>, List<SupplierProductDto>>(result)
        );
    }

    public async Task<PagedResultDto<SupplierMedicineDto>> GetSupplierListAsync(Guid productId, GetSupplierMedicineListDto input)
    {
        IQueryable<SupplierProduct> query = await _supplierProductRepo.GetQueryableAsync();

        query = query
            .Include(x => x.Supplier).ThenInclude(s => s.Country)
            .Include(x => x.DefaultUnit)
            .Include(x => x.Conditions)
            .Where(x => x.ProductId == productId && x.IsActive)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x =>
                x.Supplier.Name.Contains(input.Filter) ||
                x.Supplier.Code.Contains(input.Filter));

        int totalCount = await query.CountAsync();

        List<SupplierProduct> list = await query
            .OrderBy(input.Sorting != null && input.Sorting.Contains("StandardPrice") ? "IsPreferred DESC" : (input.Sorting ?? "IsPreferred DESC"))
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<SupplierMedicineDto>(
            totalCount,
            ObjectMapper.Map<List<SupplierProduct>, List<SupplierMedicineDto>>(list));
    }

    public async Task<SupplierProductDto> AddProductAsync(Guid supplierId, CreateUpdateSupplierProductDto input)
    {
        IQueryable<Supplier> query = await _supplierRepository.GetQueryableAsync();
        Supplier supplier = await query.Include(x => x.SupplierProducts)
                                  .ThenInclude(sp => sp.Conditions)
                                  .FirstOrDefaultAsync(x => x.Id == supplierId)
            ?? throw new EntityNotFoundException(typeof(Supplier), supplierId);

        SupplierProduct sp = await _supplierManager.AddProductAsync(
            supplier,
            input.ProductId,
            input.DefaultUnitId,
            input.LeadTimeDays,
            input.IsPreferred,
            input.Note);

        if (input.Conditions != null && input.Conditions.Any())
        {
            foreach (CreateUpdateSupplierProductConditionDto condInput in input.Conditions)
            {
                SupplierProductCondition condition = new(
                    GuidGenerator.Create(),
                    sp.Id,
                    condInput.UnitId,
                    condInput.ConversionFactor,
                    condInput.StandardPrice,
                    condInput.MinOrderQuantity
                );
                sp.AddCondition(condition);
            }
        }

        sp.ValidateConditions();

        await _supplierRepository.UpdateAsync(supplier, autoSave: true);

        IQueryable<SupplierProduct> spQuery = await _supplierProductRepo.GetQueryableAsync();
        SupplierProduct loadedSp = await spQuery
            .Include(x => x.Product)
            .Include(x => x.DefaultUnit)
            .Include(x => x.Conditions).ThenInclude(c => c.Unit)
            .FirstAsync(x => x.Id == sp.Id);

        return ObjectMapper.Map<SupplierProduct, SupplierProductDto>(loadedSp);
    }

    public async Task<SupplierProductDto> UpdateProductAsync(Guid supplierId, Guid productId, CreateUpdateSupplierProductDto input)
    {
        IQueryable<Supplier> query = await _supplierRepository.GetQueryableAsync();
        Supplier supplier = await query.Include(x => x.SupplierProducts)
                                  .ThenInclude(sp => sp.Conditions)
                                  .FirstOrDefaultAsync(x => x.Id == supplierId)
            ?? throw new EntityNotFoundException(typeof(Supplier), supplierId);

        await _supplierManager.UpdateProductAsync(
            supplier,
            productId,
            input.DefaultUnitId,
            input.LeadTimeDays,
            input.IsPreferred,
            input.Note);

        SupplierProduct sp = supplier.SupplierProducts.First(x => x.ProductId == productId);

        if (input.Conditions != null)
        {
            List<Guid> inputIds = input.Conditions.Where(c => c.Id.HasValue).Select(c => c.Id.Value).ToList();

            List<SupplierProductCondition> conditionsToRemove = sp.Conditions.Where(c => !inputIds.Contains(c.Id)).ToList();
            foreach (SupplierProductCondition? cond in conditionsToRemove)
            {
                sp.RemoveCondition(cond.Id);
            }

            foreach (CreateUpdateSupplierProductConditionDto condInput in input.Conditions)
            {
                if (condInput.Id.HasValue)
                {
                    SupplierProductCondition? existingCond = sp.Conditions.FirstOrDefault(c => c.Id == condInput.Id.Value);
                    if (existingCond != null)
                    {
                        existingCond.UpdateCondition(
                            condInput.StandardPrice,
                            condInput.MinOrderQuantity
                        );
                    }
                }
                else
                {
                    SupplierProductCondition newCondition = new(
                        GuidGenerator.Create(),
                        sp.Id,
                        condInput.UnitId,
                        condInput.ConversionFactor,
                        condInput.StandardPrice,
                        condInput.MinOrderQuantity
                    );
                    sp.AddCondition(newCondition);
                }
            }
        }

        sp.ValidateConditions();

        await _supplierRepository.UpdateAsync(supplier, autoSave: true);

        IQueryable<SupplierProduct> spQuery = await _supplierProductRepo.GetQueryableAsync();
        SupplierProduct loadedSp = await spQuery
            .Include(x => x.Product)
            .Include(x => x.DefaultUnit)
            .Include(x => x.Conditions).ThenInclude(c => c.Unit)
            .FirstAsync(x => x.ProductId == productId && x.SupplierId == supplierId);

        return ObjectMapper.Map<SupplierProduct, SupplierProductDto>(loadedSp);
    }

    public async Task RemoveProductAsync(Guid supplierId, Guid productId)
    {
        Supplier supplier = await _supplierRepository.WithDetailsAsync(x => x.SupplierProducts)
            .ContinueWith(t => t.Result.FirstOrDefault(x => x.Id == supplierId))
            ?? throw new EntityNotFoundException(typeof(Supplier), supplierId);

        await _supplierManager.RemoveProductAsync(supplier, productId);

        await _supplierRepository.UpdateAsync(supplier);
    }

    public async Task ToggleProductActiveAsync(Guid supplierId, Guid productId)
    {

        Supplier supplier = await _supplierRepository.WithDetailsAsync(x => x.SupplierProducts)
            .ContinueWith(t => t.Result.FirstOrDefault(x => x.Id == supplierId))
            ?? throw new EntityNotFoundException(typeof(Supplier), supplierId);

        _supplierManager.ToggleProductActive(supplier, productId);

        await _supplierRepository.UpdateAsync(supplier);
    }

    public async Task<List<SourcingSuggestionDto>> GetSourcingSuggestionsAsync(List<Guid> productIds)
    {
        // 1. Láº¥y táº¥t cáº£ danh má»¥c sáº£n pháº©m cá»§a cÃ¡c NCC Ä‘ang hoáº¡t Ä‘á»™ng cho danh sÃ¡ch thuá»‘c yÃªu cáº§u
        IQueryable<SupplierProduct> query = await _supplierProductRepo.GetQueryableAsync();
        List<SupplierProduct> allSupplierProducts = await query
            .Include(x => x.Supplier)
            .Include(x => x.Conditions)
            .Where(x => productIds.Contains(x.ProductId) && x.IsActive && x.Supplier.IsActive)
            .ToListAsync();

        if (!allSupplierProducts.Any())
        {
            return new List<SourcingSuggestionDto>();
        }

        List<SourcingSuggestionDto> results = new();

        // 2. NhÃ³m theo tá»«ng sáº£n pháº©m Ä‘á»ƒ cháº¥m Ä‘iá»ƒm Ä‘á»‘i Ä‘áº§u
        IEnumerable<IGrouping<Guid, SupplierProduct>> productGroups = allSupplierProducts.GroupBy(x => x.ProductId);

        foreach (IGrouping<Guid, SupplierProduct> group in productGroups)
        {
            Guid productId = group.Key;
            List<SupplierProduct> items = group.ToList();

            var benchmarkList = items.Select(sp => new
            {
                SupplierProduct = sp,
                Price = sp.Conditions != null && sp.Conditions.Any(c => c.UnitId == sp.DefaultUnitId)
                    ? sp.Conditions.First(c => c.UnitId == sp.DefaultUnitId).StandardPrice
                    : 0
            }).ToList();

            decimal minPrice = benchmarkList.Min(x => x.Price);
            int minLeadTime = items.Min(x => x.LeadTimeDays);

            // Cháº¥m Ä‘iá»ƒm táº¥t cáº£ NCC cho sáº£n pháº©m nÃ y
            List<SourcingSuggestionDto> scoredItems = benchmarkList.Select(b =>
            {
                SupplierProduct sp = b.SupplierProduct;
                decimal currentPrice = b.Price;

                // Äiá»ƒm GiÃ¡ (70% - max 700): (MinPrice / CurrentPrice) * 700
                double priceScore = currentPrice > 0
                    ? (double)(minPrice / currentPrice) * 700
                    : 700;

                // Äiá»ƒm Thá»i gian (30% - max 300): (MinTime / CurrentTime) * 300
                double timeScore = sp.LeadTimeDays > 0
                    ? (double)minLeadTime / sp.LeadTimeDays * 300
                    : 300;

                // Äiá»ƒm thÆ°á»Ÿng Æ°u tiÃªn thá»§ cÃ´ng
                double bonusScore = sp.IsPreferred ? 500 : 0;

                return new SourcingSuggestionDto
                {
                    ProductId = productId,
                    SupplierId = sp.SupplierId,
                    SupplierName = sp.Supplier.Name,
                    Score = Math.Round(priceScore + timeScore + bonusScore, 2)
                };
            })
            .OrderByDescending(x => x.Score)
            .ToList();

            results.AddRange(scoredItems);
        }

        return results;
    }
    #endregion
}

