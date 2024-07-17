using Microsoft.AspNetCore.Mvc;
using StoreManagementAPI.Services;
using StoreManagementAPI.Models;
using StoreManagementAPI.Models.RequestSchemas;
using System.Text.RegularExpressions;
using System.Net;

namespace StoreManagementAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly OrderProductsService _orderProducstsService;

        public ProductController(ProductService productService, OrderProductsService orderProducstsService)
        {
            _productService = productService;
            _orderProducstsService = orderProducstsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string text = "")
        {
            List<Product> products;
            if (!string.IsNullOrEmpty(text))
                products = await _productService.FindProductByName(text);
            else
                products = await _productService.GetAllProducts();

            if (products == null || !products.Any())
                return Ok(new ApiResponse<Product>(StatusCodes.Status404NotFound,"Not Found",new List<Product>()));

            return Ok(new ApiResponse<Product>(StatusCodes.Status200OK, "Get all products success", products));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById([FromRoute] string id)
        {
            var product = await _productService.GetById(id);

            if (product == null)
                return Ok(new ApiResponse<Product>(StatusCodes.Status404NotFound, "Not Found", new List<Product>()));

            return Ok(new ApiResponse<Product>(StatusCodes.Status200OK, "Get product by id success", new List<Product>() { product }));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if((await _productService.GetByBarcode(product.Barcode)).Any())
                return BadRequest(new ApiResponse<Product>(StatusCodes.Status400BadRequest, "Barcode already exists", new List<Product>()));

            bool isCreated = await _productService.CreateProduct(product);

            if (!isCreated)
                return BadRequest(new ApiResponse<Product>(StatusCodes.Status400BadRequest, "Failed to add", new List<Product>() { product }));

            return Ok(new ApiResponse<Product>(StatusCodes.Status200OK, "Add product success", new List<Product>() { product }));
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] string id, [FromBody] Product product)
        {
            var productInDb = await _productService.GetById(id);

            if (productInDb == null)
                return NotFound(new ApiResponse<Product>(StatusCodes.Status404NotFound, "Product not found", new List<Product>()));

            List<Product> existingProductWithBarcode = await _productService.GetByBarcode(product.Barcode);
            if (existingProductWithBarcode.Count == 1 && !productInDb.Barcode.Equals(product.Barcode))
                return BadRequest(new ApiResponse<Product>(StatusCodes.Status400BadRequest, "Barcode already exists", new List<Product>()));

            productInDb.Name = product.Name;
            productInDb.Barcode = product.Barcode;
            productInDb.RetailPrice = product.RetailPrice;
            productInDb.ImportPrice = product.ImportPrice;
            productInDb.Quantity = product.Quantity;
            productInDb.Category = product.Category;
            productInDb.Illustrator = product.Illustrator;

            bool isUpdated = await _productService.UpdateProduct(id, product);

            if (!isUpdated)
                return BadRequest(new ApiResponse<Product>(StatusCodes.Status400BadRequest, "Failed to update", new List<Product>() { product }));

            return Ok(new ApiResponse<Product>(StatusCodes.Status200OK, "Update product success", new List<Product>() { product }));
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] string id)
        {
            var product = await _productService.GetById(id);

            if (product == null)
                return NotFound(new ApiResponse<Product>(StatusCodes.Status404NotFound, "Product not found", new List<Product>()));

            List<OrderProduct> orderProducts = await _orderProducstsService.GetByProductId(id);
            if (orderProducts.Any())
                return BadRequest(new ApiResponse<Product>(StatusCodes.Status400BadRequest, "Product is in use", new List<Product>() { product }));

            bool isDeleted = await _productService.DeleteProduct(id);

            if (!isDeleted)
                return BadRequest(new ApiResponse<Product>(StatusCodes.Status400BadRequest, "Failed to delete", new List<Product>() { product }));

            return Ok(new ApiResponse<Product>(StatusCodes.Status200OK, "Delete product success", new List<Product>() { product }));
        }

        [HttpGet("total")]
        public IActionResult GetTotalProduct()
        {
            return Ok(new
            {
                code = HttpStatusCode.OK,
                message = "Success",
                data = new List<long> { _productService.GetTotalProduct() }
            });
        }
    }
}
