namespace ProjectEXE.ViewModel.ProductViewModel
{
    public class ShopBasicInfoViewModel
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public bool IsPremium { get; set; }
        public string ProfileImage { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public int ProductCount { get; set; }
    }
}
