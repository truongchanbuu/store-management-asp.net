using StoreManagementAPI.Models;
using System;
using System.Reflection.Metadata;

namespace StoreManagementAPI.Services
{
    public class AnalyticsService
    {
        private readonly OrderService? _orderService;
        private readonly PaymentService? _paymentService;

        public AnalyticsService(OrderService? orderService, PaymentService? paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
        }

        public AnalyticsReport? GetReportByTimeline(string timeline, DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime now = DateTime.UtcNow.AddHours(+7);
            DateTime start, end;

            switch (timeline.ToLower())
            {
                case "yesterday":
                    start = now.Date.AddDays(-1);
                    end = now.Date;
                    break;
                case "last7days":
                    start = now.Date.AddDays(-6);
                    end = now.Date.AddDays(1);
                    break;
                case "thismonth":
                    start = new DateTime(now.Year, now.Month, 1);
                    end = now.Date.AddDays(1);
                    break;
                case "custom":
                    if (startDate == null || endDate == null)
                        return null;

                    start = startDate.Value.Date;
                    end = endDate.Value.Date.AddDays(1);
                    break;
                default:
                    start = now.Date;
                    end = now.Date.AddDays(1);
                    break;
            }

            List<Payment>? paymentsAtTime = _paymentService?.GetPaymentByBetweenDate(start, end);
            List<Order>? orders = _orderService?.GetOrdersByTimeAndStatus(start, end, null);

            int totalOrders = orders?.Count ?? 0;
            int totalProducts = CalculateTotalProducts(orders ?? new List<Order>());

            double totalAmountReceived = CalculateTotalAmount(paymentsAtTime ?? new List<Payment>(), Status.COMPLETED);

            return new AnalyticsReport
            {
                TotalAmountReceived = totalAmountReceived,
                NumberOfOrders = totalOrders,
                NumberOfProducts = totalProducts,
                Orders = orders
            };
        }

        private int CalculateTotalProducts(List<Order> orders)
        {
            return orders?.Where(order => order.OrderProducts != null)
                          .Sum(order => order.OrderProducts.Count) ?? 0;
        }

        private double CalculateTotalAmount(List<Payment> payments, Status targetStatus = Status.COMPLETED)
        {
            return payments?.Where(payment => payment.Status == targetStatus)
                           .Sum(payment => payment.Amount) ?? 0;
        }

    }
}
