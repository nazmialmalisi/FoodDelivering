using FoodDeliveryData.Models;
using HotChocolate.AspNetCore.Authorization;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "BUYER" })]
        public async Task<OrderOutput> SubmitOrderAsync(OrderInput input, [Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            using var transaction = context.Database.BeginTransaction();

            var userName = claimsPrincipal.Identity.Name;

            try
            {
                var user = context.Users.Where(u => u.Username == userName).FirstOrDefault();
                var courier = context.Couriers.Where(u => u.Id == input.CourierId).FirstOrDefault();
                if (courier == null) return new OrderOutput
                {
                    TransactionDate = DateTime.Now.ToString(),
                    Message = "Courier Tidak Ada!"
                };
                if (user != null)
                {
                    Order order = new Order
                    {
                        UserId = user.Id,
                        CourierId = input.CourierId,
                        Status = StatusOrder.Waiting
                    };
                    foreach (var item in input.ListOrderDetails)
                    {
                        OrderDetail detail = new OrderDetail
                        {
                            OrderId = order.Id,
                            FoodId = item.FoodId,
                            Quantity = item.Quantity,
                        };
                        order.OrderDetails.Add(detail);
                    }
                    context.Orders.Add(order);
                    context.SaveChanges();

                    await transaction.CommitAsync();

                    return new OrderOutput
                    {
                        TransactionDate = DateTime.Now.ToString(),
                        Message = "Berhasil Membuat Order!"
                    };
                }
                else
                {
                    throw new Exception("User Tidak Ada!");
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

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<OrderOutput> ConfirmOrderAsync(int id, [Service] FoodDeliveringContext context)
        {
            var order = context.Orders.FirstOrDefault(o => o.Id == id && o.IsDeleted == false);
            if (order == null) return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Order Tidak Ada!"
            };
            if (order.Status != StatusOrder.Waiting) return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Order Tidak Perlu Confirm/Sudah Diantar/Sudah Selesai!"
            };
            
            order.Status = StatusOrder.OnProses;
            order.IsVerif = true;
            context.Orders.Update(order);
            await context.SaveChangesAsync();

            return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Berhasil Confirm Order!"
            };
        }

        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<OrderOutput> AddTrackingOrderAsync(TrackingInput input, [Service] FoodDeliveringContext context)
        {
            OrderOutput orderOutput = new();
            orderOutput.TransactionDate = DateTime.Now.ToString();

            var order = context.Orders.FirstOrDefault(o => o.Id == input.OrderId && o.IsDeleted == false);
            if (order == null)
            {
                orderOutput.Message = "Order Tidak Ada!";
                return orderOutput;
            }
            if (order.Status != StatusOrder.OnProses && order.Status != StatusOrder.OnDelivery)
            {
                orderOutput.Message = "Order Tidak Perlu diantar/Sudah Selesai!";
                return orderOutput;
            }
            
            using var transaction = context.Database.BeginTransaction();
            try
            {
                if (order.Status == StatusOrder.OnProses)
                {
                    OrderTracking orderTracking = new OrderTracking
                    {
                        OrderId = input.OrderId,
                        Latitude = input.Latitude,
                        Longitude = input.Longitude,
                    };
                    context.OrderTrackings.Add(orderTracking);

                    order.Status = StatusOrder.OnDelivery;
                    context.Orders.Update(order);
                    orderOutput.Message = "Tracking ditambah!";
                }
                else if(order.Status == StatusOrder.OnDelivery)
                {
                    var orderTracking = context.OrderTrackings.FirstOrDefault(o => o.OrderId == order.Id);

                    orderTracking.Longitude = input.Longitude;
                    orderTracking.Latitude = input.Latitude;
                    context.OrderTrackings.Update(orderTracking);
                    orderOutput.Message = "Tracking diupdate!";
                }

                context.SaveChanges();
                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine(ex.Message);
            }
            return orderOutput;
        }

        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<OrderOutput> CompleteOrderAsync(int id, [Service] FoodDeliveringContext context)
        {
            var order = context.Orders.FirstOrDefault(o => o.Id == id && o.IsDeleted == false);
            if (order == null) return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Order Tidak Ada!"
            };
            if (order.Status != StatusOrder.OnDelivery) return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Order Belum diambil/Sudah Selesai!"
            };

            order.Status = StatusOrder.Completed;
            context.Orders.Update(order);
            await context.SaveChangesAsync();

            return new OrderOutput
            {
                TransactionDate = DateTime.Now.ToString(),
                Message = "Order diset Selesai!"
            };
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<string> DeleteOrderAsync(int id, [Service] FoodDeliveringContext context)
        {
            var order = context.Orders.FirstOrDefault(o => o.Id == id);

            if (order == null) return "Data Order Tidak Ada!";
            order.IsDeleted = true;
            context.Orders.Update(order);

            await context.SaveChangesAsync();

            return "Berhasil Delete Order";
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<string> UpdateOrderAsync(OrderUpdate input, [Service] FoodDeliveringContext context)
        {
            var order = context.Orders.FirstOrDefault(o=>o.Id == input.Id);
            var courier = context.Couriers.FirstOrDefault(c => c.Id == input.CourierId);

            if (order == null) return "Data Order Tidak Ada!";
            if (courier == null) return "Data Courier Tidak Ada!";
            
            order.CourierId = input.CourierId;
            context.Orders.Update(order);

            await context.SaveChangesAsync();

            return "Berhasil Update Data Order!";
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<string> UpdateOrderDetailAsync(UpdateOrderDetail input, [Service] FoodDeliveringContext context)
        {
            var orderDetail = context.OrderDetails.FirstOrDefault(o => o.Id == input.id);
            var food = context.Foods.FirstOrDefault(f => f.Id == input.FoodId);

            if (orderDetail == null) return "Data Order Detail Tidak Ada!";
            if (food == null) return "Data Food Tidak Ada!";

            orderDetail.FoodId = input.FoodId;
            orderDetail.Quantity = input.Quantity;

            context.OrderDetails.Update(orderDetail);
            await context.SaveChangesAsync();

            return "Berhasil Update Data Order Detail!";
        }
    }
}