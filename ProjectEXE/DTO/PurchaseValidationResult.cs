namespace ProjectEXE.DTO
{
    public class PurchaseValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public static PurchaseValidationResult Success()
            => new PurchaseValidationResult { IsValid = true };

        public static PurchaseValidationResult Fail(string errorMessage)
            => new PurchaseValidationResult { IsValid = false, ErrorMessage = errorMessage };
    }
}
