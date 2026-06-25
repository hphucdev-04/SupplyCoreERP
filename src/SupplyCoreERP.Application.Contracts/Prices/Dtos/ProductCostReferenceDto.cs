using System;

namespace SupplyCoreERP.Prices.Dtos;

public class ProductCostReferenceDto
{
    public Guid ProductId { get; set; }

    public Guid UnitId { get; set; }

    public decimal? LowestPurchasePrice { get; set; }
}
