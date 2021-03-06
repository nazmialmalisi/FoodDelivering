using System;
using System.Collections.Generic;

namespace FoodDeliveryData.Models
{
    public partial class User
    {
        public User()
        {
            Couriers = new HashSet<Courier>();
            Orders = new HashSet<Order>();
            Profiles = new HashSet<Profile>();
            UserRoles = new HashSet<UserRole>();
        }

        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsVerif { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Courier> Couriers { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Profile> Profiles { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
