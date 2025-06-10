namespace ProjectEXE.ViewModel.OrderViewModel
{
    public class OrderDetailsViewModel
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string ProductImageUrl { get; set; }
        public string SellerName { get; set; }
        public string ShopName { get; set; }
        public string ShopContactInfo { get; set; }
        public string BuyerName { get; set; }
        public string BuyerPhoneNumber { get; set; }
        public string BuyerAddress { get; set; }
        public string OrderStatus { get; set; }
        public int OrderStatusId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CancelReason { get; set; }
        public bool CanBeCancelled => OrderStatusId == 1; // Chỉ được hủy khi đang "Chờ xác nhận"

        // THÊM CÁC THUỘC TÍNH MỚI CHO VOUCHER
        public string VoucherCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PayAmount { get; set; }
        public bool HasVoucher => !string.IsNullOrEmpty(VoucherCode);
        public int VoucherDiscountPercent { get; set; }
        public DateOnly? VoucherExpiryDate { get; set; }
    }
}
