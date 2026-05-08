using NSubstitute;
using Shouldly;
using SupplyCoreERP.Products;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

namespace SupplyCoreERP.Categories
{
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

        [Fact]
        public async Task Should_Create_Category_When_Name_Is_Unique()
        {
            // Arrange
            var categoryName = "New Category";
            var expectedGuid = Guid.NewGuid();
            
            _mockGuidGenerator.Create().Returns(expectedGuid);
            
            // Mock phương thức tùy chỉnh: Trả về false (tên chưa tồn tại)
            _mockCategoryRepo.IsNameExistsAsync(categoryName).Returns(Task.FromResult(false));

            // Act
            var result = await _categoryManager.CreateAsync(categoryName);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe(categoryName);
            result.Id.ShouldBe(expectedGuid);

            // Kiểm tra các Dependency được gọi đúng mong đợi
            await _mockCategoryRepo.Received(1).IsNameExistsAsync(categoryName);
            _mockGuidGenerator.Received(1).Create();
        }

        [Fact]
        public async Task Should_Throw_Exception_When_Creating_Duplicate_Name()
        {
            // Arrange
            var duplicateName = "Existing Category";
            _mockCategoryRepo.IsNameExistsAsync(duplicateName).Returns(Task.FromResult(true));

            // Act & Assert
            var exception = await Should.ThrowAsync<UserFriendlyException>(async () =>
            {
                await _categoryManager.CreateAsync(duplicateName);
            });

            exception.Message.ShouldContain("đã tồn tại");
        }

        [Fact]
        public async Task Should_Not_Allow_Deleting_Category_With_Products()
        {
            // Arrange
            var category = new Category(Guid.NewGuid(), "Electronics");
            
            // Mock AnyAsync: Trả về true (có sản phẩm)
            // Lưu ý: AnyAsync là Extension method, chúng ta dùng Arg.Any để khớp các calls
            _mockProductRepo.AnyAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(true));

            // Act & Assert
            var exception = await Should.ThrowAsync<UserFriendlyException>(async () =>
            {
                await _categoryManager.DeleteAsync(category);
            });

            exception.Message.ShouldContain("đang có sản phẩm thuộc nhóm này");
            
            // Đảm bảo lệnh xóa KHÔNG bao giờ được gọi
            await _mockCategoryRepo.DidNotReceive().DeleteAsync(Arg.Any<Category>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }
    }
}
