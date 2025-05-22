using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.ProductViewModel;

namespace ProjectEXE.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(ProductFilterViewModel filter, int page = 1)
        {
            // Nếu filter chưa được khởi tạo, đặt giá trị mặc định
            if (filter == null)
            {
                filter = new ProductFilterViewModel();
            }

            // Lấy danh sách danh mục và điều kiện cho bộ lọc
            filter.Categories = await _productService.GetAllCategoriesAsync();
            filter.Conditions = await _productService.GetAllConditionsAsync();

            // Lấy danh sách sản phẩm theo bộ lọc và phân trang
            var viewModel = await _productService.GetProductListAsync(filter, page);

            return View(viewModel);
        }

        public async Task<IActionResult> ProductList([FromQuery] List<int> SelectedCategoryIds, ProductFilterViewModel filter, int page = 1)
        {
            if (filter == null)
            {
                filter = new ProductFilterViewModel();
            }

            // Gán giá trị đã chọn từ query string vào filter
            if (SelectedCategoryIds != null && SelectedCategoryIds.Any())
            {
                filter.SelectedCategoryIds = SelectedCategoryIds;
            }

            // Lấy danh sách danh mục và điều kiện cho bộ lọc
            filter.Categories = await _productService.GetAllCategoriesAsync();
            filter.Conditions = await _productService.GetAllConditionsAsync();

            // Lấy danh sách sản phẩm theo bộ lọc và phân trang
            // Truyền rõ giá trị 6 cho pageSize
            var viewModel = await _productService.GetProductListAsync(filter, page, 6);

            return View(viewModel);
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _productService.GetProductDetailByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy sản phẩm liên quan
            product.RelatedProducts = await _productService.GetRelatedProductsAsync(id, product.Category, 4);

            return View(product);
        }
    }
}
