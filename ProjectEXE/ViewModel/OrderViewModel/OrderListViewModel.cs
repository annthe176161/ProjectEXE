namespace ProjectEXE.ViewModel.OrderViewModel
{
    public class OrderListViewModel
    {
        public List<OrderDetailsViewModel> Orders { get; set; } = new List<OrderDetailsViewModel>();
        public int TotalOrders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
