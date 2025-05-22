namespace ProjectEXE.ViewModel.ProductViewModel
{
    public class PaginationViewModel
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; } = 6;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)ItemsPerPage);
    }
}
