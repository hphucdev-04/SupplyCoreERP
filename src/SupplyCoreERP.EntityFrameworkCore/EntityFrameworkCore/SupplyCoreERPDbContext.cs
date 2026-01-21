using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.ActiveIngredients;
using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Categories;
using SupplyCoreERP.DosageForms;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.MasterData;
using SupplyCoreERP.Medicines;
using SupplyCoreERP.Products;
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

	// Master Data
	public DbSet<Category> Categories { get; set; }
	public DbSet<BaseUnit> BaseUnits { get; set; }
	// Dosage Form
	public DbSet<DosageForm> DosageForms { get; set; }
	// Active Ingredient
	public DbSet<ActiveIngredient> ActiveIngredients { get; set; }

	// Manufacturer
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
			b.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Area).WithMany().HasForeignKey(x => x.AreaId).OnDelete(DeleteBehavior.Restrict);
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
	}
}
