using FoodDeliveryData.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserService.GraphQL
{
    public class Mutation
    {
        public async Task<UserData> RegisterUserAsync(RegisterUser input, [Service] FoodDeliveringContext context)
        {
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData
                {
                    Messagee = "Username Sudah Ada!"
                }); ;
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Role = input.Role,
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) //Encrypt password
            };

            var role = context.Roles.Where(r=>r.Name == input.Role).FirstOrDefault();
            if (role == null)
            {
                return await Task.FromResult(new UserData
                {
                    Messagee = "Role Tidak Ada!"
                });
            }
            context.Users.Add(newUser);
            await context.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserId = newUser.Id,
                RoleId = role.Id
            };
            context.UserRoles.Add(userRole);

            var profileUser = new Profile
            {
                UserId = newUser.Id,
                FullName = newUser.FullName,
            };
            context.Profiles.Add(profileUser);

            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Role = newUser.Role,
                Email = newUser.Email,
                FullName = newUser.FullName,
                Messagee = "Berhasil Membuat User"
            });
        }
        public async Task<UserToken> LoginAsync(LoginUser input, [Service] IOptions<TokenSettings> tokenSettings, [Service] FoodDeliveringContext context)
        {
            var user = context.Users.Where(o => o.Username == input.Username && o.IsDeleted == false).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            else if (!user.IsVerif)
            {
                return await Task.FromResult(new UserToken(null, null, "Akun Belum Verifikasi, Segera hubungi Admin"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                //Generate JWT Token
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                //JWT Payload
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

                var userRoles = context.UserRoles.Where(o => o.UserId == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.Id == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(12);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims, //JWT Payload
                    signingCredentials: credentials //JWT Signature
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }
        [Authorize]
        public async Task<string> ChangePassword(ChangePassword input, [Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.Identity.Name;

            var user = context.Users.Where(u=>u.Username == username && u.IsDeleted == false).FirstOrDefault();
            
            if (user == null) return "User Tidak Ada!";
            
            bool valid = BCrypt.Net.BCrypt.Verify(input.OldPassword, user.Password);
            if(!valid) return "Password Tidak Cocok";
            
            user.Password = BCrypt.Net.BCrypt.HashPassword(input.NewPassword); //Encrypt password
            context.Update(user);
            await context.SaveChangesAsync();

            return "Berhasil Update Password";
        }
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<string> VerifikasiUser(string username, [Service] FoodDeliveringContext context)
        {
            var user = context.Users.Where(u => u.Username == username && u.IsDeleted == false).FirstOrDefault();

            if (user == null) return "User Tidak Ada!";

            using var transaction = context.Database.BeginTransaction();
            try
            {
                user.IsVerif = true;
                context.Users.Update(user);
                if (user.Role == "COURIER")
                {
                    Courier courier = new Courier
                    {
                        UserId = user.Id,
                    };
                    context.Couriers.Add(courier);
                }
                context.SaveChanges();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return ex.Message.ToString();
            }
            return "Berhasil Verifikasi User";
        }
        [Authorize]
        public async Task<ProfileData> UpdateProfile(ProfileData input,[Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            var user = context.Users.Where(u => u.Username == userName && u.IsDeleted == false).FirstOrDefault();

            if (user == null) return new ProfileData("","","");

            var profile = context.Profiles.Where(p => p.UserId == user.Id).FirstOrDefault();

            profile.FullName = input.FullName;
            profile.Address = input.Address;
            profile.Phone = input.Phone;
            context.Profiles.Update(profile);
            user.FullName = input.FullName;
            context.Users.Update(user);

            await context.SaveChangesAsync();

            return input;
        }
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<string> DeleteUser(int id, [Service] FoodDeliveringContext context)
        {
            var user = context.Users.FirstOrDefault(u => u.Id == id && u.IsDeleted == false);
            if (user == null) return "Data User Tidak Ada!";

            user.IsDeleted = true;
            context.Users.Update(user);

            await context.SaveChangesAsync();

            return "Berhasil Delete Data User!";
        }
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<string> DeleteCourier(int id, [Service] FoodDeliveringContext context)
        {
            var courier = context.Users.
                FirstOrDefault(o=>o.Id == id && o.Role == "COURIER" && o.IsDeleted == false);

            if (courier == null) return "Data Courier Tidak Ada!";

            courier.IsDeleted = true;
            context.Users.Update(courier);

            await context.SaveChangesAsync();

            return "Berhasil Delete Data Courier";
        }
    }
}
