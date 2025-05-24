
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel;

namespace ProjectEXE.Services.Implementations
{

    public class ProductService : IProductService
    {
        private readonly RevaContext _context;
        public ProductService(RevaContext context)
        {
            _context = context;
        }
        public async Task<List<Product>> GetProducts(int page = 1, int limit = 10)
        {
            if (page < 1)
            {
                page = 1;
            }
            int TotalPages = (int)Math.Ceiling(_context.Products.Count() / (double)limit);
            if(TotalPages < page)
            {
                page = TotalPages;
            }
            return await _context.Products.Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.Shop)
            .Skip((page - 1) * limit)
            .Take(limit).ToListAsync();
        }

        public async Task<bool> editProduct(Product p)
        {
            var product = await _context.Products.FindAsync(p.ProductId);
            if (product == null) return false;

            product.ProductName = p.ProductName;
            product.CategoryId = p.CategoryId;
            product.ShopId = p.ShopId;
            product.Price = p.Price;
            product.IsVisible = p.IsVisible;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> getTotalPages(int limit = 10)
        {
            int TotalPages = (int)Math.Ceiling(_context.Products.Count() / (double)limit);
            return TotalPages;
        }

        public async Task<List<ProductViewModel>> GetProductsWithSearch(string search, int page = 1, int limit = 10)
        {
            if (page < 1)
            {
                page = 1;
            }
            int TotalPages = (int)Math.Ceiling(_context.Products.Count() / (double)limit);
            if (TotalPages < page)
            {
                page = TotalPages;
            }
            return await _context.Products.Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.Shop)
            .Where(p => p.ProductName.Contains(search))
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(p => new ProductViewModel
            {
                productId = p.ProductId,
                productName = p.ProductName,
                imageUrl = p.ProductImages.FirstOrDefault().ImageUrl,
                category = p.Category.CategoryName,
                shop = p.Shop.ShopName,
                price = p.Price,
                isVisible = p.IsVisible
            })
            .ToListAsync();
        }

        public async Task<List<CategoryViewModel>> GetCategories()
        {
            return await _context.Categories.Select(c => new CategoryViewModel
            {
                categoryId = c.CategoryId,
                categoryName = c.CategoryName,
            }).ToListAsync();
        }

        public async Task<List<ShopViewModel>> GetShops()
        {
            return await _context.Shops.Select(s => new ShopViewModel
            {
                shopId = s.ShopId,
                shopName = s.ShopName,
            }).ToListAsync();
        }

        public async Task<ProductViewModel> GetProductById(int id)
        {
            return await _context.Products.Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.Shop)
            .Where(p => p.ProductId == id)
            .Select(p => new ProductViewModel
            {
                productId = p.ProductId,
                productName = p.ProductName,
                imageUrl = p.ProductImages.FirstOrDefault().ImageUrl,
                category = p.Category.CategoryName,
                categoryId = p.CategoryId,
                shopId = p.ShopId,
                shop = p.Shop.ShopName,
                price = p.Price,
                isVisible = p.IsVisible
            }).FirstOrDefaultAsync();
        } 

        public async Task<bool> deleteProductById(int id)
        {
            var item = _context.Products.Find(id);
            if (item != null)
            {
                _context.Products.Remove(item);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<int> getTotalPagesWithSearch(string search, int limit = 10)
        {
            int TotalPages = (int)Math.Ceiling(_context.Products.Where(p => p.ProductName.Contains(search)).Count() / (double)limit);
            return TotalPages;
        }
    }
}