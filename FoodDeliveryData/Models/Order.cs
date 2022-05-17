using System;
using System.Collections.Generic;

namespace FoodDeliveryData.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderTrackings = new HashSet<OrderTracking>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int FoodId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = null!;
        public bool IsVerif { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Food Food { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderTracking> OrderTrackings { get; set; }
    }
}
