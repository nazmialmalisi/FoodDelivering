using System;
using System.Collections.Generic;

namespace FoodDeliveryData.Models
{
    public partial class Food
    {
        public Food()
        {
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public double Price { get; set; }
        public DateTime Created { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
