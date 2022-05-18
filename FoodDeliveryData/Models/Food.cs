using System;
using System.Collections.Generic;

namespace FoodDeliveryData.Models
{
    public partial class Food
    {
        public Food()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public double Price { get; set; }
        public DateTime Created { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
