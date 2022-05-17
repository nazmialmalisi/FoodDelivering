using System;
using System.Collections.Generic;

namespace FoodDeliveryData.Models
{
    public partial class OrderTracking
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        public virtual Order? Order { get; set; }
    }
}
