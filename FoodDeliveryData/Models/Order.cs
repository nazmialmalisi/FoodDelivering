using System;
using System.Collections.Generic;

namespace FoodDeliveryData.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
            OrderTrackings = new HashSet<OrderTracking>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourierId { get; set; }
        public string Status { get; set; } = null!;
        public bool IsVerif { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Courier Courier { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<OrderTracking> OrderTrackings { get; set; }
    }
}
