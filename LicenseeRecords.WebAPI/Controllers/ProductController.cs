using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly DataService _dataService;

    public ProductController(DataService dataService)
    {
        _dataService = dataService;
    }

    // GET: api/product
    [HttpGet]
    public ActionResult<List<Product>> GetProducts()
    {
        var products = _dataService.GetProducts();
        if (products == null || !products.Any())
        {
            return NotFound("No products found.");
        }
        return Ok(products);
    }

    // GET: api/product/{id}
    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        var product = _dataService.GetProducts().FirstOrDefault(p => p.ProductId == id);
        if (product == null)
            return NotFound($"Product with ID {id} not found.");
        
        return Ok(product);
    }

    // POST: api/product
    [HttpPost]
    public ActionResult<Product> CreateProduct([FromBody] Product product)
    {
        if (product == null || string.IsNullOrWhiteSpace(product.ProductName))
        {
            return BadRequest("Invalid product data.");
        }

        var products = _dataService.GetProducts();
        product.ProductId = products.Max(p => p.ProductId) + 1; // Generate new ID
        products.Add(product);

        try
        {
            _dataService.SaveProducts(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
    }

    // PUT: api/product/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, [FromBody] Product updatedProduct)
    {
        if (updatedProduct == null || string.IsNullOrWhiteSpace(updatedProduct.ProductName))
        {
            return BadRequest("Invalid product data.");
        }

        var products = _dataService.GetProducts();
        var index = products.FindIndex(p => p.ProductId == id);

        if (index == -1)
            return NotFound($"Product with ID {id} not found.");

        updatedProduct.ProductId = id; // Preserve the ID
        products[index] = updatedProduct;

        try
        {
            _dataService.SaveProducts(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return NoContent();
    }

    // DELETE: api/product/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        var products = _dataService.GetProducts();
        var productToRemove = products.FirstOrDefault(p => p.ProductId == id);

        if (productToRemove == null)
            return NotFound($"Product with ID {id} not found.");

        products.Remove(productToRemove);

        try
        {
            _dataService.SaveProducts(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return NoContent();
    }
}