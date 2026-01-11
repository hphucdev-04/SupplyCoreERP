using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Categories;
using SupplyCoreERP.Medicines;
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

	// Category
	public DbSet<Category> Categories { get; set; }
	// Medicine
	public DbSet<Medicine> Medicines { get; set; }
	public DbSet<MedicineUnit> MedicineUnits { get; set; }

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

		//Bảng AppCategories
		builder.Entity<Category>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Categories", SupplyCoreERPConsts.DbSchema);

			b.ConfigureByConvention(); 

			b.Property(x => x.Code).IsRequired().HasMaxLength(50);
			b.Property(x => x.Name).IsRequired().HasMaxLength(255);

			b.HasIndex(x => x.Code).IsUnique();
		});

		//Bảng AppMedicines
		builder.Entity<Medicine>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "Medicines", SupplyCoreERPConsts.DbSchema);
			b.ConfigureByConvention();

			b.Property(x => x.Code).IsRequired().HasMaxLength(50);
			b.Property(x => x.Name).IsRequired().HasMaxLength(255);
			b.Property(x => x.BaseUnit).IsRequired().HasMaxLength(50);
			b.HasIndex(x => x.Code).IsUnique();

			b.HasOne(x => x.Category)      
			 .WithMany()                   
			 .HasForeignKey(x => x.CategoryId) 
			 .IsRequired()
			 .OnDelete(DeleteBehavior.Restrict); // Khi có thuốc không đươc xóa category

			b.HasMany(x => x.Units)
			 .WithOne(x => x.Medicine)
			 .HasForeignKey(x => x.MedicineId)
			 .IsRequired()
			 .OnDelete(DeleteBehavior.Cascade); // Xóa Thuốc thì xóa luôn unit
		});

		//Bảng AppMedicineUnits
		builder.Entity<MedicineUnit>(b =>
		{
			b.ToTable(SupplyCoreERPConsts.DbTablePrefix + "MedicineUnits", SupplyCoreERPConsts.DbSchema);

			b.ConfigureByConvention();

			b.Property(x => x.UnitName).IsRequired().HasMaxLength(50);

			b.HasIndex(x => x.MedicineId);
		});
	}
}
