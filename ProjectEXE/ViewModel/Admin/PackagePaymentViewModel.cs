namespace ProjectEXE.ViewModel.Admin
{
    public class PackagePaymentViewModel
    {
        public int PaymentId { get; set; }
        public string TransactionCode { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public int? ShopId { get; set; }
        public string ShopName { get; set; }
        public decimal Amount { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public int? SubscriptionId { get; set; }
    }
}
