using SupplyCoreERP.Categories;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Categories
{
    public class CategoryTestDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Category, Guid> _categoryRepository;

        public CategoryTestDataSeedContributor(IRepository<Category, Guid> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (await _categoryRepository.GetCountAsync() > 0)
            {
                return;
            }

            await _categoryRepository.InsertAsync(
                new Category(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Electronics"),
                autoSave: true
            );

            await _categoryRepository.InsertAsync(
                new Category(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Software"),
                autoSave: true
            );
        }
    }
}
