using System;

namespace SupplyCoreERP.SeedData;

public static class TestDataConsts
{
    // Bounded Context: Catalog
    public static readonly Guid CategoryMedicineId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ManufacturerAId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid UnitBoxId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid UnitPillId = Guid.Parse("33333333-3333-3333-3333-444444444444");

    public static readonly Guid DosageTabletId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid ActiveIngredientParacetamolId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    public static readonly Guid MedicineParacetamolId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid Batch001Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    // Bounded Context: Partner
    public static readonly Guid CountryVNId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid CityHCMId = Guid.Parse("77777777-7777-7777-8888-888888888888");
    public static readonly Guid AreaQ1Id = Guid.Parse("77777777-7777-7777-9999-999999999999");

    public static readonly Guid SupplierAId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    public static readonly Guid CustomerAId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid PriceListOfficialId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    // Bounded Context: Inventory
    public static readonly Guid WarehouseMainId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static readonly Guid ZoneCoolId = Guid.Parse("88888888-8888-8888-9999-999999999999");
    public static readonly Guid BinA1Id = Guid.Parse("88888888-8888-8888-aaaa-aaaaaaaaaaaa");
}
