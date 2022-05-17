using FoodDeliveryData.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "BUYER" })]
        public async Task<OrderOutput> SubmitOrderAsync(List<OrderInput> inputs,[Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            using var transaction = context.Database.BeginTransaction();

            var userName = claimsPrincipal.Identity.Name;

            try
            {
                var user = context.Users.Where(u => u.Username == userName).FirstOrDefault();

                if (user != null)
                {
                    List<Order> orders = new();
                    foreach (var input in inputs)
                    {
                        Order order = new Order
                        {
                            UserId = user.Id,
                            FoodId = input.FoodId,
                            Quantity = input.Quantity,
                            Status = StatusOrder.Waiting
                        };
                        orders.Add(order);
                    }
                    context.Orders.AddRange(orders);
                    context.SaveChanges();

                    await transaction.CommitAsync();

                    return new OrderOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Berhasil Membuat Order"
                    };
                }
                else
                {
                    throw new Exception("User Tidak Ada");
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new OrderOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = ex.Message
                };
            }
        }
    }
}
