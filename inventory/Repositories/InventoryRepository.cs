using inventory.Data;
using inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace inventory.Repositories;

public class InventoryRepository(AppDbContext context)
{
    public async Task<IEnumerable<Inventory>> GetAllItems() => await context.Inventory.ToListAsync();

    public async Task<Inventory?> GetInventoryById(int inventoryId)
    {
        return await context.Inventory
            .FirstOrDefaultAsync(i => i.Id == inventoryId);
    }

    public void UpdateInventory(Inventory inventory)
    {
        context.Inventory.Update(inventory);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void AddInventory(Inventory inventory)
    {
        context.Inventory.Add(inventory);
    }

    public void DeleteInventory(Inventory inventory)
    {
        context.Inventory.Remove(inventory);
    }

    public async Task<IEnumerable<Products>> GetInventoryProductsById(int inventoryId)
    {
        return await context.Products
            .Where(p => p.InventoryId == inventoryId)
            .ToListAsync();
    }
}

public class ProductRepository(AppDbContext context)
{
    public async Task<IEnumerable<Products>> GetAllItems() => await context.Products.ToListAsync();

    public async Task<Products?> GetProductByNameAndInventoryId(string name, int inventoryId)
    {
        return await context.Products
            .FirstOrDefaultAsync(p => p.Name == name && p.InventoryId == inventoryId);
    }

    public void AddProduct(Products product)
    {
        context.Products.Add(product);
    }

    public void UpdateProduct(Products product)
    {
        context.Products.Update(product);
    }

    public void DeleteProduct(Products product)
    {
        context.Products.Remove(product);
    }

    public async Task<Products?> GetProductById(int productId)
    {
        return await context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }
}

public class CategoryRepository(AppDbContext context)
{
    public async Task<IEnumerable<Categories>> GetAllItems() => await context.Categories.ToListAsync();

    public void AddCategory(Categories category)
    {
        context.Categories.Add(category);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public async Task<Categories?> GetCategoryById(int categoryId)
    {
        return await context.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId);
    }

    public async Task<Categories?> GetCategoryByName(string name)
    {
        return await context.Categories
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public void DeleteCategory(Categories category)
    {
        context.Categories.Remove(category);
    }

    public async Task<Categories?> AddProductToCategory(Product_Categories productCategory)
    {
        context.Product_Categories.Add(productCategory);
        await context.SaveChangesAsync();

        return await context.Categories
            .FirstOrDefaultAsync(c => c.Id == productCategory.CategoryId);
    }

    public async Task<IEnumerable<Products>> GetProductsByCategoryId(int categoryId)
    {
        return await context.Products
            .Where(p => context.Product_Categories.Any(pc => pc.ProductId == p.Id && pc.CategoryId == categoryId))
            .ToListAsync();
    }

    public async Task<bool> RemoveCategory(int categoryId)
    {
        var category = await context.Categories.FindAsync(categoryId);
        if (category == null)
        {
            return false;
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Categories?> RemoveCategoryByName(string name)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        if (category == null)
        {
            return null;
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return category;
    }
}