using ProjectEXE.ViewModel;

namespace ProjectEXE.Services.Interfaces
{
    public interface IShopService
    {
        Task<bool> ActiveShop(ShopViewModel shop, string imageUrl);
    }
}
