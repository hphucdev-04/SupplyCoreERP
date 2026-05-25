using System;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Products;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Sales.PriceLists;

public class ProductPrice : FullAuditedEntity<Guid>
{
    public Guid PriceListId { get; private set; }
    public virtual PriceList PriceList { get; private set; }
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; private set; }
    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; private set; }
    public decimal Price { get; private set; }

    public int MinQuantity { get; private set; }

    protected ProductPrice() { }

    public ProductPrice(
        Guid id,
        Guid priceListId,
        Guid productId,
        Guid unitId,
        decimal price,
        int minQuantity = 1)
        : base(id)
    {
        PriceListId = priceListId;
        ProductId = productId;
        UnitId = unitId;
        Price = price;
        MinQuantity = minQuantity;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
        {
            throw new BusinessException("SupplyCoreERP:InvalidPrice", "Giá bán không được nhỏ hơn 0!");
        }

        Price = newPrice;
    }
}






