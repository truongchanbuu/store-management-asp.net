using Microsoft.AspNetCore.Mvc;
using StoreManagementAPI.Models;
using StoreManagementAPI.Models.RequestSchemas;
using StoreManagementAPI.Services;
using System.Net;
using System.Text.RegularExpressions;

namespace StoreManagementAPI.Controllers
{
    [Route("api/customer")]
    [ApiController]
    public class CustomerController : Controller
    {
        private readonly CustomerService _customerService;
        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers([FromQuery] string phone = "", [FromQuery] string name = "")
        {
            List<Customer> customers = new List<Customer>();

            if (!string.IsNullOrEmpty(name))
                customers = await _customerService.SearchByName(name);
            else
                customers = await _customerService.GetAllCustomers();

            if (!string.IsNullOrEmpty(phone))
                customers.RemoveAll(customer => !customer.Phone.Equals(phone));

            return Ok(new ApiResponse<Customer>(StatusCodes.Status200OK, "Get all customers success", customers));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(string id)
        {
            List<Customer> customers = new List<Customer>();
            var customer = await _customerService.GetCustomerById(id);
            if (customer == null)
            {
                return NotFound(new ApiResponse<Customer>(StatusCodes.Status404NotFound, "Customer not found", customers));
            }

            customers.Add(customer);
            return Ok(new ApiResponse<Customer>(StatusCodes.Status200OK, "Get customer by id success", customers));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            customer.Point = 0.0;

            bool isCreated = await _customerService.CreateCustomer(customer);

            if (!isCreated)
            {
                return BadRequest(new ApiResponse<Customer>(StatusCodes.Status400BadRequest, "Failed to add", new List<Customer> { customer }));
            }

            return Ok(new ApiResponse<Customer>(StatusCodes.Status201Created, "Success", new List<Customer> { customer }));
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateCustomer([FromRoute] string id, [FromBody] UpdateCustomerRequest updateCustomerRequest)
        {
            Customer customer = new Customer
            {
                Name = updateCustomerRequest.Name,
                Phone = updateCustomerRequest.Phone,
                Email = updateCustomerRequest.Email,
                Point = updateCustomerRequest.Point
            };  
            Customer customerData = await _customerService.GetCustomerById(id);

            if (customerData == null)
                return Ok(new ApiResponse<Customer>(StatusCodes.Status404NotFound, "Not Found", new List<Customer>()));

            if (string.IsNullOrEmpty(customer.Name))
                return BadRequest(new ApiResponse<Customer>(StatusCodes.Status400BadRequest, "Name is required", new List<Customer>()));
            

            if (string.IsNullOrEmpty(customer.Phone))
                return BadRequest(new ApiResponse<Customer>(StatusCodes.Status400BadRequest, "Phone is required", new List<Customer>()));

            if (string.IsNullOrEmpty(customer.Email))
                return BadRequest(new ApiResponse<Customer>(StatusCodes.Status400BadRequest, "Email is required", new List<Customer>()));

            if (double.IsNaN(customer.Point) || customer.Point < 0)
                return BadRequest(new ApiResponse<Customer>(StatusCodes.Status400BadRequest, "Invalid point", new List<Customer>()));

            if (!Regex.IsMatch(customer.Email, @"^\w+@\w+\.\w+$"))
                return BadRequest(new ApiResponse<Customer>(StatusCodes.Status400BadRequest, "Invalid email", new List<Customer>()));

            customerData.Name = customer.Name;
            customerData.Phone = customer.Phone;
            customerData.Email = customer.Email;
            customerData.Point = customer.Point;

            bool isUpdated = await _customerService.UpdateCustomer(id, customerData);

            if (!isUpdated)
                return BadRequest(new ApiResponse<Customer>(StatusCodes.Status400BadRequest, "Failed to update", new List<Customer>()));

            return Ok(new ApiResponse<Customer>(StatusCodes.Status200OK, "Success", new List<Customer> { customerData }));
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            Customer customer = await _customerService.GetCustomerById(id);

            if (customer == null)
                return Ok(new ApiResponse<Customer>(StatusCodes.Status404NotFound, "Not Found", new List<Customer>()));

            bool isDeleted = await _customerService.DeleteCustomer(id);

            if (!isDeleted)
                return BadRequest(new ApiResponse<Customer>(StatusCodes.Status400BadRequest, "Failed to delete", new List<Customer> { customer }));

            return Ok(new ApiResponse<Customer>(StatusCodes.Status200OK, "Success", new List<Customer> { customer }));
        }

        [HttpGet("total")]
        public IActionResult GetTotalCustomer()
        {
            return Ok(new
            {
                code = HttpStatusCode.OK,
                message = "Success",
                data = new List<long> { _customerService.GetTotalCustomer() }
            });
        }
    }
}
