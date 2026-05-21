using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    private readonly SupplierManager _supplierManager;

    // Constructor dependency injection
    public SupplierAppService(
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<SupplierProduct, Guid> supplierProductRepo,
        SupplierManager supplierManager)
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
            .Where(x => x.SupplierId == supplierId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x =>
                x.Product.Name.Contains(input.Filter) ||
                x.Product.Code.Contains(input.Filter))
            .WhereIf(input.IsPreferred.HasValue, x => x.IsPreferred == input.IsPreferred)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(input.MinPrice.HasValue, x => x.StandardPrice >= input.MinPrice)
            .WhereIf(input.MaxPrice.HasValue, x => x.StandardPrice <= input.MaxPrice);

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
            .Where(x => x.ProductId == productId && x.IsActive)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x =>
                x.Supplier.Name.Contains(input.Filter) ||
                x.Supplier.Code.Contains(input.Filter));

        int totalCount = await query.CountAsync();

        List<SupplierProduct> list = await query
            .OrderBy(input.Sorting ?? "IsPreferred DESC, StandardPrice ASC")
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
                                  .FirstOrDefaultAsync(x => x.Id == supplierId)
            ?? throw new EntityNotFoundException(typeof(Supplier), supplierId);

        SupplierProduct sp = await _supplierManager.AddProductAsync(
            supplier,
            input.ProductId,
            input.DefaultUnitId,
            input.DefaultConversionFactor,
            input.StandardPrice,
            input.LeadTimeDays,
            input.MinOrderQuantity,
            input.OverDeliveryTolerancePct,
            input.UnderDeliveryTolerancePct,
            input.IsPreferred,
            input.Note);

        await _supplierRepository.UpdateAsync(supplier);

        return ObjectMapper.Map<SupplierProduct, SupplierProductDto>(sp);
    }

    public async Task<SupplierProductDto> UpdateProductAsync(Guid supplierId, Guid productId, CreateUpdateSupplierProductDto input)
    {
        IQueryable<Supplier> query = await _supplierRepository.GetQueryableAsync();
        Supplier supplier = await query.Include(x => x.SupplierProducts)
                                  .FirstOrDefaultAsync(x => x.Id == supplierId)
            ?? throw new EntityNotFoundException(typeof(Supplier), supplierId);

        await _supplierManager.UpdateProductAsync(
            supplier,
            productId,
            input.DefaultUnitId, input.DefaultConversionFactor, input.StandardPrice,
            input.LeadTimeDays, input.MinOrderQuantity,
            input.OverDeliveryTolerancePct, input.UnderDeliveryTolerancePct,
            input.IsPreferred, input.Note);

        await _supplierRepository.UpdateAsync(supplier);

        SupplierProduct sp = supplier.SupplierProducts.First(x => x.ProductId == productId);
        return ObjectMapper.Map<SupplierProduct, SupplierProductDto>(sp);
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
        // 1. Lấy tất cả danh mục sản phẩm của các NCC đang hoạt động cho danh sách thuốc yêu cầu
        IQueryable<SupplierProduct> query = await _supplierProductRepo.GetQueryableAsync();
        List<SupplierProduct> allSupplierProducts = await query
            .Include(x => x.Supplier)
            .Where(x => productIds.Contains(x.ProductId) && x.IsActive && x.Supplier.IsActive)
            .ToListAsync();

        if (!allSupplierProducts.Any())
        {
            return new List<SourcingSuggestionDto>();
        }

        var results = new List<SourcingSuggestionDto>();

        // 2. Nhóm theo từng sản phẩm để chấm điểm đối đầu
        IEnumerable<IGrouping<Guid, SupplierProduct>> productGroups = allSupplierProducts.GroupBy(x => x.ProductId);

        foreach (IGrouping<Guid, SupplierProduct> group in productGroups)
        {
            Guid productId = group.Key;
            var items = group.ToList();

            // Tìm giá trị tối ưu trong nhóm để làm mốc (benchmark)
            decimal minPrice = items.Min(x => x.StandardPrice);
            int minLeadTime = items.Min(x => x.LeadTimeDays);

            // Chấm điểm tất cả NCC cho sản phẩm này
            var scoredItems = items.Select(sp =>
            {
                // Điểm Giá (70% - max 700): (MinPrice / CurrentPrice) * 700
                double priceScore = sp.StandardPrice > 0
                    ? (double)(minPrice / sp.StandardPrice) * 700
                    : 700;

                // Điểm Thời gian (30% - max 300): (MinTime / CurrentTime) * 300
                double timeScore = sp.LeadTimeDays > 0
                    ? (double)minLeadTime / sp.LeadTimeDays * 300
                    : 300;

                // Điểm thưởng ưu tiên thủ công
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
