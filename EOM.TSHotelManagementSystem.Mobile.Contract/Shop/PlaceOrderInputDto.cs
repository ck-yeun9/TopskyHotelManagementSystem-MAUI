using System;
using System.Collections.Generic;

namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class PlaceOrderInputDto
    {
        public string RoomNumber { get; set; }
        public string DeliveryMethod { get; set; } = "RoomDelivery";
        public DateTime? DeliveryTime { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
    }
}
