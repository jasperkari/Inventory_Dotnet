using inventory.Services;
using inventory.Models;
using Microsoft.AspNetCore.Mvc;

namespace inventory.Controllers;

[ApiController]
[Route("inventory")]
public class InventoryController(InventoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await service.GetAllItems();
        return Ok(items);
    }

    [HttpGet("{inventoryId}")]
    public async Task<IActionResult> GetProductsByInventoryId(int inventoryId)
    {
        if (inventoryId <= 0)
        {
            return BadRequest(new { message = "Invalid inventory ID." });
        }

        var products = await service.GetInventoryProductsById(inventoryId);

        if (products == null || !products.Any())
        {
            return NotFound(new { message = "No products found for this inventory." });
        }

        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> AddInventory([FromBody] AddInventoryDto dto)
    {
        if (dto == null || dto.TotalSpace <= 0)
        {
            return BadRequest(new { message = "Invalid total space value." });
        }

        var inventory = await service.AddInventory(dto.TotalSpace);
        return CreatedAtAction(nameof(GetAll), new { id = inventory.Id }, inventory);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateTotalSpace([FromBody] UpdateTotalSpaceDto dto)
    {
        if (dto == null || dto.InventoryId <= 0 || dto.NewTotalSpace <= 0)
        {
            return BadRequest(new { message = "Invalid request data." });
        }

        var inventory = await service.UpdateTotalSpace(dto.InventoryId, dto.NewTotalSpace);
        if (inventory == null)
        {
            return BadRequest(new { message = "Invalid total space update." });
        }

        return Ok(inventory);
    }


    [HttpDelete("{inventoryId}")]
    public async Task<IActionResult> RemoveInventory(int inventoryId)
    {
        if (inventoryId <= 0)
        {
            return BadRequest(new { message = "Invalid inventory ID." });
        }

        var result = await service.RemoveInventory(inventoryId);
        if (!result)
        {
            return NotFound(new { message = "Inventory not found." });
        }

        return NoContent();
    }
}

[ApiController]
[Route("product")]
public class ProductController(ProductService service, CategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await service.GetAllItems();
        return Ok(items);
    }

    [HttpPut]
    public async Task<IActionResult> AddOrUpdateProduct([FromBody] ProductDto productDto)
    {
        if (productDto == null)
        {
            return BadRequest("Product data is required.");
        }

        var result = await service.AddOrUpdateProduct(productDto);

        if (result == null)
        {
            return NotFound($"Inventory with ID {productDto.InventoryId} not found.");
        }

        return Ok(result);
    }

    [HttpPost("category")]
    public async Task<IActionResult> AddProductToCategory([FromBody] ProductCategoryDto productCategoryDto)
    {
        if (productCategoryDto == null || string.IsNullOrWhiteSpace(productCategoryDto.ProductName) ||
         string.IsNullOrWhiteSpace(productCategoryDto.CategoryName) ||
         productCategoryDto.InventoryId <= 0)
        {
            return BadRequest("Product name, category name and inventory id are required.");
        }

        var product = await service.GetProductByNameAndInventoryId(productCategoryDto.ProductName, productCategoryDto.InventoryId);
        if (product == null)
        {
            return NotFound("Product not found.");
        }

        var category = await categoryService.GetCategoryByName(productCategoryDto.CategoryName);
        if (category == null)
        {
            return NotFound("Category not found.");
        }

        var result = await categoryService.AddProductToCategory(product.Id, category.Id);

        if (result == null)
        {
            return NotFound("Failed to add product to category.");
        }

        return Ok(result);
    }
}

[ApiController]
[Route("category")]
public class CategoryController(CategoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await service.GetAllItems();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory([FromBody] Categories category)
    {
        if (category == null || string.IsNullOrWhiteSpace(category.Name))
        {
            return BadRequest("Category name is required.");
        }

        var result = await service.AddCategory(category);

        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpDelete("{identifier}")]
    public async Task<IActionResult> RemoveCategory(string identifier)
    {
        try
        {
            var result = await service.RemoveCategory(identifier);
            if (!result)
            {
                return NotFound("Category not found.");
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetCategoryProductsByName(string name)
    {
        var products = await service.GetCategoryProductsByName(name);
        if (products == null || !products.Any())
        {
            return NotFound("No products found in this category.");
        }

        return Ok(products);

    }
}