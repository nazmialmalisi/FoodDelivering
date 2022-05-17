using FoodDeliveryData.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UserService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "MANAGER" , "ADMIN"})]
        public IQueryable<User> GetUsers([Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            // check admin role ?
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role && o.Value == "ADMIN" || o.Value == "MANAGER").FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName && o.IsDeleted == false).FirstOrDefault();
            if (user != null)
            {
                if(adminRole==null) return new List<User>().AsQueryable();

                if (adminRole.Value.Equals("ADMIN"))
                {
                    return context.Users.Where(u=>u.IsDeleted == false);
                }
                else if(adminRole.Value.Equals("MANAGER"))
                {
                    return context.Users.Where(u => u.Role == "COURIER" && u.IsDeleted == false);
                }
            }

            return new List<User>().AsQueryable();
        }

        [Authorize]
        public IQueryable<Profile> GetProfiles([Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            // check admin role ?
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role && o.Value == "ADMIN").FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName && o.IsDeleted == false).FirstOrDefault();
            if (user != null)
            {
                if (adminRole != null)
                {
                    return context.Profiles;
                }
                var profiles = context.Profiles.Where(o => o.UserId == user.Id);
                return profiles.AsQueryable();
            }

            return new List<Profile>().AsQueryable();
        }
    }
}
