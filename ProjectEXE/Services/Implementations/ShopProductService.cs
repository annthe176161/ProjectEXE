using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.Shop;

namespace ProjectEXE.Services.Implementations
{
    public class ShopProductService : IShopProductService
    {
        private readonly RevaContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ShopProductService(RevaContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<bool> CanCreateProductAsync(int shopId)
        {
            // Kiểm tra xem shop có gói dịch vụ active không và còn slot đăng sản phẩm không
            var activeSubscription = await _context.PackageSubscriptions
                .Where(ps => ps.ShopId == shopId && ps.StatusId == 1 && ps.EndDate > DateTime.Now)
                .Include(ps => ps.Package)
                .FirstOrDefaultAsync();

            if (activeSubscription == null)
                return false;

            var currentProductCount = await _context.Products
                .CountAsync(p => p.ShopId == shopId);

            return currentProductCount < activeSubscription.Package.ProductLimit;
        }

        public async Task<ProductFormViewModel> GetProductFormDataAsync(int? productId, int shopId)
        {
            var viewModel = new ProductFormViewModel
            {
                Categories = await GetCategoriesAsync(),
                Conditions = await GetConditionsAsync()
            };

            if (productId.HasValue)
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.ShopId == shopId);

                if (product != null)
                {
                    viewModel.ProductId = product.ProductId;
                    viewModel.ProductName = product.ProductName;
                    viewModel.Description = product.Description;
                    viewModel.Price = product.Price;
                    viewModel.CategoryId = product.CategoryId;
                    viewModel.ConditionId = product.ConditionId;
                    viewModel.Gender = product.Gender;
                    viewModel.Size = product.Size;
                    viewModel.Color = product.Color;
                    viewModel.Brand = product.Brand;
                    viewModel.Material = product.Material;
                    viewModel.IsInStock = product.IsInStock;
                    viewModel.IsVisible = product.IsVisible;

                    viewModel.ExistingImages = product.ProductImages.Select(pi => new ProductImageViewModel
                    {
                        ImageId = pi.ImageId,
                        ImageUrl = pi.ImageUrl,
                        IsMainImage = pi.IsMainImage
                    }).ToList();

                    var mainImage = product.ProductImages.FirstOrDefault(pi => pi.IsMainImage);
                    if (mainImage != null)
                    {
                        viewModel.MainImageId = mainImage.ImageId;
                    }
                }
            }

            return viewModel;
        }

        public async Task<bool> CreateProductAsync(ProductFormViewModel model, int shopId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var product = new Product
                    {
                        ShopId = shopId,
                        ProductName = model.ProductName,
                        Description = model.Description,
                        Price = model.Price,
                        CategoryId = model.CategoryId,
                        ConditionId = model.ConditionId,
                        Gender = model.Gender,
                        Size = model.Size,
                        Color = model.Color,
                        Brand = model.Brand,
                        Material = model.Material,
                        IsInStock = model.IsInStock,
                        IsVisible = model.IsVisible,
                        CreatedAt = DateTime.Now
                    };

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    // Xử lý upload hình ảnh
                    await ProcessProductImagesAsync(model, product.ProductId);

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }

        public async Task<bool> UpdateProductAsync(ProductFormViewModel model, int shopId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == model.ProductId && p.ShopId == shopId);

                    if (product == null)
                        return false;

                    product.ProductName = model.ProductName;
                    product.Description = model.Description;
                    product.Price = model.Price;
                    product.CategoryId = model.CategoryId;
                    product.ConditionId = model.ConditionId;
                    product.Gender = model.Gender;
                    product.Size = model.Size;
                    product.Color = model.Color;
                    product.Brand = model.Brand;
                    product.Material = model.Material;
                    product.IsInStock = model.IsInStock;
                    product.IsVisible = model.IsVisible;

                    await _context.SaveChangesAsync();

                    // Xử lý xóa hình ảnh cũ nếu cần
                    if (model.ImageIdsToDelete != null && model.ImageIdsToDelete.Any())
                    {
                        var imagesToDelete = await _context.ProductImages
                            .Where(pi => model.ImageIdsToDelete.Contains(pi.ImageId) && pi.ProductId == product.ProductId)
                            .ToListAsync();

                        foreach (var image in imagesToDelete)
                        {
                            // Xóa file vật lý nếu cần
                            DeleteImageFile(image.ImageUrl);
                            _context.ProductImages.Remove(image);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Cập nhật hình ảnh chính nếu có
                    if (model.MainImageId.HasValue)
                    {
                        var productImages = await _context.ProductImages
                            .Where(pi => pi.ProductId == product.ProductId)
                            .ToListAsync();

                        foreach (var image in productImages)
                        {
                            image.IsMainImage = (image.ImageId == model.MainImageId);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Xử lý upload hình ảnh mới
                    await ProcessProductImagesAsync(model, product.ProductId);

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }

        public async Task<bool> DeleteProductAsync(int productId, int shopId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var product = await _context.Products
                        .Include(p => p.ProductImages)
                        .FirstOrDefaultAsync(p => p.ProductId == productId && p.ShopId == shopId);

                    if (product == null)
                        return false;

                    // Xóa các hình ảnh sản phẩm
                    foreach (var image in product.ProductImages)
                    {
                        // Xóa file vật lý
                        DeleteImageFile(image.ImageUrl);
                        _context.ProductImages.Remove(image);
                    }

                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }

        public async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ParentCategoryId = c.ParentCategoryId
                })
                .ToListAsync();
        }

        public async Task<List<ConditionViewModel>> GetConditionsAsync()
        {
            return await _context.ProductConditions
                .Select(c => new ConditionViewModel
                {
                    ConditionId = c.ConditionId,
                    ConditionName = c.ConditionName
                })
                .ToListAsync();
        }

        private async Task ProcessProductImagesAsync(ProductFormViewModel model, int productId)
        {
            if (model.NewImages != null && model.NewImages.Any())
            {
                bool setMainImage = !await _context.ProductImages.AnyAsync(pi => pi.ProductId == productId && pi.IsMainImage);

                foreach (var imageFile in model.NewImages)
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imageUrl = await UploadImageAsync(imageFile, "products");

                        var productImage = new ProductImage
                        {
                            ProductId = productId,
                            ImageUrl = imageUrl,
                            IsMainImage = setMainImage
                        };

                        _context.ProductImages.Add(productImage);

                        // Chỉ set hình đầu tiên làm hình chính (nếu chưa có hình chính)
                        if (setMainImage)
                            setMainImage = false;
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/images/{folderName}/{uniqueFileName}";
        }

        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            try
            {
                // Chuyển URL thành đường dẫn vật lý
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));

                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }
            catch
            {
                // Bỏ qua lỗi khi xóa file
            }
        }
    }
}
