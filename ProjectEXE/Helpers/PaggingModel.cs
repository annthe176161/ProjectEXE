namespace ProjectEXE.Helpers
{
    public class PaggingModel
    {
        public int CurrentPage { get; set; }
        public int CountPage { get; set; }
        public Func<int?, string> GenerateUrl { get; set; }
    }
}
