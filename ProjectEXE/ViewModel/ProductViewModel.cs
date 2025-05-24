using ProjectEXE.Models;

namespace ProjectEXE.ViewModel
{
    public class ProductViewModel
    {
        public int productId { get; set; }
        public string productName { get; set; }
        public string category { get; set; }
        public int? categoryId { get; set; }
        public string shop { get; set; }
        public int? shopId { get; set; }
        public decimal price { get; set; }
        public bool isVisible { get; set; }
        public string? imageUrl { get; set; }
    }
}
