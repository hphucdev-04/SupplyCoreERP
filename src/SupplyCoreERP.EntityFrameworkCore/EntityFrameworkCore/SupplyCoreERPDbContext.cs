using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Catalog.ActiveIngredients;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Categories;
using SupplyCoreERP.Catalog.DosageForms;
using SupplyCoreERP.Catalog.Manufacturers;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Common.Notifications;
using SupplyCoreERP.Inventory.Balances;
using SupplyCoreERP.Inventory.Batches;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Inventory.Transactions;
using SupplyCoreERP.Inventory.Warehouses;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Partner.Customers;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseRequisitions;
using SupplyCoreERP.Procurement.PurchaseReturnRequests;
using SupplyCoreERP.Procurement.PurchaseReturns;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.Sales.PriceLists;
using SupplyCoreERP.Sales.SalesRecalls;
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
    public DbSet<MedicineRegistration> MedicineRegistrations { get; set; }

    // Price
    public DbSet<PriceList> PriceLists { get; set; }
    public DbSet<ProductPrice> ProductPrices { get; set; }

    // Partner
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<SupplierProduct> SupplierProducts { get; set; }
    public DbSet<SupplierProductCondition> SupplierProductConditions { get; set; }

    // Warehouse & Inventory
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Zone> Zones { get; set; }
    public DbSet<Bin> Bins { get; set; }
    public DbSet<ProductBatch> ProductBatches { get; set; }
    public DbSet<InventoryTicket> InventoryTickets { get; set; }
    public DbSet<InventoryTicketLine> InventoryTicketLines { get; set; }
    public DbSet<InventoryTicketDetail> InventoryTicketDetails { get; set; }
    public DbSet<InventoryBalance> InventoryBalances { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<InventoryReservation> InventoryReservations { get; set; }

    // Orders
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
    public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
    public DbSet<PurchaseRequisitionLine> PurchaseRequisitionLines { get; set; }

    // Purchase Returns & Sales Recalls
    public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
    public DbSet<PurchaseReturnLine> PurchaseReturnLines { get; set; }
    public DbSet<PurchaseReturnRequest> PurchaseReturnRequests { get; set; }
    public DbSet<PurchaseReturnRequestLine> PurchaseReturnRequestLines { get; set; }
    public DbSet<SalesRecall> SalesRecalls { get; set; }
    public DbSet<SalesRecallLine> SalesRecallLines { get; set; }


    // Document Sequence
    public DbSet<DocumentSequence> DocumentSequences { get; set; }

    // Notification
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserNotification> UserNotifications { get; set; }

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
        builder.Entity<Continent>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Continents", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<Country>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Countries", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.ISO).IsUnique();

            b.HasOne(x => x.Continent)
             .WithMany()
             .HasForeignKey(x => x.ContinentId)
             .OnDelete(DeleteBehavior.Restrict); //Chặn xóa Châu lục nếu còn Quốc gia
        });

        builder.Entity<City>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Cities", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasOne(x => x.Country)
             .WithMany()
             .HasForeignKey(x => x.CountryId)
             .OnDelete(DeleteBehavior.Restrict); //Chặn xóa Quốc gia nếu còn Thành phố
        });

        builder.Entity<Area>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Areas", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasOne(x => x.City)
             .WithMany()
             .HasForeignKey(x => x.CityId)
             .OnDelete(DeleteBehavior.Restrict); //Chặn xóa Thành phố nếu còn Quận/Huyện
        });

        // Category
        builder.Entity<Category>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Categories", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.Name).IsUnique(); // Tên nhóm hàng không trùng
        });

        // Base Unit
        builder.Entity<BaseUnit>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "BaseUnits", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.Code).IsUnique(); // Mã đơn vị duy nhất
        });

        // Manufacturer
        builder.Entity<Manufacturer>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Manufacturers", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            // Mapping Location (Restrict: Xóa location không được xóa Manufacturer)
            b.HasOne(x => x.Continent).WithMany().HasForeignKey(x => x.ContinentId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.Code).IsUnique();
        });

        // ActiveIngredient
        builder.Entity<ActiveIngredient>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "ActiveIngredients", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.Code).IsUnique();
        });

        // Product
        builder.Entity<Product>(b =>
        {
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
        builder.Entity<ProductUnit>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "ProductUnits", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();
            // Link tới BaseUnit (Danh mục) -> RESTRICT
            b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        });

        // Medicine
        builder.Entity<Medicine>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Medicines", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            // Link DosageForm -> RESTRICT
            b.HasOne(x => x.DosageForm).WithMany().HasForeignKey(x => x.DosageFormId).OnDelete(DeleteBehavior.Restrict);

            // Cascade: Xóa Medicine -> Xóa luôn Ingredients và Registrations
            b.HasMany(x => x.Ingredients).WithOne().HasForeignKey(x => x.MedicineId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Registrations).WithOne().HasForeignKey(x => x.MedicineId).OnDelete(DeleteBehavior.Cascade);
        });

        // MedicineRegistration
        builder.Entity<MedicineRegistration>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "MedicineRegistrations", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.RegistrationNumber).IsRequired().HasMaxLength(100);
            b.HasIndex(x => new { x.MedicineId, x.RegistrationNumber });
            b.HasIndex(x => x.IsActive);
        });

        //MedicineIngredient
        builder.Entity<MedicineIngredient>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "MedicineIngredients", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            // Link ActiveIngredient (Danh mục) -> RESTRICT
            b.HasOne(x => x.ActiveIngredient).WithMany().HasForeignKey(x => x.ActiveIngredientId).OnDelete(DeleteBehavior.Restrict);
        });

        // Dosage Form
        builder.Entity<DosageForm>(b =>
        {
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
        // Supplier Product
        builder.Entity<SupplierProduct>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "SupplierProducts", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasKey(x => x.Id);

            b.HasOne(x => x.Supplier)
             .WithMany(x => x.SupplierProducts)
             .HasForeignKey(x => x.SupplierId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Product)
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.DefaultUnit)
             .WithMany()
             .HasForeignKey(x => x.DefaultUnitId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.SupplierId, x.ProductId }).IsUnique();

            // Cấu hình quan hệ 1-N với Conditions
            b.HasMany(x => x.Conditions)
             .WithOne(x => x.SupplierProduct)
             .HasForeignKey(x => x.SupplierProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Supplier Product Condition
        builder.Entity<SupplierProductCondition>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "SupplierProductConditions", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasKey(x => x.Id);

            b.HasOne(x => x.Unit)
             .WithMany()
             .HasForeignKey(x => x.UnitId)
             .OnDelete(DeleteBehavior.Restrict);

            // UNIQUE INDEX MỚI: Cho phép trùng UnitId nhưng cấm trùng MinOrderQuantity
            b.HasIndex(x => new { x.SupplierProductId, x.UnitId, x.MinOrderQuantity }).IsUnique();
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

            // Liên kết Số đăng ký (cho Thuốc)
            b.HasOne(x => x.MedicineRegistration).WithMany().HasForeignKey(x => x.MedicineRegistrationId).OnDelete(DeleteBehavior.Restrict);

            // Index hỗ trợ tìm kiếm FEFO nhanh hơn
            b.HasIndex(x => new { x.ProductId, x.Status, x.ExpiryDate });
            b.HasIndex(x => x.Code).IsUnique();
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

        // InventoryTicketLine
        builder.Entity<InventoryTicketLine>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "InventoryTicketLines", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Quantity).HasColumnType("decimal(18, 2)");

            b.HasOne(x => x.Ticket).WithMany(x => x.Lines).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);

            // Index cho ReferenceDocumentLineId để tối ưu hóa truy vấn tìm kiếm dòng liên kết đơn hàng
            b.HasIndex(x => x.ReferenceDocumentLineId);
        });

        // InventoryTicketDetail
        builder.Entity<InventoryTicketDetail>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "InventoryTicketDetails", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Quantity).HasColumnType("decimal(18, 2)");

            b.HasOne(x => x.TicketLine).WithMany(x => x.Details).HasForeignKey(x => x.TicketLineId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ProductBatch).WithMany().HasForeignKey(x => x.ProductBatchId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Bin).WithMany().HasForeignKey(x => x.BinId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).IsRequired().OnDelete(DeleteBehavior.NoAction);
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
            b.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.PurchaseRequisition).WithMany().HasForeignKey(x => x.PurchaseRequisitionId).OnDelete(DeleteBehavior.SetNull);

            b.HasMany(x => x.Lines)
             .WithOne(x => x.PurchaseOrder)
             .HasForeignKey(x => x.PurchaseOrderId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
        });

        // PurchaseOrderLine
        builder.Entity<PurchaseOrderLine>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "PurchaseOrderLines", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.UnitPrice).HasPrecision(18, 4);
            b.Property(x => x.TaxRate).HasPrecision(5, 2); // % thuế (VD: 99.99)
            b.Property(x => x.ReceivedQuantity).HasPrecision(18, 4);

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        });

        // PurchaseRequisition
        builder.Entity<PurchaseRequisition>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "PurchaseRequisitions", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.Note).HasMaxLength(1000);

            b.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).IsRequired().OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Lines)
             .WithOne(x => x.PurchaseRequisition)
             .HasForeignKey(x => x.PurchaseRequisitionId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
        });

        // PurchaseRequisitionLine
        builder.Entity<PurchaseRequisitionLine>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "PurchaseRequisitionLines", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.OrderedQuantity).HasPrecision(18, 4);
            b.Property(x => x.Note).HasMaxLength(500);

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

            b.HasMany(x => x.Lines)
             .WithOne(x => x.SalesOrder)
             .HasForeignKey(x => x.SalesOrderId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
        });

        // SalesOrderLine
        builder.Entity<SalesOrderLine>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "SalesOrderLines", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.UnitPrice).HasPrecision(18, 4);
            b.Property(x => x.DiscountRate).HasPrecision(5, 2); // Khống chế tỷ lệ phần trăm (VD: 99.99)
            b.Property(x => x.TaxRate).HasPrecision(5, 2);
            b.Property(x => x.DeliveredQuantity).HasPrecision(18, 4);

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        });

        // PurchaseReturn
        builder.Entity<PurchaseReturn>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "PurchaseReturns", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.Note).HasMaxLength(1000);

            b.Property(x => x.SubTotal).HasPrecision(18, 4);
            b.Property(x => x.TaxAmount).HasPrecision(18, 4);
            b.Property(x => x.TotalAmount).HasPrecision(18, 4);

            b.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.PurchaseReturnRequest).WithMany().HasForeignKey(x => x.PurchaseReturnRequestId).OnDelete(DeleteBehavior.SetNull);

            b.HasMany(x => x.Lines)
             .WithOne(x => x.PurchaseReturn)
             .HasForeignKey(x => x.PurchaseReturnId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
        });

        // PurchaseReturnLine
        builder.Entity<PurchaseReturnLine>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "PurchaseReturnLines", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.OriginalUnitPrice).HasPrecision(18, 4);
            b.Property(x => x.DepreciationRate).HasPrecision(5, 2);
            b.Property(x => x.TaxRate).HasPrecision(5, 2);

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        });

        // PurchaseReturnRequest
        builder.Entity<PurchaseReturnRequest>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "PurchaseReturnRequests", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.Note).HasMaxLength(1000);

            b.Property(x => x.SubTotal).HasPrecision(18, 4);
            b.Property(x => x.TaxAmount).HasPrecision(18, 4);
            b.Property(x => x.TotalAmount).HasPrecision(18, 4);

            b.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).IsRequired().OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Lines)
             .WithOne(x => x.PurchaseReturnRequest)
             .HasForeignKey(x => x.PurchaseReturnRequestId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
        });

        // PurchaseReturnRequestLine
        builder.Entity<PurchaseReturnRequestLine>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "PurchaseReturnRequestLines", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.BaseQuantity).HasPrecision(18, 4);
            b.Property(x => x.OriginalUnitPrice).HasPrecision(18, 4);
            b.Property(x => x.ReturnUnitPrice).HasPrecision(18, 4);
            b.Property(x => x.DepreciationRate).HasPrecision(5, 2);
            b.Property(x => x.TaxRate).HasPrecision(5, 2);
            b.Property(x => x.TotalPrice).HasPrecision(18, 4);
            b.Property(x => x.TaxAmount).HasPrecision(18, 4);
            b.Property(x => x.FinalPrice).HasPrecision(18, 4);

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne<PurchaseOrder>().WithMany().HasForeignKey(x => x.PurchaseOrderId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne<PurchaseOrderLine>().WithMany().HasForeignKey(x => x.PurchaseOrderLineId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        });

        // SalesRecall

        builder.Entity<SalesRecall>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "SalesRecalls", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.RecallDecisionNumber).IsRequired().HasMaxLength(256);
            b.Property(x => x.Note).HasMaxLength(1000);

            b.Property(x => x.TotalAmount).HasPrecision(18, 4);
            b.Property(x => x.Deadline).IsRequired();

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ProductBatch).WithMany().HasForeignKey(x => x.ProductBatchId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).IsRequired().OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Lines)
             .WithOne(x => x.SalesRecall)
             .HasForeignKey(x => x.SalesRecallId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
        });

        // SalesRecallLine
        builder.Entity<SalesRecallLine>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "SalesRecallLines", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.OriginalUnitPrice).HasPrecision(18, 4);
            b.Property(x => x.TaxRate).HasPrecision(5, 2);

            b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.SalesOrder).WithMany().HasForeignKey(x => x.SalesOrderId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        });

        // DocumentSeuqence
        builder.Entity<DocumentSequence>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "DocumentSequences", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            // Đảm bảo DocumentType là duy nhất để tránh tạo trùng loại chứng từ
            b.HasIndex(x => x.DocumentType).IsUnique();

            b.Property(x => x.DocumentType).IsRequired().HasMaxLength(10);
            b.Property(x => x.PrefixDate).IsRequired().HasMaxLength(6);
            b.Property(x => x.LastValue).IsRequired();
        });

        // Notification
        builder.Entity<Notification>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Notifications", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Title).HasMaxLength(255).IsRequired();
            b.Property(x => x.Content).HasMaxLength(2048).IsRequired();
        });

        // UserNotification
        builder.Entity<UserNotification>(b =>
        {
            b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "UserNotifications", SupplyCoreERPConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(x => new { x.UserId, x.NotificationId });
        });
    }
}
