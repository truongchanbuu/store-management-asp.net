using Microsoft.AspNetCore.Mvc;
using StoreManagementAPI.Models;
using StoreManagementAPI.Services;
using System.Net;

namespace StoreManagementAPI.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class PaymentController : Controller
    {
        private readonly AnalyticsService _analyticsService;
        public PaymentController(AnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("sale-results")]
        public IActionResult GetSaleResults(
            [FromQuery(Name = "timeline")] string timeline = "today",
            [FromQuery(Name = "startDate")] DateTime? startDate = null,
            [FromQuery(Name = "endDate")] DateTime? endDate = null)
        {
            AnalyticsReport? analyticsReport = _analyticsService.GetReportByTimeline(timeline, startDate, endDate);
            
            if (analyticsReport == null)
            {
                return BadRequest(new
                {
                    code = HttpStatusCode.OK,
                    message = "There is something wrong..."
                });
            }
            
            return Ok(new
            {
                code = HttpStatusCode.OK,
                message = "Success",
                data = new List<AnalyticsReport> { analyticsReport }
            });
        }
    }
}
