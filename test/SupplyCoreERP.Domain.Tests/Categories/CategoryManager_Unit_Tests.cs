using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SupplyCoreERP.Products;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

namespace SupplyCoreERP.Categories;

public class CategoryManager_Unit_Tests
{
    private readonly ICategoryRepository _mockCategoryRepo;
    private readonly IRepository<Product, Guid> _mockProductRepo;
    private readonly IGuidGenerator _mockGuidGenerator;
    private readonly CategoryManager _categoryManager;

    public CategoryManager_Unit_Tests()
    {
        // --- ARRANGE (Mocking dependencies) ---
        _mockCategoryRepo = Substitute.For<ICategoryRepository>();
        _mockProductRepo = Substitute.For<IRepository<Product, Guid>>();
        _mockGuidGenerator = Substitute.For<IGuidGenerator>();

        // Khởi tạo Domain Service (DI - Constructor Injection)
        _categoryManager = new CategoryManager(_mockCategoryRepo, _mockProductRepo, _mockGuidGenerator);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidName_ShouldCreateCategory()
    {
        // Arrange
        string categoryName = "New Category";
        var expectedGuid = Guid.NewGuid();

        _mockGuidGenerator.Create().Returns(expectedGuid);
        _mockCategoryRepo.IsNameExistsAsync(categoryName).Returns(Task.FromResult(false));

        // Act
        Category result = await _categoryManager.CreateAsync(categoryName);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(categoryName);
        result.Id.ShouldBe(expectedGuid);

        await _mockCategoryRepo.Received(1).IsNameExistsAsync(categoryName);
        _mockGuidGenerator.Received(1).Create();
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ShouldThrowException()
    {
        // Arrange
        string duplicateName = "Existing Category";
        _mockCategoryRepo.IsNameExistsAsync(duplicateName).Returns(Task.FromResult(true));

        // Act & Assert
        UserFriendlyException exception = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _categoryManager.CreateAsync(duplicateName);
        });

        exception.Message.ShouldContain("đã tồn tại");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ValidInput_ShouldUpdateName()
    {
        // Arrange
        var category = new Category(Guid.NewGuid(), "Old Name");
        string newName = "New Name";

        _mockCategoryRepo.IsNameExistsAsync(newName.Trim(), category.Id).Returns(Task.FromResult(false));

        // Act
        await _categoryManager.UpdateAsync(category, newName);

        // Assert
        category.Name.ShouldBe(newName.Trim());
        await _mockCategoryRepo.Received(1).IsNameExistsAsync(newName.Trim(), category.Id);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ShouldThrowException()
    {
        // Arrange
        var category = new Category(Guid.NewGuid(), "Electronics");
        string existingName = "Software";

        _mockCategoryRepo.IsNameExistsAsync(existingName, category.Id).Returns(Task.FromResult(true));

        // Act & Assert
        UserFriendlyException exception = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _categoryManager.UpdateAsync(category, existingName);
        });

        exception.Message.ShouldContain("đã bị sử dụng");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_NoProducts_ShouldDelete()
    {
        // Arrange
        var category = new Category(Guid.NewGuid(), "Electronics");

        _mockProductRepo.AnyAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        // Act
        await _categoryManager.DeleteAsync(category);

        // Assert
        await _mockCategoryRepo.Received(1).DeleteAsync(category);
    }

    [Fact]
    public async Task DeleteAsync_WithProducts_ShouldThrowException()
    {
        // Arrange
        var category = new Category(Guid.NewGuid(), "Electronics");

        _mockProductRepo.AnyAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act & Assert
        UserFriendlyException exception = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _categoryManager.DeleteAsync(category);
        });

        exception.Message.ShouldContain("đang có sản phẩm thuộc nhóm này");

        await _mockCategoryRepo.DidNotReceive().DeleteAsync(Arg.Any<Category>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    #endregion
}
