using Microsoft.AspNetCore.Mvc;
using StoreManagementAPI.Models;
using StoreManagementAPI.Services;
using System.Net;

namespace StoreManagementAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] string customerId = "")
        {
            List<Order> orders;

            if (!string.IsNullOrEmpty(customerId))
                orders = await _orderService.GetByCustomerId(customerId);
            else
                orders = await _orderService.GetAllOrders();

            if (orders == null || orders.Count == 0)
                return NotFound(new ApiResponse<Order>(StatusCodes.Status404NotFound, "Not Found", new List<Order>()));

            return Ok(new ApiResponse<Order>(StatusCodes.Status200OK, "Success", orders));
        }

        [HttpGet("total")]
        public IActionResult GetTotalOrder()
        {
            return Ok(new
            {
                code = HttpStatusCode.OK,
                message = "Success",
                data = new List<long> { _orderService.GetTotalOrder() }
            });
        }
    }
}
