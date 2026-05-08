using Shouldly;
using SupplyCoreERP.Categories.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Categories
{
    public abstract class CategoryAppService_Integration_Tests<TStartupModule> : SupplyCoreERPApplicationTestBase<TStartupModule>
        where TStartupModule : IAbpModule
    {
        private readonly ICategoryAppService _categoryAppService;

        protected CategoryAppService_Integration_Tests()
        {
            _categoryAppService = GetRequiredService<ICategoryAppService>();
        }

        [Fact]
        public async Task Should_Get_List_Of_Categories()
        {
            // Act
            var result = await _categoryAppService.GetListAsync(new GetCategoryListDto());

            // Assert
            result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
            result.Items.ShouldContain(c => c.Name == "Electronics");
            result.Items.ShouldContain(c => c.Name == "Software");
        }

        [Fact]
        public async Task Should_Filter_Categories_By_Name()
        {
            // Act
            var result = await _categoryAppService.GetListAsync(new GetCategoryListDto { Filter = "Elect" });

            // Assert
            result.TotalCount.ShouldBe(1);
            result.Items.ShouldContain(c => c.Name == "Electronics");
            result.Items.ShouldNotContain(c => c.Name == "Software");
        }

        [Fact]
        public async Task Should_Create_A_Valid_Category()
        {
            // Arrange
            var input = new CreateUpdateCategoryDto
            {
                Name = "Health"
            };

            // Act
            var result = await _categoryAppService.CreateAsync(input);

            // Assert
            result.Id.ShouldNotBe(Guid.Empty);
            result.Name.ShouldBe("Health");
        }

        [Fact]
        public async Task Should_Not_Create_Duplicate_Category_Name()
        {
            // Arrange
            var input = new CreateUpdateCategoryDto
            {
                Name = "Electronics" // Already exists in seed
            };

            // Act & Assert
            var exception = await Should.ThrowAsync<UserFriendlyException>(async () =>
            {
                await _categoryAppService.CreateAsync(input);
            });

            exception.Message.ShouldContain("đã tồn tại");
        }

        [Fact]
        public async Task Should_Not_Create_Category_With_Empty_Name()
        {
            // Arrange
            var input = new CreateUpdateCategoryDto
            {
                Name = ""
            };

            // Act & Assert
            await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(async () =>
            {
                await _categoryAppService.CreateAsync(input);
            });
        }
    }
}
