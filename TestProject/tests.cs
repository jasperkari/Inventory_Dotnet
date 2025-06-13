using inventory.Data;
using inventory.Models;
using inventory.Repositories;
using inventory.Services;
using Microsoft.EntityFrameworkCore;

namespace inventory.Tests;

public class InventoryTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("RepositoryDb")
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public async Task GetAllItems_ShouldReturnAllInventoryItems()
    {
        var context = CreateInMemoryDbContext();
        var repository = new InventoryRepository(context);

        context.Inventory.Add(new Inventory { Id = 1, TotalSpace = 100, UsedSpace = 50 });
        context.Inventory.Add(new Inventory { Id = 2, TotalSpace = 200, UsedSpace = 80 });
        await context.SaveChangesAsync();

        var result = await repository.GetAllItems();

        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetInventoryById_ShouldReturnCorrectInventoryItem()
    {
        var context = CreateInMemoryDbContext();
        var repository = new InventoryRepository(context);

        var inventoryItem = new Inventory { Id = 1, TotalSpace = 100, UsedSpace = 50 };
        context.Inventory.Add(inventoryItem);
        await context.SaveChangesAsync();

        var result = await repository.GetInventoryById(1);

        Assert.NotNull(result);
        Assert.Equal(1, result?.Id);
    }

    [Fact]
    public async Task UpdateInventory_ShouldUpdateInventoryItem()
    {
        var context = CreateInMemoryDbContext();
        var repository = new InventoryRepository(context);

        var inventoryItem = new Inventory { Id = 1, TotalSpace = 100, UsedSpace = 50 };
        context.Inventory.Add(inventoryItem);
        await context.SaveChangesAsync();

        inventoryItem.TotalSpace = 200;
        repository.UpdateInventory(inventoryItem);
        await context.SaveChangesAsync();

        var updatedItem = await repository.GetInventoryById(1);
        Assert.NotNull(updatedItem);
        Assert.Equal(200, updatedItem.TotalSpace);
    }

    [Fact]
    public async Task DeleteInventory_ShouldDeleteInventory()
    {
        var context = CreateInMemoryDbContext();
        var repository = new InventoryRepository(context);

        var inventoryItem = new Inventory { Id = 1, TotalSpace = 100, UsedSpace = 50 };
        context.Inventory.Add(inventoryItem);
        await context.SaveChangesAsync();

        repository.DeleteInventory(inventoryItem);
        await context.SaveChangesAsync();

        var deletedItem = await repository.GetInventoryById(1);
        Assert.Null(deletedItem);
    }
}

public class ProductTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("ServiceDb")
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public async Task AddOrUpdateProduct_ShouldAddNewUpdateOrDeleteProduct()
    {
        var context = CreateInMemoryDbContext();
        var inventoryRepository = new InventoryRepository(context);
        var productRepository = new ProductRepository(context);
        var service = new ProductService(productRepository, inventoryRepository);

        var inventoryItem = new Inventory { Id = 1, TotalSpace = 100, UsedSpace = 50 };
        context.Inventory.Add(inventoryItem);
        await context.SaveChangesAsync();

        var productDto = new ProductDto
        {
            Name = "Product A",
            Quantity = 10,
            InventoryId = inventoryItem.Id
        };

        var result = await service.AddOrUpdateProduct(productDto);

        Assert.NotNull(result);
        Assert.Equal("Product A", result.Name);
        Assert.Equal(10, result.Quantity);

        var updateProductDto = new ProductDto
        {
            Name = "Product A",
            Quantity = 12,
            InventoryId = inventoryItem.Id
        };

        var updateResult = await service.AddOrUpdateProduct(updateProductDto);

        Assert.NotNull(updateResult);
        Assert.Equal("Product A", updateResult.Name);
        Assert.Equal(22, updateResult.Quantity);

        var reduceProductDto = new ProductDto
        {
            Name = "Product A",
            Quantity = -13,
            InventoryId = inventoryItem.Id
        };

        var reduceResult = await service.AddOrUpdateProduct(reduceProductDto);

        Assert.NotNull(reduceResult);
        Assert.Equal("Product A", reduceResult.Name);
        Assert.Equal(9, reduceResult.Quantity);

        var deleteProductDto = new ProductDto
        {
            Name = "Product A",
            Quantity = -9,
            InventoryId = inventoryItem.Id
        };

        var deleteResult = await service.AddOrUpdateProduct(deleteProductDto);

        Assert.Null(deleteResult);
    }
}

public class CategoryTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("CategoryDb")
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public async Task AddCategory_ShouldAddNewCategory()
    {
        var context = CreateInMemoryDbContext();
        var categoryRepository = new CategoryRepository(context);
        var service = new CategoryService(categoryRepository);

        var category = new Categories { Name = "Category A" };
        var result = await service.AddCategory(category);

        Assert.NotNull(result);
        Assert.Equal("Category A", result.Name);
    }

    [Fact]
    public async Task GetAllCategoryItems_ShouldReturnAllCategories()
    {
        var context = CreateInMemoryDbContext();
        var categoryRepository = new CategoryRepository(context);
        var service = new CategoryService(categoryRepository);

        context.Categories.Add(new Categories { Id = 1, Name = "Category A" });
        context.Categories.Add(new Categories { Id = 2, Name = "Category B" });
        await context.SaveChangesAsync();

        var result = await service.GetAllItems();

        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeleteCategory_ShouldDeleteCategory()
    {
        var context = CreateInMemoryDbContext();
        var categoryRepository = new CategoryRepository(context);
        var service = new CategoryService(categoryRepository);

        var category = new Categories { Id = 1, Name = "Category A" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        await service.RemoveCategory("1");

        var deletedCategory = await categoryRepository.GetCategoryById(1);
    }

    [Fact]
    public async Task DeleteCategoryByName_ShouldDeleteCategory()
    {
        var context = CreateInMemoryDbContext();
        var categoryRepository = new CategoryRepository(context);
        var service = new CategoryService(categoryRepository);

        var category = new Categories { Id = 1, Name = "Category A" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var deletedCategory = await service.RemoveCategoryByName("Category A");

        Assert.NotNull(deletedCategory);
    }

    [Fact]
    public async Task GetCategoryProductsByName_ShouldReturnProducts()
    {
        var context = CreateInMemoryDbContext();
        var categoryRepository = new CategoryRepository(context);
        var service = new CategoryService(categoryRepository);

        var category = new Categories { Id = 1, Name = "Category A" };
        context.Categories.Add(category);

        var inventory = new Inventory { Id = 1, TotalSpace = 100, UsedSpace = 50 };
        context.Inventory.Add(inventory);

        var product = new Products { Id = 1, Name = "Product A", Quantity = 10, InventoryId = inventory.Id };
        context.Products.Add(product);

        var productCategory = new Product_Categories { ProductId = product.Id, CategoryId = category.Id };
        context.Product_Categories.Add(productCategory);

        await context.SaveChangesAsync();

        var result = await service.GetCategoryProductsByName("Category A");
    }
}
