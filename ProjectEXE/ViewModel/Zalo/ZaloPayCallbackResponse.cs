namespace ProjectEXE.ViewModel.Zalo
{
    public class ZaloPayCallbackResponse
    {
        public int ReturnCode { get; set; }
        public string ReturnMessage { get; set; }

        public static ZaloPayCallbackResponse Success() => new ZaloPayCallbackResponse
        {
            ReturnCode = 1,
            ReturnMessage = "Callback received successfully"
        };

        public static ZaloPayCallbackResponse Fail() => new ZaloPayCallbackResponse
        {
            ReturnCode = -1,
            ReturnMessage = "Invalid MAC or failed processing"
        };
    }
}
