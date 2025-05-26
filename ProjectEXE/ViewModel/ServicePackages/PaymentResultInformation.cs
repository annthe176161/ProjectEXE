namespace ProjectEXE.ViewModel.ServicePackages
{
    public class PaymentResultInformation
    {
        public int ProductLimit { get; set; }
        public string PackageName { get; set; }
        public string TransactionCode { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

    }
}
