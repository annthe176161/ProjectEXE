namespace ProjectEXE.DTO
{
    public class VoucherResult
    {
        public bool IsSuccess { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string Message { get; set; }
    }

}
