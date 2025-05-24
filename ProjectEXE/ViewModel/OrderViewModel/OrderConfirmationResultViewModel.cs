namespace ProjectEXE.ViewModel.OrderViewModel
{
    public class OrderConfirmationResultViewModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? OrderId { get; set; }
    }
}
