namespace WebApiTeaShopManageMent.Models
{
    public class Order
    {
        public int MenuId { get; set; }
        public int OrderId { get; set; }   // Not string
        public string? CustomerName { get; set; }  // new
        public string? Item { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
    }

}
