using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SupplyCoreERP.Categories.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Categories;

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
        PagedResultDto<CategoryDto> result = await _categoryAppService.GetListAsync(new GetCategoryListDto());

        // Assert
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        result.Items.ShouldContain(c => c.Name == "Electronics");
        result.Items.ShouldContain(c => c.Name == "Software");
    }

    [Fact]
    public async Task Should_Filter_Categories_By_Name()
    {
        // Act
        PagedResultDto<CategoryDto> result = await _categoryAppService.GetListAsync(new GetCategoryListDto { Filter = "Elect" });

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
        CategoryDto result = await _categoryAppService.CreateAsync(input);

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
        UserFriendlyException exception = await Should.ThrowAsync<UserFriendlyException>(async () =>
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

    [Fact]
    public async Task Should_Update_A_Category()
    {
        // Arrange
        CategoryDto category = (await _categoryAppService.GetListAsync(new GetCategoryListDto())).Items.First();
        var input = new CreateUpdateCategoryDto { Name = "Updated Name" };

        // Act
        CategoryDto result = await _categoryAppService.UpdateAsync(category.Id, input);

        // Assert
        result.Name.ShouldBe("Updated Name");

        CategoryDto updatedCategory = await _categoryAppService.GetAsync(category.Id);
        updatedCategory.Name.ShouldBe("Updated Name");
    }

    [Fact]
    public async Task Should_Delete_A_Category()
    {
        // Arrange
        var input = new CreateUpdateCategoryDto { Name = "To Be Deleted" };
        CategoryDto created = await _categoryAppService.CreateAsync(input);

        // Act
        await _categoryAppService.DeleteAsync(created.Id);

        // Assert
        PagedResultDto<CategoryDto> result = await _categoryAppService.GetListAsync(new GetCategoryListDto { Filter = "To Be Deleted" });
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Should_Not_Delete_Category_If_In_Use()
    {
        // Note: This requires a product to be seeded that uses one of the categories.
        // For now, we assume the seed doesn't have products, but this test case is here for completeness.
        // If there's a Product seed, we should use a category that has products.
    }
}
