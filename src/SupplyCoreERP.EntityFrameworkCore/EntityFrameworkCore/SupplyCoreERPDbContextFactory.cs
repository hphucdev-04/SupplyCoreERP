using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SupplyCoreERP.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class SupplyCoreERPDbContextFactory : IDesignTimeDbContextFactory<SupplyCoreERPDbContext>
{
    public SupplyCoreERPDbContext CreateDbContext(string[] args)
    {
        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        IConfigurationRoot configuration = BuildConfiguration();

        SupplyCoreERPEfCoreEntityExtensionMappings.Configure();

        DbContextOptionsBuilder<SupplyCoreERPDbContext> builder = new DbContextOptionsBuilder<SupplyCoreERPDbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));

        return new SupplyCoreERPDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SupplyCoreERP.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
