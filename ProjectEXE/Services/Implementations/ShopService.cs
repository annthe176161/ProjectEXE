using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel;

namespace ProjectEXE.Services.Implementations
{
    public class ShopService : IShopService
    {
        private readonly RevaContext _context;
        public ShopService(RevaContext context)
        {
            _context = context;
        }

        public async Task<bool> ActiveShop(ShopViewModel shop, string imageUrl)
        {
            if (_context.Shops.Any(n => n.ShopName == shop.ShopName))
            {
                return false;   
            }
            var shopModel = new Shop
            {
                UserId = 1, //tạm thời lấy user = 1
                ShopName = shop.ShopName,
                Description = shop.Description,
                ProfileImage = imageUrl,
                ContactInfo = shop.ContactInfo,
                CreatedAt = DateTime.Now,
            };

            await _context.Shops.AddAsync(shopModel);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
