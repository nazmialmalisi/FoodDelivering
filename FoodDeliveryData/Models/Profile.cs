using System;
using System.Collections.Generic;

namespace FoodDeliveryData.Models
{
    public partial class Profile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Address { get; set; }
        public string? Phone { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
