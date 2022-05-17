using FoodDeliveryData.Models;
using HotChocolate.AspNetCore.Authorization;

namespace FoodService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Food> AddFoodAsync(FoodInput input, [Service] FoodDeliveringContext context)
        {

            var food = new Food
            {
                Name = input.Name,
                Price = input.Price,
                Created = DateTime.Now
            };

            var ret = context.Foods.Add(food);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        public async Task<Food> GetFoodByIdAsync(int id, [Service] FoodDeliveringContext context)
        {
            var food = context.Foods.Where(o => o.Id == id && o.IsDeleted == false).FirstOrDefault();
            if(food != null) return await Task.FromResult(food);

            return new Food();
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Food> UpdateFoodAsync(FoodInput input, [Service] FoodDeliveringContext context)
        {
            var food = context.Foods.Where(o => o.Id == input.Id && o.IsDeleted == false).FirstOrDefault();
            if (food != null)
            {
                food.Name = input.Name;
                food.Price = input.Price;

                context.Foods.Update(food);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(food);
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Food> DeleteFoodByIdAsync(int id, [Service] FoodDeliveringContext context)
        {
            var food = context.Foods.Where(o => o.Id == id).FirstOrDefault();
            if (food != null)
            {
                food.IsDeleted = true;
                context.Foods.Update(food);
                await context.SaveChangesAsync();   
            }
            return await Task.FromResult(food);
        }
    }
}
