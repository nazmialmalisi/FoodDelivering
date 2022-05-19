using FoodDeliveryData.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "MANAGER" , "BUYER" })]
        public IQueryable<Order> GetOrders([Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            // check manager role ?
            var managerRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role && o.Value == "MANAGER").FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                if (managerRole != null)
                    return context.Orders.Where(o=>o.IsDeleted == false).Include(o=>o.OrderDetails);

                var orders = context.Orders.Where(o => o.UserId == user.Id && o.IsDeleted == false).Include(o=>o.OrderDetails);
                return orders.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }
        [Authorize]
        public IQueryable<CourierData> GetCourier([Service] FoodDeliveringContext context)
        {
            var users = context.Users.Where(u=>u.Role == "COURIER" && u.IsDeleted == false).Include(o=>o.Couriers).ToList();

            List<CourierData> couriers = new();
            foreach(var user in users)
            {
                foreach(var courier in user.Couriers)
                {
                    couriers.Add(new CourierData
                    {
                        Id = courier.Id,
                        FullName = user.FullName,
                    });
                }
            }

            return couriers.AsQueryable();
        }

        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<OrderTracking> GetOrderTrackings([Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            List<OrderTracking> orderTrackings = new();
            var username = claimsPrincipal.Identity.Name;

            var user = context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return orderTrackings.AsQueryable();

            var orders = context.Orders.Where(o=>o.UserId == user.Id).Include(o=>o.OrderTrackings).ToList();
            
            foreach(var order in orders)
            {
                if (order.Status == StatusOrder.OnDelivery)
                {
                    foreach (var orderTracking in order.OrderTrackings)
                    {
                        orderTrackings.Add(orderTracking);
                    }
                }
            }
            return orderTrackings.AsQueryable();
        }
    }
}
