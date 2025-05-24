namespace ProjectEXE.ViewModel.ServicePackages
{
    public class PaymentCompletionViewModel
    {
        public int PaymentId { get; set; }
        public string TransactionCode { get; set; }
        public decimal Amount { get; set; }
        public string PackageName { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
