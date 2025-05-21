namespace ProjectEXE.ViewModel.Zalo
{
    public class ZaloPayOrderResponse
    {
        public int ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public string SubReturnMessage { get; set; }
        public string OrderUrl { get; set; }
        public string ZpTransToken { get; set; }
        public string AppTransId { get; set; }
    }
}
