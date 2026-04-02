namespace DoAnMonLTWeb.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public string Phone { get; set; }

        public string ShippingAddress { get; set; }

        public string Notes { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; } = "Pending";

        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
