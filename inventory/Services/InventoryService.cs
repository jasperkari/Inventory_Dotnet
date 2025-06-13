using System.Runtime.CompilerServices;
using inventory.Models;
using inventory.Repositories;

namespace inventory.Services;

public class InventoryService(InventoryRepository repository)
{
    public async Task<IEnumerable<Inventory>> GetAllItems()
    {
        return await repository.GetAllItems();
    }

    public async Task<Inventory> AddInventory(int totalSpace)
    {
        var inventory = new Inventory
        {
            TotalSpace = totalSpace,
            UsedSpace = 0
        };

        repository.AddInventory(inventory);
        await repository.SaveChangesAsync();

        return inventory;
    }

    public async Task<bool> RemoveInventory(int inventoryId)
    {
        var inventory = await repository.GetInventoryById(inventoryId);
        if (inventory == null)
        {
            return false;
        }

        repository.DeleteInventory(inventory);
        await repository.SaveChangesAsync();
        return true;
    }

    public async Task<Inventory?> UpdateTotalSpace(int inventoryId, int newTotalSpace)
    {
        var inventory = await repository.GetInventoryById(inventoryId);
        if (inventory == null)
        {
            return null;
        }

        if (newTotalSpace < inventory.UsedSpace)
        {
            return null;
        }

        inventory.TotalSpace = newTotalSpace;
        repository.UpdateInventory(inventory);
        await repository.SaveChangesAsync();

        return inventory;
    }

    public async Task<IEnumerable<Products>> GetInventoryProductsById(int inventoryId)
    {
        return await repository.GetInventoryProductsById(inventoryId);
    }
}

public class ProductService(ProductRepository repository, InventoryRepository inventoryRepository)
{
    public async Task<IEnumerable<Products>> GetAllItems()
    {
        return await repository.GetAllItems();
    }

    public async Task<Products?> AddOrUpdateProduct(ProductDto productDto)
    {
        var inventory = await inventoryRepository.GetInventoryById(productDto.InventoryId);

        if (inventory == null)
        {
            return null;
        }

        if (productDto.Quantity < 0)
        {
            var productToReduce = await repository.GetProductByNameAndInventoryId(productDto.Name, productDto.InventoryId);
            if (productToReduce == null)
            {
                return null;
            }

            return await ReduceProductQuantity(productToReduce.Id, -productDto.Quantity);
        }

        var existingProduct = await repository.GetProductByNameAndInventoryId(productDto.Name, productDto.InventoryId);

        if (existingProduct != null)
        {
            existingProduct.Quantity += productDto.Quantity;
            repository.UpdateProduct(existingProduct);

            inventory.UsedSpace += productDto.Quantity;
            inventoryRepository.UpdateInventory(inventory);

            await repository.SaveChangesAsync();
            await inventoryRepository.SaveChangesAsync();
            return existingProduct;
        }
        else
        {
            var product = new Products
            {
                Name = productDto.Name,
                Quantity = productDto.Quantity,
                InventoryId = productDto.InventoryId
            };

            repository.AddProduct(product);

            inventory.UsedSpace += productDto.Quantity;
            inventoryRepository.UpdateInventory(inventory);


            await repository.SaveChangesAsync();
            await inventoryRepository.SaveChangesAsync();

            return product;
        }
    }

    public async Task<Products?> ReduceProductQuantity(int productId, int quantityToRemove)
    {
        var product = await repository.GetProductById(productId);
        if (product == null)
        {
            return null;
        }

        if (product.Quantity <= quantityToRemove)
        {
            repository.DeleteProduct(product);
            var inventory = await inventoryRepository.GetInventoryById(product.InventoryId);
            if (inventory != null)
            {
                inventory.UsedSpace -= product.Quantity;
                inventoryRepository.UpdateInventory(inventory);
                await repository.SaveChangesAsync();
                await inventoryRepository.SaveChangesAsync();
            }

            return null;
        }
        else
        {
            product.Quantity -= quantityToRemove;
            var inventory = await inventoryRepository.GetInventoryById(product.InventoryId);
            if (inventory != null)
            {
                inventory.UsedSpace -= quantityToRemove;
                inventoryRepository.UpdateInventory(inventory);
            }

            repository.UpdateProduct(product);
            await repository.SaveChangesAsync();
            await inventoryRepository.SaveChangesAsync();

            return product;
        }
    }

    public async Task<Products?> GetProductByNameAndInventoryId(string name, int inventoryId)
    {
        var products = await inventoryRepository.GetInventoryProductsById(inventoryId);
        return products.FirstOrDefault(p => p.Name == name);
    }
}

public class CategoryService(CategoryRepository repository)
{
    public async Task<IEnumerable<Categories>> GetAllItems()
    {
        return await repository.GetAllItems();
    }

    public async Task<Categories> AddCategory(Categories category)
    {
        repository.AddCategory(category);
        await repository.SaveChangesAsync();

        return category;
    }

    public async Task<bool> RemoveCategory(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("Category identifier is required.", nameof(identifier));
        }

        bool result;
        if (int.TryParse(identifier, out int categoryId))
        {
            if (categoryId <= 0)
            {
                throw new ArgumentException("Invalid category ID.", nameof(identifier));
            }
            result = await repository.RemoveCategory(categoryId);
        }
        else
        {
            var category = await repository.RemoveCategoryByName(identifier);
            result = category != null;
        }

        return result;
    }

    public async Task<Categories?> RemoveCategoryByName(string name)
    {
        var category = await repository.GetCategoryByName(name);
        if (category == null)
        {
            return null;
        }

        repository.DeleteCategory(category);
        await repository.SaveChangesAsync();
        return category;
    }

    public async Task<Categories?> GetCategoryByName(string name)
    {
        return await repository.GetCategoryByName(name);
    }

    public async Task<Categories?> AddProductToCategory(int productId, int categoryId)
    {
        var productCategory = new Product_Categories
        {
            ProductId = productId,
            CategoryId = categoryId
        };

        await repository.AddProductToCategory(productCategory);
        await repository.SaveChangesAsync();

        return await repository.GetCategoryById(categoryId);
    }

    public async Task<IEnumerable<Products>?> GetCategoryProductsByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        var category = await repository.GetCategoryByName(name);
        if (category == null)
        {
            return null;
        }

        return await repository.GetProductsByCategoryId(category.Id);
    }
}
