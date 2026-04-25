using SupplyCoreERP.Enums.PriceList;
using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Prices.Dtos
{
    public class PriceListDto : EntityDto<Guid>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public CurrencyType Currency { get; set; }
        public bool IsBase { get; set; }
    }
}
