using System;
using System.Threading.Tasks;

namespace SupplyCoreERP.Categories;

public interface ICategoryManager
{
    Task<Category> CreateAsync(string name);
    Task UpdateAsync(Category category, string newName);
    Task DeleteAsync(Category category);
}
