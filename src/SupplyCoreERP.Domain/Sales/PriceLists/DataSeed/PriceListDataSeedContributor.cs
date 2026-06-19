using System;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.PriceList;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace SupplyCoreERP.Sales.PriceLists.DataSeed;

public class PriceListDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<PriceList, Guid> _priceListRepository;
    private readonly IGuidGenerator _guidGenerator;

    public PriceListDataSeedContributor(
        IRepository<PriceList, Guid> priceListRepository,
        IGuidGenerator guidGenerator)
    {
        _priceListRepository = priceListRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _priceListRepository.GetCountAsync() > 0)
        {
            return;
        }

        await _priceListRepository.InsertAsync(
            new PriceList(
                id: _guidGenerator.Create(),
                code: "PRICE-BASE",
                name: "Bảng giá gốc",
                isBase: true,
                currency: CurrencyType.VND
            ),
            autoSave: true
        );

        await _priceListRepository.InsertAsync(
            new PriceList(
                id: _guidGenerator.Create(),
                code: "PRICE-RETAIL",
                name: "Bảng giá bán lẻ",
                isBase: false,
                currency: CurrencyType.VND
            ),
            autoSave: true
        );

        await _priceListRepository.InsertAsync(
            new PriceList(
                id: _guidGenerator.Create(),
                code: "PRICE-WHOLESALE",
                name: "Bảng giá bán sỉ",
                isBase: false,
                currency: CurrencyType.VND
            ),
            autoSave: true
        );

        await _priceListRepository.InsertAsync(
           new PriceList(
               id: _guidGenerator.Create(),
               code: "PRICE-HOSPITAL",
               name: "Bảng giá bán bệnh viện",
               isBase: false,
               currency: CurrencyType.VND
           ),
           autoSave: true
       );

        await _priceListRepository.InsertAsync(
            new PriceList(
                id: _guidGenerator.Create(),
                code: "PRICE-USD",
                name: "Bảng giá niêm yết USD",
                isBase: false,
                currency: CurrencyType.USD
            ),
            autoSave: true
        );
    }
}






