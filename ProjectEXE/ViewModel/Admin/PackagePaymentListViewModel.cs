namespace ProjectEXE.ViewModel.Admin
{
    public class PackagePaymentListViewModel
    {
        public List<PackagePaymentViewModel> PendingPayments { get; set; }
        public List<PackagePaymentViewModel> RecentPayments { get; set; }
        public int TotalPending { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalRejected { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
