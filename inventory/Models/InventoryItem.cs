namespace inventory.Models;
using Swashbuckle.AspNetCore.Annotations;

public class Inventory
{
    [SwaggerSchema(ReadOnly = true)] 
    public int Id { get; set; }
    public int UsedSpace { get; set; }
    public int TotalSpace { get; set; }
}

public class Products
{
    [SwaggerSchema(ReadOnly = true)] 
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int InventoryId { get; set; }
}

public class ProductDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int InventoryId { get; set; }
}

public class AddInventoryDto
{
    public int TotalSpace { get; set; }
}

public class UpdateTotalSpaceDto
{
    public int InventoryId { get; set; }
    public int NewTotalSpace { get; set; }
}

public class Categories
{
    [SwaggerSchema(ReadOnly = true)] 
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static implicit operator bool(Categories? v)
    {
        throw new NotImplementedException();
    }
}

public class Product_Categories
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
}

public class ProductCategoryDto
{
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int InventoryId { get; set; }
}