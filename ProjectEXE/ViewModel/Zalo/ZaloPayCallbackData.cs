namespace ProjectEXE.ViewModel.Zalo
{
    public class ZaloPayCallbackData
    {
        public string AppId { get; set; }
        public string AppTransId { get; set; }
        public long ZpTransId { get; set; }
        public long Amount { get; set; }
        public long ServerTime { get; set; }
        public int Status { get; set; }
        public string Mac { get; set; }
        public string EmbedData { get; set; }
        public string Items { get; set; }
    }
}
