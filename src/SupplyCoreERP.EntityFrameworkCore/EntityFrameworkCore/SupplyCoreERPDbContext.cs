using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.ActiveIngredients;
using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Categories;
using SupplyCoreERP.Customers;
using SupplyCoreERP.DosageForms;
using SupplyCoreERP.Inventories.Balances;
using SupplyCoreERP.Inventories.Batches;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Inventories.Transactions;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Manufacturers;
using SupplyCoreERP.Medicines;
using SupplyCoreERP.Orders.PO;
using SupplyCoreERP.Prices;
using SupplyCoreERP.Products;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.Suppliers;
using SupplyCoreERP.Warehouses;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace SupplyCoreERP.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ConnectionStringName("Default")]
public class SupplyCoreERPDbContext :
    AbpDbContext<SupplyCoreERPDbContext>,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */


    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext 
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext .
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

	// Catalog
	public DbSet<Category> Categories { get; set; }
	public DbSet<BaseUnit> BaseUnits { get; set; }
	public DbSet<DosageForm> DosageForms { get; set; }
	public DbSet<ActiveIngredient> ActiveIngredients { get; set; }
	public DbSet<Manufacturer> Manufacturers { get; set; }

	// Location
	public DbSet<Continent> Continents { get; set; }
	public DbSet<Country> Countries { get; set; }
	public DbSet<City> Cities { get; set; }
	public DbSet<Area> Areas { get; set; }

	// Product
	public DbSet<Product> Products { get; set; }
	public DbSet<ProductUnit> ProductUnits { get; set; }

	// Medicine
	public DbSet<Medicine> Medicines { get; set; }
	public DbSet<MedicineIngredient> MedicineIngredients { get; set; }

	// Price
	public DbSet<PriceList> PriceLists { get; set; }
	public DbSet<ProductPrice> ProductPrices { get; set; }

	// Partner
	public DbSet<Customer> Customers { get; set; }
	public DbSet<Supplier> Suppliers { get; set; }

	// Warehouse & Inventory
	public DbSet<Warehouse> Warehouses { get; set; }
	public DbSet<Zone> Zones { get; set; }
	public DbSet<Bin> Bins { get; set; }
	public DbSet<ProductBatch> ProductBatches { get; set; }
	public DbSet<InventoryTicket> InventoryTickets { get; set; }
	public DbSet<InventoryTicketDetail> InventoryTicketDetails { get; set; }
	public DbSet<InventoryBalance> InventoryBalances { get; set; }
	public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
	public DbSet<InventoryReservation> InventoryReservations { get; set; }

	//Order
	public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
	public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
	public DbSet<SalesOrder> SalesOrders { get; set; } 
	public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
	#endregion

	public SupplyCoreERPDbContext(DbContextOptions<SupplyCoreERPDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureBlobStoring();

		/* Configure your own tables/entities inside here */

		//builder.Entity<YourEntity>(b =>
		//{
		//    b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "YourEntities", SupplyCoreERPConsts.DbSchema);
		//    b.ConfigureByConvention(); //auto configure for the base class props
		//    //...
		//});

		// Location
		builder.Entity<Continent>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Continents", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();
		});

		builder.Entity<Country>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Countries", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();
			b.HasIndex(x => x.ISO).IsUnique();

			b.HasOne(x => x.Continent)
			 .WithMany()
			 .HasForeignKey(x => x.ContinentId)
			 .OnDelete(DeleteBehavior.Restrict); //Chặn xóa Châu lục nếu còn Quốc gia
		});

		builder.Entity<City>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Cities", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.HasOne(x => x.Country)
			 .WithMany()
			 .HasForeignKey(x => x.CountryId)
			 .OnDelete(DeleteBehavior.Restrict); //Chặn xóa Quốc gia nếu còn Thành phố
		});

		builder.Entity<Area>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Areas", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.HasOne(x => x.City)
			 .WithMany()
			 .HasForeignKey(x => x.CityId)
			 .OnDelete(DeleteBehavior.Restrict); //Chặn xóa Thành phố nếu còn Quận/Huyện
		});

		// Category
		builder.Entity<Category>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Categories", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();
			b.HasIndex(x => x.Name).IsUnique(); // Tên nhóm hàng không trùng
		});

		// Base Unit
		builder.Entity<BaseUnit>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "BaseUnits", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();
			b.HasIndex(x => x.Code).IsUnique(); // Mã đơn vị duy nhất
		});

		// Manufacturer
		builder.Entity<Manufacturer>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Manufacturers", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			// Mapping Location (Restrict: Xóa location không được xóa Manufacturer)
			b.HasOne(x => x.Continent).WithMany().HasForeignKey(x => x.ContinentId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
		});

		// ActiveIngredient
		builder.Entity<ActiveIngredient>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "ActiveIngredients", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();
			b.HasIndex(x => x.Code).IsUnique();
		});

		// Product
		builder.Entity<Product>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Products", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.HasIndex(x => x.Code).IsUnique(); 

			// Mapping khóa ngoại 
			b.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Manufacturer).WithMany().HasForeignKey(x => x.ManufacturerId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.BaseUnit).WithMany().HasForeignKey(x => x.BaseUnitId).OnDelete(DeleteBehavior.Restrict);

			// Cascade: Xóa Product -> Xóa luôn ProductUnits
			b.HasMany(x => x.Units).WithOne(x => x.Product).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
		});

		// Product Unit
		builder.Entity<ProductUnit>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "ProductUnits", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();
			// Link tới BaseUnit (Danh mục) -> RESTRICT
			b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
		});

		// Medicine
		builder.Entity<Medicine>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Medicines", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			// Link DosageForm -> RESTRICT
			b.HasOne(x => x.DosageForm).WithMany().HasForeignKey(x => x.DosageFormId).OnDelete(DeleteBehavior.Restrict);

			// Cascade: Xóa Medicine -> Xóa luôn Ingredients
			b.HasMany(x => x.Ingredients).WithOne().HasForeignKey(x => x.MedicineId).OnDelete(DeleteBehavior.Cascade);
		});

		//MedicineIngredient
		builder.Entity<MedicineIngredient>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "MedicineIngredients", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			// Link ActiveIngredient (Danh mục) -> RESTRICT
			b.HasOne(x => x.ActiveIngredient).WithMany().HasForeignKey(x => x.ActiveIngredientId).OnDelete(DeleteBehavior.Restrict);
		});

		// Dosage Form
		builder.Entity<DosageForm>(b => {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "DosageForms", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();
			b.HasIndex(x => x.Code).IsUnique();
		});

		// PriceList
		builder.Entity<PriceList>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "PriceLists", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention(); 

			b.Property(x => x.Code).IsRequired().HasMaxLength(20);
			b.HasIndex(x => x.Code).IsUnique();

			b.Property(x => x.Name).IsRequired().HasMaxLength(100);

			b.HasIndex(x => x.IsBase);
		});
		// ProductPrice
		builder.Entity<ProductPrice>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "ProductPrices", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();

			//Link tới Bảng giá (Xóa Bảng giá -> Xóa luôn chi tiết giá)
			b.HasOne(x => x.PriceList)
			 .WithMany()
			 .HasForeignKey(x => x.PriceListId)
			 .OnDelete(DeleteBehavior.Cascade);

			//Link tới Đơn vị tính (BaseUnit)
			b.HasOne(x => x.Unit)
			 .WithMany()
			 .HasForeignKey(x => x.UnitId)
			 .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Unit nếu đang có giá gán vào

			// 3. Link tới Sản phẩm (Product)
			b.HasOne(x => x.Product)
			 .WithMany()
			 .HasForeignKey(x => x.ProductId)
			 .OnDelete(DeleteBehavior.Cascade);// Xóa sản phẩm - xóa hết giá

			// Trong 1 Bảng giá, 1 Sản phẩm, 1 Đơn vị, 1 Mức số lượng -> Chỉ có 1 dòng giá.
			b.HasIndex(x => new { x.PriceListId, x.ProductId, x.UnitId, x.MinQuantity })
			 .IsUnique();
		});

		// Supplier
		builder.Entity<Supplier>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Suppliers", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.CountryId).IsRequired(false);
			b.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).IsRequired(false);
			b.HasOne(x => x.Area).WithMany().HasForeignKey(x => x.AreaId).IsRequired(false);

			b.HasIndex(x => x.Code).IsUnique();
		});

		// Customer
		builder.Entity<Customer>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Customers", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.CountryId).IsRequired(false);
			b.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).IsRequired(false);
			b.HasOne(x => x.Area).WithMany().HasForeignKey(x => x.AreaId).IsRequired(false);

			b.HasIndex(x => x.Code).IsUnique();
			b.HasIndex(x => x.PhoneNumber).IsUnique().HasFilter("\"PhoneNumber\" IS NOT NULL AND \"PhoneNumber\" != ''");
			// HasFilter để cho phép nhiều dòng null, chỉ bắt trùng với những dòng có dữ liệu
		});

		// Warehouse
		builder.Entity<Warehouse>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Warehouses", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention(); 

			b.Property(x => x.Code).IsRequired().HasMaxLength(50);
			b.HasIndex(x => x.Code).IsUnique(); // Ràng buộc Unique cấp độ Database

			b.Property(x => x.Name).IsRequired().HasMaxLength(255);
			b.Property(x => x.Address).HasMaxLength(500);

			// Quan hệ với Tỉnh/Thành & Quận/Huyện (Không Cascade)
			b.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Area).WithMany().HasForeignKey(x => x.AreaId).OnDelete(DeleteBehavior.Restrict);
		});

		// Zone
		builder.Entity<Zone>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Zones", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.Code).IsRequired().HasMaxLength(50);
			b.Property(x => x.Name).IsRequired().HasMaxLength(255);
			b.Property(x => x.Color).HasMaxLength(20);

			// Một kho không được có 2 Zone trùng mã
			b.HasIndex(x => new { x.WarehouseId, x.Code }).IsUnique();

			// Xóa Kho -> Xóa luôn Zone
			b.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Cascade);
		});

		// StorageLocation
		

		// ProductBatch
		builder.Entity<ProductBatch>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "ProductBatches", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.BatchNumber).IsRequired().HasMaxLength(100);

			// 1 Lô thuốc chỉ thuộc về 1 Sản phẩm (Product)
			b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);

			// Mua của NCC nào
			b.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);

			// Index hỗ trợ tìm kiếm FEFO nhanh hơn
			b.HasIndex(x => new { x.ProductId, x.Status, x.ExpiryDate });
		});

		// Bin
		builder.Entity<Bin>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Bins", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.Code).IsRequired().HasMaxLength(50);

			// Unique Code trong 1 kho
			b.HasIndex(x => new { x.WarehouseId, x.Code }).IsUnique();

			// Quan hệ với Zone: Xóa Zone thì KHÔNG được xóa Bin (để an toàn dữ liệu) -> Restrict
			b.HasOne(x => x.Zone).WithMany().HasForeignKey(x => x.ZoneId).OnDelete(DeleteBehavior.Restrict);

			// Quan hệ với Warehouse: Xóa Kho thì xóa Bin
			b.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Cascade);
		});

		// InventoryTicket
		builder.Entity<InventoryTicket>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "InventoryTickets", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.TicketNumber).IsRequired().HasMaxLength(50);
			b.HasIndex(x => x.TicketNumber).IsUnique();

			b.Property(x => x.Note).HasMaxLength(1000);

			b.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);

			// Index để sau này JOIN lấy Phiếu từ Order (PurchaseOrder/SalesOrder) cho nhanh
			b.HasIndex(x => x.ReferenceDocumentId);
		});

		// InventoryTicketDetail
		builder.Entity<InventoryTicketDetail>(b =>
        {
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "InventoryTicketDetails", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

            b.Property(x => x.Quantity).HasColumnType("decimal(18, 2)");

            b.HasOne(x => x.Ticket).WithMany(x => x.Details).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ProductBatch).WithMany().HasForeignKey(x => x.ProductBatchId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Bin).WithMany().HasForeignKey(x => x.BinId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).IsRequired().OnDelete(DeleteBehavior.NoAction);
			b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.NoAction);
		});

		// InventoryBalance
		builder.Entity<InventoryBalance>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "InventoryBalances", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.Quantity).HasColumnType("decimal(18, 2)");
			b.Property(x => x.LockedQuantity).HasColumnType("decimal(18, 2)");

			b.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.ProductBatch).WithMany().HasForeignKey(x => x.ProductBatchId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Bin).WithMany().HasForeignKey(x => x.BinId).OnDelete(DeleteBehavior.Restrict);

			// Index Unique: Kho - Bin - Sản phẩm - Lô
			b.HasIndex(x => new { x.WarehouseId, x.BinId, x.ProductId, x.ProductBatchId }).IsUnique();
		});

		// InventoryTransaction
		builder.Entity<InventoryTransaction>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "InventoryTransactions", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.QuantityChanged).HasColumnType("decimal(18, 2)");
			b.Property(x => x.BalanceAfterTransaction).HasColumnType("decimal(18, 2)");
			b.Property(x => x.Note).HasMaxLength(1000);

			b.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.ProductBatch).WithMany().HasForeignKey(x => x.ProductBatchId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Bin).WithMany().HasForeignKey(x => x.BinId).OnDelete(DeleteBehavior.Restrict);

			b.HasIndex(x => new { x.WarehouseId, x.ProductId, x.CreationTime });
		});
		// InventoryReservation
		builder.Entity<InventoryReservation>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "InventoryReservations", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.ReferenceDocumentNumber).HasMaxLength(50);
			b.Property(x => x.ReservedQuantity).HasColumnType("decimal(18, 4)");

			b.HasIndex(x => new { x.ReferenceDocumentId, x.Status });
		});

		// PurchaseOrder
		builder.Entity<PurchaseOrder>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "PurchaseOrders", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			// Ràng buộc thuộc tính
			b.Property(x => x.Code).IsRequired().HasMaxLength(50);
			b.Property(x => x.Note).HasMaxLength(1000);

			b.Property(x => x.SubTotal).HasPrecision(18, 4);
			b.Property(x => x.TaxAmount).HasPrecision(18, 4);
			b.Property(x => x.TotalAmount).HasPrecision(18, 4);

			b.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).IsRequired();

			b.HasMany(x => x.Details)
			 .WithOne(x => x.PurchaseOrder)
			 .HasForeignKey(x => x.PurchaseOrderId)
			 .IsRequired()
			 .OnDelete(DeleteBehavior.Cascade);
		});

		// PurchaseOrderDetail
		builder.Entity<PurchaseOrderDetail>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "PurchaseOrderDetails", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.Quantity).HasPrecision(18, 4);
			b.Property(x => x.UnitPrice).HasPrecision(18, 4);
			b.Property(x => x.TaxRate).HasPrecision(5, 2); // % thuế (VD: 99.99)
			b.Property(x => x.ReceivedQuantity).HasPrecision(18, 4);

			b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).IsRequired().OnDelete(DeleteBehavior.Restrict);
		});

		// SalesOrder
		builder.Entity<SalesOrder>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "SalesOrders", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.Code).IsRequired().HasMaxLength(50);
			b.HasIndex(x => x.Code).IsUnique(); 

			b.Property(x => x.Note).HasMaxLength(1000);

			b.Property(x => x.SubTotal).HasPrecision(18, 4);
			b.Property(x => x.DiscountAmount).HasPrecision(18, 4);
			b.Property(x => x.TaxAmount).HasPrecision(18, 4);
			b.Property(x => x.TotalAmount).HasPrecision(18, 4);

			b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).IsRequired().OnDelete(DeleteBehavior.Restrict);

			b.HasMany(x => x.Details)
			 .WithOne(x => x.SalesOrder)
			 .HasForeignKey(x => x.SalesOrderId)
			 .IsRequired()
			 .OnDelete(DeleteBehavior.Cascade);
		});

		// SalesOrderDetail
		builder.Entity<SalesOrderDetail>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "SalesOrderDetails", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.Quantity).HasPrecision(18, 4);
			b.Property(x => x.UnitPrice).HasPrecision(18, 4);
			b.Property(x => x.DiscountRate).HasPrecision(5, 2); // Khống chế tỷ lệ phần trăm (VD: 99.99)
			b.Property(x => x.TaxRate).HasPrecision(5, 2);
			b.Property(x => x.DeliveredQuantity).HasPrecision(18, 4);

			b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).IsRequired().OnDelete(DeleteBehavior.Restrict);
		});
	}
}

