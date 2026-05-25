using System;
using SupplyCoreERP.Enums.PriceList;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Prices.Dtos;

public class ProductPriceDto : EntityDto<Guid>
{
    public Guid PriceListId { get; set; }
    public string PriceListName { get; set; }
    public string PriceListCode { get; set; }
    public CurrencyType Currency { get; set; }

    public Guid UnitId { get; set; }
    public string UnitName { get; set; }

    public decimal Price { get; set; }
    public int MinQuantity { get; set; }
}

