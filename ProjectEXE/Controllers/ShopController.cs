using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Implementations;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.Shop;
using ProjectEXE.ViewModel.ShopViewModel;
using System.Composition;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectEXE.Controllers
{
    //[Authorize]
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;
        private readonly ICloudinaryService _cloudinaryService;

        public ShopController(IShopService shopService, ICloudinaryService cloudinaryService)
        {
            _shopService = shopService;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IActionResult> Details(int id, int page = 1)
        {
            var shopViewModel = await _shopService.GetShopWithProductsAsync(id, page);

            if (shopViewModel == null)
            {
                return NotFound();
            }

            return View("ShopView", shopViewModel);
        }

        public ActionResult ActiveShop(int id)
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ActiveShop(ShopView shopModel)
        {
            string imageUrl = await _cloudinaryService.UploadImageAsync(shopModel.ImagePath);
            if (await _shopService.ActiveShop(shopModel, imageUrl))
            {
                TempData["Success"] = "Bạn đã kích hoạt gian hàng thành công!";
            }
            else
            {
                TempData["Error"] = "Tên Shop đã tồn tại, hãy chọn tên khác";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> AddNewProduct()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //lấy id của shop
            int shopId = await _shopService.GetShopIdByUserId(userId);
            //check shop đã đăng ký hay chưa
            if (shopId == null)
            {
                TempData["Error"] = "Shop của bạn chưa được kích hoạt, vui lòng kích hoạt";
                return RedirectToAction("ActiveShop", "Shop");
            }
            //check hạn gói đăng ký dịch vụ của shop
            if (!await _shopService.CheckExpiryDate(shopId))
            {
                TempData["Error"] = "Shop của bạn đã hết hạn gói dịch vụ, vui lòng gia hạn để tiếp tục sử dụng dịch vụ!!";
                return RedirectToAction("Index", "Package");
            }
            //kiểm tra xem còn số lượng thêm sản phẩm theo gói dịch vụ hay không
            if (!await _shopService.CanAddProductAsync(shopId))
            {
                TempData["Error"] = "Bạn đã đạt giới hạn sản phẩm của gói hiện tại. Nâng cấp gói để đăng thêm sản phẩm và tăng doanh thu!";
                return RedirectToAction("Index", "Package");
            }
            var viewModel = await _shopService.GetProductFormDataAsync(null, shopId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewProduct(ProductFormViewModel model)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //lấy id của shop
            int shopId = await _shopService.GetShopIdByUserId(userId);

            bool result = await _shopService.CreateProductAsync(model, shopId);
            if (result)
            {
                TempData["Success"] = "Sản phẩm đã được tạo thành công!";
                return RedirectToAction("Index", "ShopProfile");
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi tạo sản phẩm. Vui lòng thử lại.";
            }

            // Nếu có lỗi, tải lại dữ liệu cho form
            model.Categories = await _shopService.GetCategoriesAsync();
            model.Conditions = await _shopService.GetConditionsAsync();
            return View("AddNewProduct", model);
        }
    }
}
