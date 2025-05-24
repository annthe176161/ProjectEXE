using System;
namespace ProjectEXE.ViewModel
{
    public class RecentPackagePaymentDto
    {
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string PackageName { get; set; }
        public decimal PricePaid { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }

        public int PaymentId { get; set; }  
    }
}
