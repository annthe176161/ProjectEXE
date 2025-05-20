using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel;

namespace ProjectEXE.Controllers
{
    public class ShopController : Controller
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IShopService _shopService;
        public ShopController(ICloudinaryService cloudinaryService, IShopService shopService)
        {
            _cloudinaryService = cloudinaryService;
            _shopService = shopService;
        }

        public IActionResult ActiveShop()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ActiveShop(ShopViewModel shopModel)
        {
            string imageUrl = await _cloudinaryService.UploadImageAsync(shopModel.ImagePath);
            if(await _shopService.ActiveShop(shopModel, imageUrl))
            {
                TempData["Success"] = "Chuyển đổi thành người bán thành công";
            }
            else
            {
                TempData["Error"] = "Tên Shop đã tồn tại, hãy chọn tên khác";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
