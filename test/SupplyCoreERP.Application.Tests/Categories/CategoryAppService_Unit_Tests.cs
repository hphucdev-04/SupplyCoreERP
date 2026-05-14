using System;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SupplyCoreERP.Categories.Dtos;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace SupplyCoreERP.Categories;

public class CategoryAppService_Unit_Tests
{
    private readonly ICategoryRepository _mockCategoryRepo;
    private readonly ICategoryManager _mockCategoryManager;
    private readonly IObjectMapper _mockObjectMapper;
    private readonly CategoryAppService _categoryAppService;

    public CategoryAppService_Unit_Tests()
    {
        // --- ARRANGE (Mocking dependencies) ---
        _mockCategoryRepo = Substitute.For<ICategoryRepository>();
        _mockCategoryManager = Substitute.For<ICategoryManager>();
        _mockObjectMapper = Substitute.For<IObjectMapper>();

        // Khởi tạo Service bằng bản Mock thông qua Constructor
        _categoryAppService = new CategoryAppService(
            _mockCategoryRepo,
            _mockCategoryManager,
            _mockObjectMapper);
    }

    [Fact]
    public async Task CreateAsync_Should_Call_Manager_And_Repository()
    {
        // Arrange
        CreateUpdateCategoryDto input = new() { Name = "New Category" };
        Category category = new(Guid.NewGuid(), input.Name);
        CategoryDto categoryDto = new() { Id = category.Id, Name = category.Name };

        _mockCategoryManager.CreateAsync(input.Name).Returns(Task.FromResult(category));
        _mockObjectMapper.Map<Category, CategoryDto>(category).Returns(categoryDto);

        // Act
        CategoryDto result = await _categoryAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(input.Name);

        // Kiểm chứng sự phối hợp giữa các thành phần (Orchestration)
        await _mockCategoryManager.Received(1).CreateAsync(input.Name);
        await _mockCategoryRepo.Received(1).InsertAsync(category, autoSave: true);
        _mockObjectMapper.Received(1).Map<Category, CategoryDto>(category);
    }

    [Fact]
    public async Task UpdateAsync_Should_Call_Manager_And_Repository()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        CreateUpdateCategoryDto input = new() { Name = "Updated Category" };
        Category category = new(categoryId, "Old Category");
        CategoryDto categoryDto = new() { Id = categoryId, Name = input.Name };

        _mockCategoryRepo.GetAsync(categoryId).Returns(Task.FromResult(category));
        _mockObjectMapper.Map<Category, CategoryDto>(category).Returns(categoryDto);

        // Act
        CategoryDto result = await _categoryAppService.UpdateAsync(categoryId, input);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(input.Name);

        await _mockCategoryRepo.Received(1).GetAsync(categoryId);
        await _mockCategoryManager.Received(1).UpdateAsync(category, input.Name);
        await _mockCategoryRepo.Received(1).UpdateAsync(category, autoSave: true);
    }

    [Fact]
    public async Task DeleteAsync_Should_Call_Manager()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        Category category = new(categoryId, "Category to Delete");
        _mockCategoryRepo.GetAsync(categoryId).Returns(Task.FromResult(category));

        // Act
        await _categoryAppService.DeleteAsync(categoryId);

        // Assert
        await _mockCategoryRepo.Received(1).GetAsync(categoryId);
        await _mockCategoryManager.Received(1).DeleteAsync(category);
    }
}
