using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace SupplyCoreERP.EntityFrameworkCore;

[DependsOn(
    typeof(SupplyCoreERPApplicationTestModule),
    typeof(SupplyCoreERPEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class SupplyCoreERPEntityFrameworkCoreTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeatureManagementOptions>(options =>
        {
            options.SaveStaticFeaturesToDatabase = false;
            options.IsDynamicFeatureStoreEnabled = false;
        });
        Configure<PermissionManagementOptions>(options =>
        {
            options.SaveStaticPermissionsToDatabase = false;
            options.IsDynamicPermissionStoreEnabled = false;
        });
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        ConfigureInMemorySqlite(context.Services);
    }

    private static void ConfigureInMemorySqlite(IServiceCollection services)
    {
        // Mỗi lần DI container resolve DbContext sẽ tạo connection mới
        // Dùng tên unique per-registration thay vì shared connection field
        string connectionString = $"Data Source=SupplyCoreERP_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

        var connection = new SqliteConnection(connectionString);
        connection.Open();

        DbContextOptions<SupplyCoreERPDbContext> options =
            new DbContextOptionsBuilder<SupplyCoreERPDbContext>()
                .UseSqlite(connection)
                .Options;

        using (var dbContext = new SupplyCoreERPDbContext(options))
        {
            dbContext.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        // Register connection như singleton trong DI
        // Mỗi test class tạo DI scope riêng → connection riêng
        services.AddSingleton(connection);

        services.Configure<AbpDbContextOptions>(opts =>
        {
            opts.Configure(ctx =>
            {
                ctx.DbContextOptions.UseSqlite(connection);
            });
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        // Dispose connection từ DI thay vì field
        var connection = context.ServiceProvider
            .GetService<SqliteConnection>();
        connection?.Dispose();
    }
}
