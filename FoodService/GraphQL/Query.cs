using FoodDeliveryData.Models;
using HotChocolate.AspNetCore.Authorization;

namespace FoodService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "MANAGER","BUYER" })]
        public IQueryable<Food> GetFoods([Service] FoodDeliveringContext context) =>
            context.Foods.Where(f=>f.IsDeleted == false);
    }
}
