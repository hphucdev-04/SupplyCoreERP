using System;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.PriceList;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace SupplyCoreERP.Prices.DataSeed;

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
        // Kiểm tra nếu đã có dữ liệu thì không seed nữa để tránh trùng lặp khi chạy Migrator
        if (await _priceListRepository.GetCountAsync() > 0)
        {
            return;
        }

        // Bảng giá gốc  - Dùng làm giá tham chiếu hoặc fallback
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

        // Bảng giá bán lẻ
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

        // Bảng giá sỉ (Wholesale)
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

        // Bảng giá bênh viện (Hospital)
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

        // Bảng giá ngoại tệ USE
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

        // Bảng giá ngoại tệ EUR
        await _priceListRepository.InsertAsync(
            new PriceList(
                id: _guidGenerator.Create(),
                code: "PRICE-EUR",
                name: "Bảng giá niêm yết EUR",
                isBase: false,
                currency: CurrencyType.EUR
            ),
            autoSave: true
        );
    }
}
