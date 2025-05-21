namespace ProjectEXE.ViewModel.ShopOrderViewModel
{
    public class OrderStatusHistoryViewModel
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public string Notes { get; set; }
    }
}
