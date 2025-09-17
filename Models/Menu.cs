namespace WebApiTeaShopManageMent.Models
{
    public class Menu
    {
        public int MenuId { get; set; }   // Not string
        public string? Item { get; set; }
        public decimal Price { get; set; }
    }
}
