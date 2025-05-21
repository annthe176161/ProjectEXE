namespace ProjectEXE.ViewModel.ServicePackageViewModel
{
    public class PaymentInformationViewModel
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public decimal Amount { get; set; }
        public string TransactionCode { get; set; }
        public string AdminQRCode { get; set; }
        public string PaymentInstruction { get; set; }
        public string AdminBankAccount { get; set; }
        public string AdminBankName { get; set; }
        public int ShopId { get; set; }
    }
}
